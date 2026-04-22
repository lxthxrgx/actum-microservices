using Microsoft.EntityFrameworkCore;
using SharedLibraries.components;
using SharedLibraries.Database;
using SharedLibraries.model;
using System.Text.RegularExpressions;
using XDoc;

namespace Lease.Factory
{
    // ─── Enums ────────────────────────────────────────────────────────────────

    public enum LeaseType { Sublease, Rent1, Rent2 }
    public enum ContractorType { Tov, Fop }
    public enum ContractDocumentType
    {
        ContractWithActAndAnnex,
        SupplementaryAgreement,
        ReturnAct,
        ExtensionAgreement
    }

    // ─── ContractData ─────────────────────────────────────────────────────────

    /// <summary>
    /// Flat snapshot of all data needed to fill any document template.
    /// Loaded once per request via ContractDataBuilder.
    /// </summary>
    public class ContractData
    {
        public GroupModel Group { get; set; } = null!;
        public SubleaseModel Sublease { get; set; } = null!;
        public CounterpartyLLC? CounterpartyLLC { get; set; }
        public CounterpartyFop? CounterpartyFop { get; set; }

        public bool IsFop => CounterpartyFop != null;

        /// <summary>One of SubleaseRentInfo / RentType1Info / RentType2Info.</summary>
        public GroupRentInfo? RentInfo => Group.RentInfo;
    }

    // ─── Builder ──────────────────────────────────────────────────────────────

    public class ContractDataBuilder
    {
        private readonly DatabaseModel _context;
        public ContractDataBuilder(DatabaseModel context) => _context = context;

        public async Task<ContractData> BuildAsync(Guid subleaseId)
        {
            var sublease = await _context.Subleases
                .Include(s => s.Group)
                    .ThenInclude(g => g.RentInfo)
                .Include(s => s.Group)
                    .ThenInclude(g => g.Counterparty)
                .FirstOrDefaultAsync(s => s.Id == subleaseId)
                ?? throw new InvalidOperationException($"Sublease {subleaseId} not found.");

            if (sublease.Group.Counterparty == null)
            {
                await _context.Entry(sublease.Group)
                    .Reference(g => g.Counterparty)
                    .LoadAsync();
            }

            Console.WriteLine($"Counterparty loaded: {sublease.Group.Counterparty?.GetType().Name}");
            Console.WriteLine($"Address: {(sublease.Group.Counterparty as CounterpartyLLC)?.Address}");
            Console.WriteLine($"ContractEndDate: {sublease.ContractEndDate}");

            var data = new ContractData
            {
                Group = sublease.Group,
                Sublease = sublease
            };

            switch (sublease.Group.Counterparty)
            {
                case CounterpartyLLC llc: data.CounterpartyLLC = llc; break;
                case CounterpartyFop fop: data.CounterpartyFop = fop; break;
                default:
                    throw new InvalidOperationException(
                        $"Unknown counterparty type: {sublease.Group.Counterparty?.GetType().Name}");
            }

            return data;
        }
    }

    // ─── Static helpers ───────────────────────────────────────────────────────

    internal static class DocHelper
    {
        public static string Doublezero(double num) =>
            $"{num:N2}".Replace('.', ',');

        public static string SanitizeName(string name) =>
            new string(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());

        public static string RenameGog(string name)
        {
            var invalid = new string(Path.GetInvalidFileNameChars());
            return Regex.Replace(name, "[" + Regex.Escape(invalid) + "]", "_");
        }

        /// <summary>
        /// Returns "РНОКПП …" or "ЄДРПОУ …" depending on which field is populated
        /// in the SubleaseRentInfo (parent-lessor identifier for sublease documents).
        /// </summary>
        public static string GetSublessorId(SubleaseRentInfo info)
        {
            if (!string.IsNullOrEmpty(info.Rnokpp)) return "РНОКПП " + info.Rnokpp;
            if (!string.IsNullOrEmpty(info.Edrpou)) return "ЄДРПОУ " + info.Edrpou;
            return "_____";
        }

        public static string BuildNaPidType1(RentType1Info info) =>
            $"свідоцтва про право власності серії {info.SeriesCert} " +
            $"номер {info.CertNumber}, виданого {info.Issued}.";

        public static string BuildNaPidType2(RentType2Info info) =>
            $"Витягу з ЄДР речових прав на нерухоме майно про реєстрацію прав та їх обтяжень " +
            $"від {info.Date:dd/MM/yyyy} індексний номер {info.Num}";

        /// <summary>Relative save path: "NumberGroup-GroupName\fileName".</summary>
        public static string SavePath(GroupModel group, string groupName, string fileName) =>
            $"{group.NumberGroup}-{SanitizeName(groupName)}\\{RenameGog(fileName)}";
    }

    // ─── Abstract base ────────────────────────────────────────────────────────

    public abstract class ContractDocument
    {
        protected readonly DatabaseModel _context;
        protected ContractDocument(DatabaseModel context) => _context = context;
        public abstract Task CreateAsync(Guid subleaseId);
    }

    public abstract class LeaseDocument : ContractDocument
    {
        protected LeaseDocument(DatabaseModel context) : base(context) { }

        protected async Task<ContractData> GetDataAsync(Guid subleaseId)
        {
            var data = await new ContractDataBuilder(_context).BuildAsync(subleaseId);
            return data;
        }

        protected XPathProcessor CreateProcessor(string configKey) =>
            new XPathProcessor(ConfigHelper.Configuration[configKey], ConfigHelper.Configuration["CustomSettings:path"]);

        protected void Save(XPathProcessor p, ContractData data, string fileName)
        {
            var groupName = data.CounterpartyLLC?.GroupName
                         ?? data.CounterpartyFop?.GroupName
                         ?? "unknown";
            p.Save(DocHelper.SavePath(data.Group, groupName, fileName));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUBLEASE / TOV
    // ═══════════════════════════════════════════════════════════════════════════

    public class SubleaseContractWithActAndAnnexTov : LeaseDocument
    {
        public SubleaseContractWithActAndAnnexTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC ?? throw new InvalidOperationException("Expected LLC counterparty.");
            Console.WriteLine("llc.Address: ", llc.Address);
            Console.WriteLine("StrokDii: ", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            var rentInfo = data.RentInfo as SubleaseRentInfo;

            // ── Договір ──────────────────────────────────────────────────────
            var dog = CreateProcessor("sublease-tov:sublease-agreement-tov");
            dog.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dog.WriteXmlTree("PIB", llc.Fullname);
            dog.WriteXmlTree("rnokpp", llc.Rnokpp);
            dog.WriteXmlTree("area", data.Group.Area.ToString());
            dog.WriteXmlTree("address_p", data.Group.Address);
            dog.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate);
            dog.WriteXmlTree("address", llc.Address);
            dog.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(llc.BankAccount ?? ""));
            dog.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            dog.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            dog.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            dog.WriteXmlTree("Director", llc.Director);
            dog.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            dog.WriteXmlTree("subleaseDopNum", rentInfo?.RentNumber ?? "____");
            dog.WriteXmlTree("subleaseDopDate", rentInfo?.StartDate.ToString("dd/MM/yyyy") ?? "____");
            dog.WriteXmlTree("subleaseDopName", rentInfo?.Person ?? "____");
            dog.WriteXmlTree("subleaseDopRnokpp", rentInfo != null ? DocHelper.GetSublessorId(rentInfo) : "____");
            dog.WriteXmlTree("subleaseDopStatus", rentInfo?.Edrpou ?? "____");
            dog.Save($"{data.Sublease.ContractNumber}-{llc.GroupName}-договір");

            // ── Акт ──────────────────────────────────────────────────────────
            var akt = CreateProcessor("sublease-tov:sublease-act-tov");
            akt.WriteXmlTree("fullDate", NumToText.NumberMonthToText(data.Sublease.AktDate.ToString("dd/MM/yyyy")));
            akt.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            akt.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            akt.WriteXmlTree("PIB", llc.Fullname);
            akt.WriteXmlTree("rnokpp", llc.Rnokpp);
            akt.WriteXmlTree("area", data.Group.Area.ToString());
            akt.WriteXmlTree("address_p", data.Group.Address);
            akt.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            akt.WriteXmlTree("address", llc.Address);
            akt.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(llc.BankAccount ?? ""));
            akt.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            akt.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            akt.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            akt.WriteXmlTree("Director", llc.Director);
            akt.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            akt.Save($"{data.Sublease.ContractNumber}-{llc.GroupName}-акт");

            // ── Додаток ───────────────────────────────────────────────────────
            var dod = CreateProcessor("sublease-tov:supplement-tov");
            dod.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dod.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dod.WriteXmlTree("address_p", data.Group.Address);
            dod.WriteXmlTree("PIB", llc.Fullname);
            dod.WriteXmlTree("address", llc.Address);
            dod.WriteXmlTree("rnokpp", llc.Rnokpp);
            dod.WriteXmlTree("BanckAccount", llc.BankAccount ?? "");
            dod.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            dod.Save($"{data.Sublease.ContractNumber}-{llc.GroupName}-додаток");
        }
    }

    public class SubleaseSupplementaryAgreementTov : LeaseDocument
    {
        public SubleaseSupplementaryAgreementTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC ?? throw new InvalidOperationException("Expected LLC counterparty.");

            var p = CreateProcessor("sublease-tov:sublease-termination-tov");
            p.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            p.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.AddDays(-1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("OriginalStrokDii", data.Sublease.ContractEndDate.AddDays(1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("PIB", llc.Fullname);
            p.WriteXmlTree("address_p", data.Group.Address);
            p.WriteXmlTree("rnokpp", llc.Rnokpp);
            p.WriteXmlTree("address", llc.Address);
            p.WriteXmlTree("Director", llc.Director);
            p.WriteXmlTree("area", data.Group.Area.ToString());
            p.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("BanckAccount", llc.BankAccount ?? "");
            p.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            Save(p, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-припинення-ТОВ");
        }
    }

    public class SubleaseReturnActTov : LeaseDocument
    {
        public SubleaseReturnActTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC ?? throw new InvalidOperationException("Expected LLC counterparty.");

            var p = CreateProcessor("sublease-tov:sublease-return-act-tov");
            p.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            p.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("OriginalStrokDii", data.Sublease.ContractEndDate.AddDays(1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("fullDate", NumToText.NumberMonthToText(data.Sublease.ContractEndDate.ToString("dd/MM/yyyy")));
            p.WriteXmlTree("PIB", llc.Fullname);
            p.WriteXmlTree("address_p", data.Group.Address);
            p.WriteXmlTree("rnokpp", llc.Rnokpp);
            p.WriteXmlTree("address", llc.Address);
            p.WriteXmlTree("area", data.Group.Area.ToString());
            p.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("BanckAccount", llc.BankAccount ?? "");
            p.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            p.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            p.WriteXmlTree("Director", llc.Director);
            p.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            Save(p, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-повернення-ТОВ");
        }
    }

    public class SubleaseExtensionAgreementTov : LeaseDocument
    {
        public SubleaseExtensionAgreementTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC ?? throw new InvalidOperationException("Expected LLC counterparty.");
            var rentInfo = data.RentInfo as SubleaseRentInfo;

            var p = CreateProcessor("sublease-tov:extension-contract-tov");
            p.WriteXmlTree("ContractNumber", data.Sublease.ContractNumber);
            p.WriteXmlTree("CreationContractDate", data.Sublease.ContractSigningDate.ToString("dd.MM.yyyy"));
            p.WriteXmlTree("CreationDate", data.Sublease.ContractEndDate.ToString("dd.MM.yyyy"));
            p.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate2?.ToString("dd.MM.yyyy") ?? "____");
            p.WriteXmlTree("PipSublessor", llc.Fullname);
            p.WriteXmlTree("rnokppSublessor", llc.Rnokpp);
            p.WriteXmlTree("addressSublessor", llc.Address);
            p.WriteXmlTree("PipDirector", llc.Director);
            p.WriteXmlTree("PipsDirector", llc.ShortNameDirector);
            p.WriteXmlTree("RoomArea", data.Group.Area.ToString());
            p.WriteXmlTree("RoomAreaText", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("RoomAreaAddress", data.Group.Address);
            p.WriteXmlTree("subleaseDopContractNumber", rentInfo?.RentNumber ?? "____");
            p.WriteXmlTree("subleaseDopStartDate", rentInfo?.StartDate.ToString("dd.MM.yyyy") ?? "____");
            p.WriteXmlTree("subleaseDopName", rentInfo?.Person ?? "____");
            p.WriteXmlTree("rnokppSublessorDop", rentInfo != null ? DocHelper.GetSublessorId(rentInfo) : "____");
            p.WriteXmlTree("BanckAccount", llc.BankAccount ?? "");
            Save(p, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-продовження");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUBLEASE / FOP
    // ═══════════════════════════════════════════════════════════════════════════

    public class SubleaseContractWithActAndAnnexFop : LeaseDocument
    {
        public SubleaseContractWithActAndAnnexFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop ?? throw new InvalidOperationException("Expected FOP counterparty.");
            var rentInfo = data.RentInfo as SubleaseRentInfo;

            // NOTE: CounterpartyFop.Rnokpp — add this property to the model if not yet present.

            // ── Договір ──────────────────────────────────────────────────────
            var dog = CreateProcessor("sublease:sublease-agreement");
            dog.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dog.WriteXmlTree("PIB", fop.Fullname);
            dog.WriteXmlTree("edruofop_Data", fop.Edryofop);
            dog.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");   // Rnokpp — add to CounterpartyFop
            dog.WriteXmlTree("area", data.Group.Area.ToString());
            dog.WriteXmlTree("address_p", data.Group.Address);
            dog.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("address", fop.Address);
            dog.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(fop.BankAccount ?? ""));
            dog.WriteXmlTree("PIBS", fop.ResPerson);
            dog.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            dog.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            dog.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            dog.WriteXmlTree("subleaseDopNum", rentInfo?.RentNumber ?? "____");
            dog.WriteXmlTree("subleaseDopDate", rentInfo?.StartDate.ToString("dd/MM/yyyy") ?? "____");
            dog.WriteXmlTree("subleaseDopName", rentInfo?.Person ?? "____");
            dog.WriteXmlTree("subleaseDopRnokpp", rentInfo != null ? DocHelper.GetSublessorId(rentInfo) : "____");
            dog.WriteXmlTree("subleaseDopStatus", rentInfo?.Edrpou ?? "____");
            Save(dog, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-договір-ФОП");

            // ── Акт ──────────────────────────────────────────────────────────
            var akt = CreateProcessor("sublease:sublease-act");
            akt.WriteXmlTree("fullDate", NumToText.NumberMonthToText(data.Sublease.AktDate.ToString("dd/MM/yyyy")));
            akt.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            akt.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            akt.WriteXmlTree("PIB", fop.Fullname);
            akt.WriteXmlTree("edruofop_Data", fop.Edryofop);
            akt.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            akt.WriteXmlTree("area", data.Group.Area.ToString());
            akt.WriteXmlTree("address_p", data.Group.Address);
            akt.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            akt.WriteXmlTree("address", fop.Address);
            akt.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(fop.BankAccount ?? ""));
            akt.WriteXmlTree("PIBS", fop.ResPerson);
            akt.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            akt.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            akt.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            Save(akt, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-акт-ФОП");

            // ── Додаток ───────────────────────────────────────────────────────
            var dod = CreateProcessor("sublease:supplement-fop");
            dod.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dod.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dod.WriteXmlTree("address_p", data.Group.Address);
            dod.WriteXmlTree("PIB", fop.Fullname);
            dod.WriteXmlTree("address", fop.Address);
            dod.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            dod.WriteXmlTree("BanckAccount", fop.BankAccount ?? "");
            dod.WriteXmlTree("PIBS", fop.ResPerson);
            Save(dod, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-додаток-ФОП");
        }
    }

    public class SubleaseSupplementaryAgreementFop : LeaseDocument
    {
        public SubleaseSupplementaryAgreementFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop ?? throw new InvalidOperationException("Expected FOP counterparty.");

            var p = CreateProcessor("sublease:sublease-termination");
            p.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            p.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.AddDays(-1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("OriginalStrokDii", data.Sublease.ContractEndDate.AddDays(1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("PIB", fop.Fullname);
            p.WriteXmlTree("address_p", data.Group.Address);
            p.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            p.WriteXmlTree("address", fop.Address);
            p.WriteXmlTree("edruofop_Data", fop.Edryofop);
            p.WriteXmlTree("area", data.Group.Area.ToString());
            p.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("BanckAccount", fop.BankAccount ?? "");
            p.WriteXmlTree("PIBS", fop.ResPerson);
            Save(p, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-припинення-ФОП");
        }
    }

    public class SubleaseReturnActFop : LeaseDocument
    {
        public SubleaseReturnActFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop ?? throw new InvalidOperationException("Expected FOP counterparty.");

            var p = CreateProcessor("sublease:sublease-return-act");
            p.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            p.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("OriginalStrokDii", data.Sublease.ContractEndDate.AddDays(1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("fullDate", NumToText.NumberMonthToText(data.Sublease.ContractEndDate.ToString("dd/MM/yyyy")));
            p.WriteXmlTree("PIB", fop.Fullname);
            p.WriteXmlTree("address_p", data.Group.Address);
            p.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            p.WriteXmlTree("address", fop.Address);
            p.WriteXmlTree("edruofop_Data", fop.Edryofop);
            p.WriteXmlTree("area", data.Group.Area.ToString());
            p.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("BanckAccount", fop.BankAccount ?? "");
            p.WriteXmlTree("PIBS", fop.ResPerson);
            p.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            p.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            Save(p, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-повернення-ФОП");
        }
    }

    public class SubleaseExtensionAgreementFop : LeaseDocument
    {
        public SubleaseExtensionAgreementFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop ?? throw new InvalidOperationException("Expected FOP counterparty.");
            var rentInfo = data.RentInfo as SubleaseRentInfo;

            var p = CreateProcessor("sublease:extension-contract-fop");
            p.WriteXmlTree("ContractNumber", data.Sublease.ContractNumber);
            p.WriteXmlTree("CreationContractDate", data.Sublease.ContractSigningDate.ToString("dd.MM.yyyy"));
            p.WriteXmlTree("CreationDate", data.Sublease.ContractEndDate.ToString("dd.MM.yyyy"));
            p.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate2?.ToString("dd.MM.yyyy") ?? "____");
            p.WriteXmlTree("PipSublessor", fop.Fullname);
            p.WriteXmlTree("rnokppSublessor", fop.Rnokpp ?? "");
            p.WriteXmlTree("addressSublessor", fop.Address);
            p.WriteXmlTree("Edruofop", fop.Edryofop);
            p.WriteXmlTree("PipsSublessor", fop.ResPerson);
            p.WriteXmlTree("RoomArea", data.Group.Area.ToString());
            p.WriteXmlTree("RoomAreaText", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("RoomAreaAddress", data.Group.Address);
            p.WriteXmlTree("subleaseDopContractNumber", rentInfo?.RentNumber ?? "____");
            p.WriteXmlTree("subleaseDopStartDate", rentInfo?.StartDate.ToString("dd.MM.yyyy") ?? "____");
            p.WriteXmlTree("subleaseDopName", rentInfo?.Person ?? "____");
            p.WriteXmlTree("rnokppSublessorDop", rentInfo != null ? DocHelper.GetSublessorId(rentInfo) : "____");
            p.WriteXmlTree("BanckAccount", fop.BankAccount ?? "");
            Save(p, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-продовження");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENT-1 (свідоцтво про право власності) / TOV
    // ═══════════════════════════════════════════════════════════════════════════

    public class Rent1ContractWithActAndAnnexTov : LeaseDocument
    {
        public Rent1ContractWithActAndAnnexTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC ?? throw new InvalidOperationException("Expected LLC counterparty.");
            var rentInfo = data.RentInfo as RentType1Info ?? throw new InvalidOperationException("Expected RentType1Info.");
            var naPid = DocHelper.BuildNaPidType1(rentInfo);

            // ── Договір ──────────────────────────────────────────────────────
            var dog = CreateProcessor("rent-tov:rent-dog-tov");
            dog.WriteXmlTree("DogovirRent", data.Sublease.ContractNumber);
            dog.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("PIB", llc.Fullname);
            dog.WriteXmlTree("rnokpp", llc.Rnokpp);
            dog.WriteXmlTree("area", data.Group.Area.ToString());
            dog.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            dog.WriteXmlTree("address_p", data.Group.Address);
            dog.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("address", llc.Address);
            dog.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(llc.BankAccount ?? ""));
            dog.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            dog.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            dog.WriteXmlTree("Director", llc.Director);
            dog.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            dog.WriteXmlTree("NaPid", naPid);
            Save(dog, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-договір");

            // ── Акт ──────────────────────────────────────────────────────────
            var akt = CreateProcessor("rent-tov:rent-act-tov");
            akt.WriteXmlTree("fullDate", NumToText.NumberMonthToText(data.Sublease.AktDate.ToString("dd/MM/yyyy")));
            akt.WriteXmlTree("PIB", llc.Fullname);
            akt.WriteXmlTree("rnokpp", llc.Rnokpp);
            akt.WriteXmlTree("address", llc.Address);
            akt.WriteXmlTree("address_p", data.Group.Address);
            akt.WriteXmlTree("Director", llc.Director);
            akt.WriteXmlTree("DogovirRent", data.Sublease.ContractNumber);
            akt.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            akt.WriteXmlTree("area", data.Group.Area.ToString());
            akt.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            akt.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(llc.BankAccount ?? ""));
            akt.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            Save(akt, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-акт");

            // ── Додаток ───────────────────────────────────────────────────────
            var dod = CreateProcessor("sublease-tov:supplement-tov");
            dod.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dod.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dod.WriteXmlTree("address_p", data.Group.Address);
            dod.WriteXmlTree("PIB", llc.Fullname);
            dod.WriteXmlTree("address", llc.Address);
            dod.WriteXmlTree("rnokpp", llc.Rnokpp);
            dod.WriteXmlTree("BanckAccount", llc.BankAccount ?? "");
            dod.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            Save(dod, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-додаток");
        }
    }

    public class Rent1SupplementaryAgreementTov : LeaseDocument
    {
        public Rent1SupplementaryAgreementTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC ?? throw new InvalidOperationException("Expected LLC counterparty.");

            var p = CreateProcessor("rent-tov:rent-prup-tov");
            p.WriteXmlTree("DogovirRent", data.Sublease.ContractNumber);
            p.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.AddDays(-1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("OriginalStrokDii", data.Sublease.ContractEndDate.AddDays(1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("PIB", llc.Fullname);
            p.WriteXmlTree("address_p", data.Group.Address);
            p.WriteXmlTree("rnokpp", llc.Rnokpp);
            p.WriteXmlTree("address", llc.Address);
            p.WriteXmlTree("Director", llc.Director);
            p.WriteXmlTree("area", data.Group.Area.ToString());
            p.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("BanckAccount", llc.BankAccount ?? "");
            p.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            Save(p, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-припинення-ТОВ");
        }
    }

    public class Rent1ReturnActTov : LeaseDocument
    {
        public Rent1ReturnActTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC ?? throw new InvalidOperationException("Expected LLC counterparty.");

            var p = CreateProcessor("rent-tov:rent-return-act-tov");
            p.WriteXmlTree("fullDate", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("PIB", llc.Fullname);
            p.WriteXmlTree("rnokpp", llc.Rnokpp);
            p.WriteXmlTree("address", llc.Address);
            p.WriteXmlTree("Director", llc.Director);
            p.WriteXmlTree("DogovirRent", data.Sublease.ContractNumber);
            p.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("area", data.Group.Area.ToString());
            p.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("address_p", data.Group.Address);
            p.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(llc.BankAccount ?? ""));
            p.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            Save(p, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-повернення");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENT-1 (свідоцтво про право власності) / FOP
    // ═══════════════════════════════════════════════════════════════════════════

    public class Rent1ContractWithActAndAnnexFop : LeaseDocument
    {
        public Rent1ContractWithActAndAnnexFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop ?? throw new InvalidOperationException("Expected FOP counterparty.");
            var rentInfo = data.RentInfo as RentType1Info ?? throw new InvalidOperationException("Expected RentType1Info.");
            var naPid = DocHelper.BuildNaPidType1(rentInfo);

            // ── Договір ──────────────────────────────────────────────────────
            var dog = CreateProcessor("rent-fop:rent-dog-fop");
            dog.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dog.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("PIB", fop.Fullname);
            dog.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            dog.WriteXmlTree("address", fop.Address);
            dog.WriteXmlTree("edruofop_Data", fop.Edryofop);
            dog.WriteXmlTree("area", data.Group.Area.ToString());
            dog.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            dog.WriteXmlTree("address_p", data.Group.Address);
            dog.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            dog.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            dog.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(fop.BankAccount ?? ""));
            dog.WriteXmlTree("PIBS", fop.ResPerson);
            dog.WriteXmlTree("NaPid", naPid);
            // subleaseDopXXX — not applicable for Rent-1; written as blanks
            dog.WriteXmlTree("subleaseDopNum", "____");
            dog.WriteXmlTree("subleaseDopDate", "____");
            dog.WriteXmlTree("subleaseDopName", "____");
            dog.WriteXmlTree("subleaseDopRnokpp", "____");
            Save(dog, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-договір");

            // ── Акт ──────────────────────────────────────────────────────────
            var akt = CreateProcessor("rent-fop:rent-act-fop");
            akt.WriteXmlTree("fullDate", NumToText.NumberMonthToText(data.Sublease.AktDate.ToString("dd/MM/yyyy")));
            akt.WriteXmlTree("PIB", fop.Fullname);
            akt.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            akt.WriteXmlTree("address", fop.Address);
            akt.WriteXmlTree("edruofop_Data", fop.Edryofop);
            akt.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            akt.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            akt.WriteXmlTree("area", data.Group.Area.ToString());
            akt.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            akt.WriteXmlTree("address_p", data.Group.Address);
            akt.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(fop.BankAccount ?? ""));
            akt.WriteXmlTree("PIBS", fop.ResPerson);
            Save(akt, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-акт");

            // ── Додаток ───────────────────────────────────────────────────────
            var dod = CreateProcessor("sublease:supplement-fop");
            dod.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dod.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dod.WriteXmlTree("address_p", data.Group.Address);
            dod.WriteXmlTree("PIB", fop.Fullname);
            dod.WriteXmlTree("address", fop.Address);
            dod.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            dod.WriteXmlTree("BanckAccount", fop.BankAccount ?? "");
            dod.WriteXmlTree("PIBS", fop.ResPerson);
            Save(dod, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-додаток");
        }
    }

    public class Rent1SupplementaryAgreementFop : LeaseDocument
    {
        public Rent1SupplementaryAgreementFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop ?? throw new InvalidOperationException("Expected FOP counterparty.");

            var p = CreateProcessor("rent-fop:rent-prup-fop");
            p.WriteXmlTree("DogovirRent", data.Sublease.ContractNumber);
            p.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.AddDays(-1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("OriginalStrokDii", data.Sublease.ContractEndDate.AddDays(1).ToString("dd/MM/yyyy"));
            p.WriteXmlTree("PIB", fop.Fullname);
            p.WriteXmlTree("address_p", data.Group.Address);
            p.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            p.WriteXmlTree("address", fop.Address);
            p.WriteXmlTree("edruofop_Data", fop.Edryofop);
            p.WriteXmlTree("area", data.Group.Area.ToString());
            p.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("BanckAccount", fop.BankAccount ?? "");
            p.WriteXmlTree("PIBS", fop.ResPerson);
            Save(p, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-припинення-ФОП");
        }
    }

    public class Rent1ReturnActFop : LeaseDocument
    {
        public Rent1ReturnActFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop ?? throw new InvalidOperationException("Expected FOP counterparty.");

            var p = CreateProcessor("rent-fop:rent-return-act-fop");
            p.WriteXmlTree("fullDate", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("PIB", fop.Fullname);
            p.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            p.WriteXmlTree("address", fop.Address);
            p.WriteXmlTree("edruofop_Data", fop.Edryofop);
            p.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            p.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            p.WriteXmlTree("area", data.Group.Area.ToString());
            p.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            p.WriteXmlTree("address_p", data.Group.Address);
            p.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(fop.BankAccount ?? ""));
            p.WriteXmlTree("PIBS", fop.ResPerson);
            Save(p, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-повернення");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RENT-2 (витяг з ЄДР) — NaPid рядок інший, шаблони ті самі що Rent-1
    // ═══════════════════════════════════════════════════════════════════════════

    public class Rent2ContractWithActAndAnnexTov : LeaseDocument
    {
        public Rent2ContractWithActAndAnnexTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC ?? throw new InvalidOperationException("Expected LLC counterparty.");
            var rentInfo = data.RentInfo as RentType2Info ?? throw new InvalidOperationException("Expected RentType2Info.");
            var naPid = DocHelper.BuildNaPidType2(rentInfo);

            var dog = CreateProcessor("rent-tov:rent-dog-tov");
            dog.WriteXmlTree("DogovirRent", data.Sublease.ContractNumber);
            dog.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("PIB", llc.Fullname);
            dog.WriteXmlTree("rnokpp", llc.Rnokpp);
            dog.WriteXmlTree("area", data.Group.Area.ToString());
            dog.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            dog.WriteXmlTree("address_p", data.Group.Address);
            dog.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("address", llc.Address);
            dog.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(llc.BankAccount ?? ""));
            dog.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            dog.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            dog.WriteXmlTree("Director", llc.Director);
            dog.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            dog.WriteXmlTree("NaPid", naPid);
            Save(dog, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-договір");

            var akt = CreateProcessor("rent-tov:rent-act-tov");
            akt.WriteXmlTree("fullDate", NumToText.NumberMonthToText(data.Sublease.AktDate.ToString("dd/MM/yyyy")));
            akt.WriteXmlTree("PIB", llc.Fullname);
            akt.WriteXmlTree("rnokpp", llc.Rnokpp);
            akt.WriteXmlTree("address", llc.Address);
            akt.WriteXmlTree("address_p", data.Group.Address);
            akt.WriteXmlTree("Director", llc.Director);
            akt.WriteXmlTree("DogovirRent", data.Sublease.ContractNumber);
            akt.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            akt.WriteXmlTree("area", data.Group.Area.ToString());
            akt.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            akt.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(llc.BankAccount ?? ""));
            akt.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            Save(akt, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-акт");

            var dod = CreateProcessor("sublease-tov:supplement-tov");
            dod.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dod.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dod.WriteXmlTree("address_p", data.Group.Address);
            dod.WriteXmlTree("PIB", llc.Fullname);
            dod.WriteXmlTree("address", llc.Address);
            dod.WriteXmlTree("rnokpp", llc.Rnokpp);
            dod.WriteXmlTree("BanckAccount", llc.BankAccount ?? "");
            dod.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);
            Save(dod, data, $"{data.Sublease.ContractNumber}-{llc.GroupName}-додаток");
        }
    }

    public class Rent2ContractWithActAndAnnexFop : LeaseDocument
    {
        public Rent2ContractWithActAndAnnexFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop ?? throw new InvalidOperationException("Expected FOP counterparty.");
            var rentInfo = data.RentInfo as RentType2Info ?? throw new InvalidOperationException("Expected RentType2Info.");
            var naPid = DocHelper.BuildNaPidType2(rentInfo);

            var dog = CreateProcessor("rent-fop:rent-dog-fop");
            dog.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dog.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("PIB", fop.Fullname);
            dog.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            dog.WriteXmlTree("address", fop.Address);
            dog.WriteXmlTree("edruofop_Data", fop.Edryofop);
            dog.WriteXmlTree("area", data.Group.Area.ToString());
            dog.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            dog.WriteXmlTree("address_p", data.Group.Address);
            dog.WriteXmlTree("StrokDii", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
            dog.WriteXmlTree("suma", DocHelper.Doublezero(data.Sublease.RentalFee));
            dog.WriteXmlTree("sum_text", NumToText.SumToText(data.Sublease.RentalFee));
            dog.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(fop.BankAccount ?? ""));
            dog.WriteXmlTree("PIBS", fop.ResPerson);
            dog.WriteXmlTree("NaPid", naPid);
            dog.WriteXmlTree("subleaseDopNum", "____");
            dog.WriteXmlTree("subleaseDopDate", "____");
            dog.WriteXmlTree("subleaseDopName", "____");
            dog.WriteXmlTree("subleaseDopRnokpp", "____");
            Save(dog, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-договір");

            var akt = CreateProcessor("rent-fop:rent-act-fop");
            akt.WriteXmlTree("fullDate", NumToText.NumberMonthToText(data.Sublease.AktDate.ToString("dd/MM/yyyy")));
            akt.WriteXmlTree("PIB", fop.Fullname);
            akt.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            akt.WriteXmlTree("address", fop.Address);
            akt.WriteXmlTree("edruofop_Data", fop.Edryofop);
            akt.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            akt.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            akt.WriteXmlTree("area", data.Group.Area.ToString());
            akt.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            akt.WriteXmlTree("address_p", data.Group.Address);
            akt.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(fop.BankAccount ?? ""));
            akt.WriteXmlTree("PIBS", fop.ResPerson);
            Save(akt, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-акт");

            var dod = CreateProcessor("sublease:supplement-fop");
            dod.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            dod.WriteXmlTree("DateTime", data.Sublease.ContractSigningDate.ToString("dd/MM/yyyy"));
            dod.WriteXmlTree("address_p", data.Group.Address);
            dod.WriteXmlTree("PIB", fop.Fullname);
            dod.WriteXmlTree("address", fop.Address);
            dod.WriteXmlTree("rnokpp", fop.Rnokpp ?? "");
            dod.WriteXmlTree("BanckAccount", fop.BankAccount ?? "");
            dod.WriteXmlTree("PIBS", fop.ResPerson);
            Save(dod, data, $"{data.Sublease.ContractNumber}-{fop.GroupName}-додаток");
        }
    }

    // Rent-2 Supplementary + Return reuse Rent-1 logic (same templates, NaPid
    // is only relevant for ContractWithActAndAnnex).
    public class Rent2SupplementaryAgreementTov : Rent1SupplementaryAgreementTov
    {
        public Rent2SupplementaryAgreementTov(DatabaseModel context) : base(context) { }
    }

    public class Rent2ReturnActTov : Rent1ReturnActTov
    {
        public Rent2ReturnActTov(DatabaseModel context) : base(context) { }
    }

    public class Rent2SupplementaryAgreementFop : Rent1SupplementaryAgreementFop
    {
        public Rent2SupplementaryAgreementFop(DatabaseModel context) : base(context) { }
    }

    public class Rent2ReturnActFop : Rent1ReturnActFop
    {
        public Rent2ReturnActFop(DatabaseModel context) : base(context) { }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY
    // ═══════════════════════════════════════════════════════════════════════════

    public static class ContractDocumentFactory
    {
        /// <summary>
        /// Creates the right document class by explicit enum combination.
        /// </summary>
        public static ContractDocument Create(
            LeaseType leaseType,
            ContractorType contractorType,
            ContractDocumentType docType,
            DatabaseModel context)
            => (leaseType, contractorType, docType) switch
            {
                // ── Sublease / TOV ────────────────────────────────────────────
                (LeaseType.Sublease, ContractorType.Tov, ContractDocumentType.ContractWithActAndAnnex)
                    => new SubleaseContractWithActAndAnnexTov(context),
                (LeaseType.Sublease, ContractorType.Tov, ContractDocumentType.SupplementaryAgreement)
                    => new SubleaseSupplementaryAgreementTov(context),
                (LeaseType.Sublease, ContractorType.Tov, ContractDocumentType.ReturnAct)
                    => new SubleaseReturnActTov(context),
                (LeaseType.Sublease, ContractorType.Tov, ContractDocumentType.ExtensionAgreement)
                    => new SubleaseExtensionAgreementTov(context),

                // ── Sublease / FOP ────────────────────────────────────────────
                (LeaseType.Sublease, ContractorType.Fop, ContractDocumentType.ContractWithActAndAnnex)
                    => new SubleaseContractWithActAndAnnexFop(context),
                (LeaseType.Sublease, ContractorType.Fop, ContractDocumentType.SupplementaryAgreement)
                    => new SubleaseSupplementaryAgreementFop(context),
                (LeaseType.Sublease, ContractorType.Fop, ContractDocumentType.ReturnAct)
                    => new SubleaseReturnActFop(context),
                (LeaseType.Sublease, ContractorType.Fop, ContractDocumentType.ExtensionAgreement)
                    => new SubleaseExtensionAgreementFop(context),

                // ── Rent-1 / TOV ──────────────────────────────────────────────
                (LeaseType.Rent1, ContractorType.Tov, ContractDocumentType.ContractWithActAndAnnex)
                    => new Rent1ContractWithActAndAnnexTov(context),
                (LeaseType.Rent1, ContractorType.Tov, ContractDocumentType.SupplementaryAgreement)
                    => new Rent1SupplementaryAgreementTov(context),
                (LeaseType.Rent1, ContractorType.Tov, ContractDocumentType.ReturnAct)
                    => new Rent1ReturnActTov(context),

                // ── Rent-1 / FOP ──────────────────────────────────────────────
                (LeaseType.Rent1, ContractorType.Fop, ContractDocumentType.ContractWithActAndAnnex)
                    => new Rent1ContractWithActAndAnnexFop(context),
                (LeaseType.Rent1, ContractorType.Fop, ContractDocumentType.SupplementaryAgreement)
                    => new Rent1SupplementaryAgreementFop(context),
                (LeaseType.Rent1, ContractorType.Fop, ContractDocumentType.ReturnAct)
                    => new Rent1ReturnActFop(context),

                // ── Rent-2 / TOV ──────────────────────────────────────────────
                (LeaseType.Rent2, ContractorType.Tov, ContractDocumentType.ContractWithActAndAnnex)
                    => new Rent2ContractWithActAndAnnexTov(context),
                (LeaseType.Rent2, ContractorType.Tov, ContractDocumentType.SupplementaryAgreement)
                    => new Rent2SupplementaryAgreementTov(context),
                (LeaseType.Rent2, ContractorType.Tov, ContractDocumentType.ReturnAct)
                    => new Rent2ReturnActTov(context),

                // ── Rent-2 / FOP ──────────────────────────────────────────────
                (LeaseType.Rent2, ContractorType.Fop, ContractDocumentType.ContractWithActAndAnnex)
                    => new Rent2ContractWithActAndAnnexFop(context),
                (LeaseType.Rent2, ContractorType.Fop, ContractDocumentType.SupplementaryAgreement)
                    => new Rent2SupplementaryAgreementFop(context),
                (LeaseType.Rent2, ContractorType.Fop, ContractDocumentType.ReturnAct)
                    => new Rent2ReturnActFop(context),

                _ => throw new ArgumentOutOfRangeException(
                    $"Unknown combination: {leaseType}, {contractorType}, {docType}")
            };

        /// <summary>
        /// Convenience overload — infers LeaseType and ContractorType directly
        /// from a loaded ContractData so the caller doesn't have to map enums.
        /// </summary>
        public static ContractDocument Create(
            ContractData data,
            ContractDocumentType docType,
            DatabaseModel context)
        {
            var leaseType = data.RentInfo?.RentType switch
            {
                GroupRentType.Sublease => LeaseType.Sublease,
                GroupRentType.Type1 => LeaseType.Rent1,
                GroupRentType.Type2 => LeaseType.Rent2,
                _ => throw new InvalidOperationException(
                    $"Cannot map GroupRentType '{data.RentInfo?.RentType}' to LeaseType.")
            };

            var contractorType = data.IsFop ? ContractorType.Fop : ContractorType.Tov;

            return Create(leaseType, contractorType, docType, context);
        }
    }
}