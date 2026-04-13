using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;
using SharedLibraries.model;

namespace Sublease.Builder
{
    public record CounterpartySubleaseLLC(
        string Fullname,
        string Rnokpp,
        string BankAccount,
        string Address,
        string Director,
        string ShortNameDirector,
        string GroupName,
        CounterpartyStatus Status
    );

    public record CounterpartySubleaseFop(
        string Fullname,
        string Edryofop,
        string BankAccount,
        string Address,
        string ResPerson,
        string ShortName,
        CounterpartyStatus Status
    );

    public record GroupData(
        int NumberGroup,
        string Address,
        double Area,
        string Rnokpp
    );

    public record SubleaseData(
        string ContractNumber,
        DateOnly ContractSigningDate,
        DateOnly AktDate,
        DateOnly ContractEndDate,
        double RentalFee
    );

    public class ContractData
    {
        public CounterpartySubleaseLLC? CounterpartyLLC { get; init; }
        public CounterpartySubleaseFop? CounterpartyFop { get; init; }
        public GroupData Group { get; init; } = null!;
        public SubleaseData Sublease { get; init; } = null!;

        public bool IsFop => CounterpartyFop is not null;
    }

    public class ContractDataBuilder
    {
        private readonly DatabaseModel _context;

        public ContractDataBuilder(DatabaseModel context)
        {
            _context = context;
        }

        public async Task<ContractData> BuildAsync(Guid subleaseId)
        {
            var sublease = await _context.Subleases
                .Include(s => s.Group)
                    .ThenInclude(g => g.RentInfo)
                .Include(s => s.Group)
                    .ThenInclude(g => g.Counterparty)
                .Where(s => s.Id == subleaseId)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException($"Sublease {subleaseId} not found.");

            var group = sublease.Group
                ?? throw new InvalidOperationException("Sublease has no Group.");

            var counterparty = group.Counterparty
                ?? throw new InvalidOperationException("Group has no Counterparty.");

            var rentInfo = (group.RentInfo as SubleaseRentInfo)
                ?? throw new InvalidOperationException("Group does not have SubleaseRentInfo.");

            CounterpartySubleaseLLC? llcData = null;
            CounterpartySubleaseFop? fopData = null;

            switch (counterparty)
            {
                case CounterpartyLLC llc:
                    llcData = new CounterpartySubleaseLLC(
                        llc.Fullname,
                        llc.Rnokpp,
                        llc.BankAccount,
                        llc.Address,
                        llc.Director,
                        llc.ShortNameDirector,
                        llc.GroupName,
                        llc.Status
                    );
                    break;

                case CounterpartyFop fop:
                    fopData = new CounterpartySubleaseFop(
                        fop.Fullname,
                        fop.Edryofop,
                        fop.BankAccount,
                        fop.Address,
                        fop.ResPerson,
                        fop.ShortName,
                        fop.Status
                    );
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown counterparty type: {counterparty.GetType().Name}");
            }

            var groupData = new GroupData(
                group.NumberGroup,
                group.Address,
                group.Area,
                rentInfo.Rnokpp
            );

            var subleaseData = new SubleaseData(
                sublease.ContractNumber,
                sublease.ContractSigningDate,
                sublease.AktDate,
                sublease.ContractEndDate,
                sublease.RentalFee
            );

            return new ContractData
            {
                CounterpartyLLC = llcData,
                CounterpartyFop = fopData,
                Group = groupData,
                Sublease = subleaseData
            };
        }
    }
}