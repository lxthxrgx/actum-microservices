using Microsoft.EntityFrameworkCore;
using SharedLibraries.components;
using SharedLibraries.Database;
using Sublease.Builder;
using System.Text.RegularExpressions;
using XDoc;

namespace Lease.Factory
{
    public enum LeaseType { Sublease, DirectLease, FinancialLease }
    public enum ContractorType { Tov, Fop }
    public enum ContractDocumentType
    {
        ContractWithActAndAnnex,
        SupplementaryAgreement,
        ReturnAct,
        ExtensionAgreement
    }

    public abstract class ContractDocument
    {
        protected readonly DatabaseModel _context;

        protected ContractDocument(DatabaseModel context)
        {
            _context = context;
        }

        public abstract Task CreateAsync(Guid subleaseId);
    }

    public abstract class SubleaseDocument : ContractDocument
    {
        protected SubleaseDocument(DatabaseModel context) : base(context) { }

        protected async Task<ContractData> GetDataAsync(Guid subleaseId)
            => await new ContractDataBuilder(_context).BuildAsync(subleaseId);

        protected XPathProcessor CreateProcessor(string configKey)
            => new XPathProcessor(ConfigHelper.Configuration[configKey], "C:\\Dev");

        protected string WriteCommonFields(XPathProcessor processor, ContractData data)
        {
            return "";
        }

        protected void WriteLlcFields(XPathProcessor processor, CounterpartySubleaseLLC llc)
        {
            processor.WriteXmlTree("PIB", llc.Fullname);
            processor.WriteXmlTree("rnokpp", llc.Rnokpp);
            processor.WriteXmlTree("Director", llc.Director);
            processor.WriteXmlTree("address_p", llc.Address);
        }

        protected void WriteFopFields(XPathProcessor processor, CounterpartySubleaseFop fop)
        {
            processor.WriteXmlTree("PIB", fop.Fullname);
            processor.WriteXmlTree("edruofop_Data", fop.Edryofop);
            processor.WriteXmlTree("PIBS", fop.ResPerson);
            processor.WriteXmlTree("address_p", fop.Address);
        }
    }

    // SUBLEASE / TOV
    public class SubleaseContractWithActAndAnnexTov : SubleaseDocument
    {
        public string Doublezero(double num)
        {
            return $"{num:N2}".Replace('.', ',');
        }
        public string SanitizeDirectoryName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(name
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray());
            return sanitized;
        }
        public string RenameGog(string Name)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars());

            string pattern = "[" + Regex.Escape(invalidChars) + "]";

            string output = Regex.Replace(Name, pattern, "_");

            return output;
        }
        public SubleaseContractWithActAndAnnexTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            Console.WriteLine($"CreateAsync called for {subleaseId}");
            var data = await GetDataAsync(subleaseId);
            Console.WriteLine($"data.CounterpartyLLC: {data.CounterpartyLLC?.Fullname}");
            Console.WriteLine($"data.Group: {data.Group?.Address}");
            Console.WriteLine($"data.Sublease: {data.Sublease?.ContractNumber}");

            Console.WriteLine($"CounterpartyLLC is null: {data.CounterpartyLLC == null}");
            Console.WriteLine($"IsFop: {data.IsFop}");

            var llc = data.CounterpartyLLC
                ?? throw new InvalidOperationException("Expected LLC counterparty.");
            Console.WriteLine($"llc ok: {llc.Fullname}");

            var templatePath = ConfigHelper.Configuration["sublease-tov:sublease-agreement-tov"];
            Console.WriteLine($"Template path: {templatePath}");

            var processor = new XPathProcessor(templatePath, "C:\\Dev\\");

            processor.WriteXmlTree("DateTime", DateTime.Now.ToString("dd/MM/yyyy"));
            processor.WriteXmlTree("DogovirSuborendu", data.Sublease.ContractNumber);
            processor.WriteXmlTree("PIB", llc.Fullname);
            processor.WriteXmlTree("rnokpp", llc.Rnokpp);
            processor.WriteXmlTree("area", data.Group.Area.ToString());
            processor.WriteXmlTree("address_p", data.Group.Address);
            processor.WriteXmlTree("BanckAccount", DeleteSpace.Deletespace(llc.BankAccount ?? ""));
            processor.WriteXmlTree("area_text", NumToText.NumberToText(data.Group.Area));
            processor.WriteXmlTree("sum_text", NumToText.SumToText(Convert.ToDouble(data.Sublease.RentalFee)));
            processor.WriteXmlTree("suma", Doublezero(Convert.ToDouble(data.Sublease.RentalFee)));
            processor.WriteXmlTree("Director", llc.Director);
            processor.WriteXmlTree("PIBSDirector", llc.ShortNameDirector);

            string folder = $"C:\\Dev\\{data.Group.NumberGroup}-{SanitizeDirectoryName(llc.GroupName)}";
            string fileNameDog = $"{RenameGog(data.Sublease.ContractNumber)}-{llc.GroupName}-договір";
            string safeFileNameDog = RenameGog(fileNameDog);

            Console.WriteLine($"Saving to: {folder}\\{safeFileNameDog}");
            Directory.CreateDirectory(folder);

            processor.Save($"{data.Group.NumberGroup}-{SanitizeDirectoryName(llc.GroupName)}\\{safeFileNameDog}");
        }
    }

    public class SubleaseSupplementaryAgreementTov : SubleaseDocument
    {
        public SubleaseSupplementaryAgreementTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC
                ?? throw new InvalidOperationException("Expected LLC counterparty.");

            var processor = new XPathProcessor();
            
            WriteLlcFields(processor, llc);
        }
    }

    public class SubleaseReturnActTov : SubleaseDocument
    {
        public SubleaseReturnActTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC
                ?? throw new InvalidOperationException("Expected LLC counterparty.");

            var processor = new XPathProcessor();
            
            WriteLlcFields(processor, llc);
            processor.WriteXmlTree("AktDate", data.Sublease.AktDate.ToString("dd/MM/yyyy"));
        }
    }

    public class SubleaseExtensionAgreementTov : SubleaseDocument
    {
        public SubleaseExtensionAgreementTov(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var llc = data.CounterpartyLLC
                ?? throw new InvalidOperationException("Expected LLC counterparty.");

            var processor = new XPathProcessor();
            
            WriteLlcFields(processor, llc);
            processor.WriteXmlTree("ContractEndDate2", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
        }
    }

    // SUBLEASE / FOP
    public class SubleaseContractWithActAndAnnexFop : SubleaseDocument
    {
        public SubleaseContractWithActAndAnnexFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop
                ?? throw new InvalidOperationException("Expected FOP counterparty.");

            var processor = new XPathProcessor();
            
            WriteFopFields(processor, fop);
        }
    }

    public class SubleaseSupplementaryAgreementFop : SubleaseDocument
    {
        public SubleaseSupplementaryAgreementFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop
                ?? throw new InvalidOperationException("Expected FOP counterparty.");

            var processor = new XPathProcessor();
            
            WriteFopFields(processor, fop);
        }
    }

    public class SubleaseReturnActFop : SubleaseDocument
    {
        public SubleaseReturnActFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop
                ?? throw new InvalidOperationException("Expected FOP counterparty.");

            var processor = new XPathProcessor();
            
            WriteFopFields(processor, fop);
            processor.WriteXmlTree("AktDate", data.Sublease.AktDate.ToString("dd/MM/yyyy"));
        }
    }

    public class SubleaseExtensionAgreementFop : SubleaseDocument
    {
        public SubleaseExtensionAgreementFop(DatabaseModel context) : base(context) { }

        public override async Task CreateAsync(Guid subleaseId)
        {
            var data = await GetDataAsync(subleaseId);
            var fop = data.CounterpartyFop
                ?? throw new InvalidOperationException("Expected FOP counterparty.");

            var processor = new XPathProcessor();
            
            WriteFopFields(processor, fop);
            processor.WriteXmlTree("ContractEndDate2", data.Sublease.ContractEndDate.ToString("dd/MM/yyyy"));
        }
    }

    public static class ContractDocumentFactory
    {
        public static ContractDocument Create(
            LeaseType leaseType,
            ContractorType contractorType,
            ContractDocumentType docType,
            DatabaseModel context)
            => (leaseType, contractorType, docType) switch
            {
                (LeaseType.Sublease, ContractorType.Tov, ContractDocumentType.ContractWithActAndAnnex) => new SubleaseContractWithActAndAnnexTov(context),
                (LeaseType.Sublease, ContractorType.Tov, ContractDocumentType.SupplementaryAgreement) => new SubleaseSupplementaryAgreementTov(context),
                (LeaseType.Sublease, ContractorType.Tov, ContractDocumentType.ReturnAct) => new SubleaseReturnActTov(context),
                (LeaseType.Sublease, ContractorType.Tov, ContractDocumentType.ExtensionAgreement) => new SubleaseExtensionAgreementTov(context),

                (LeaseType.Sublease, ContractorType.Fop, ContractDocumentType.ContractWithActAndAnnex) => new SubleaseContractWithActAndAnnexFop(context),
                (LeaseType.Sublease, ContractorType.Fop, ContractDocumentType.SupplementaryAgreement) => new SubleaseSupplementaryAgreementFop(context),
                (LeaseType.Sublease, ContractorType.Fop, ContractDocumentType.ReturnAct) => new SubleaseReturnActFop(context),
                (LeaseType.Sublease, ContractorType.Fop, ContractDocumentType.ExtensionAgreement) => new SubleaseExtensionAgreementFop(context),

                _ => throw new ArgumentOutOfRangeException(
                    $"Unknown combination: {leaseType}, {contractorType}, {docType}")
            };
    }
}