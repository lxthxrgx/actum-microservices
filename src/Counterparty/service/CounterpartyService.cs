using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;
using SharedLibraries.Factory;
using SharedLibraries.model;
using SharedLibraries.model.dto;

namespace Counterparty.service
{
    public class CounterpartyService
    {
        private readonly DatabaseModel _context;

        public CounterpartyService(DatabaseModel context){
            _context = context;
        }

        public async Task<IResponse> GetCounterparty(){
            var Counterparty = await _context.Counterparties.ToListAsync();
            if(Counterparty == null)
            {
                return ResponseFactory.Error("No counterparties found");
            }

            return ResponseFactory.Ok(Counterparty);
        }

        public async Task<IResponse> GetCounterpartyById(counterpartyIdDto counterparty)
        {
            var Counterparty = await _context.Counterparties.FindAsync(counterparty.Id);

            if(Counterparty == null)
            {
                return ResponseFactory.Error("This Counterparty not found");
            }

            return ResponseFactory.Ok<object>(Counterparty, "Counterparty");
        }

        public async Task<IResponse> CreateCounterparty(List<counterpartyDto> counterparty)
        {
            if(counterparty == null)
            {
                return ResponseFactory.Error("This Counterparty not found");
            }

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var counterpartyDto in counterparty)
                {
                    var newCounterpaty = new CounterpartyModel
                    {
                        Fullname = counterpartyDto.Fullname,
                        ShortName = counterpartyDto.ShortName,
                        GroupName = counterpartyDto.GroupName,
                        Address = counterpartyDto.Address,
                        BankAccount = counterpartyDto.BankAccount,
                        ResPerson = counterpartyDto.ResPerson,
                        Phone = counterpartyDto.Phone,
                        Email = counterpartyDto.Email,
                        Status = counterpartyDto.Status
                    };

                    await _context.Counterparties.AddAsync(newCounterpaty);
                }
                await _context.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ResponseFactory.Error($"Error creating counterparty: {ex.Message}");
            }

            return ResponseFactory.Ok<object>(counterparty);
        }

        public async Task<IResponse> UpdateCounterparty(List<counterpartyDto> counterparty)
        {
            if (counterparty == null || !counterparty.Any())
                return ResponseFactory.Error("This Counterparty empty");

            var ids = counterparty.Select(x => x.Id).ToList();

            var existing = await _context.Counterparties
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in counterparty)
                {
                    var entity = existing.FirstOrDefault(x => x.Id == item.Id);
                    if (entity == null) continue;

                    entity.Fullname = item.Fullname;
                    entity.ShortName = item.ShortName;
                    entity.GroupName = item.GroupName;
                    entity.Address = item.Address;
                    entity.BankAccount = item.BankAccount;
                    entity.ResPerson = item.ResPerson;
                    entity.Phone = item.Phone;
                    entity.Email = item.Email;
                    entity.Status = item.Status;
                    _context.Counterparties.Update(entity);
                }

                await _context.SaveChangesAsync();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ResponseFactory.Error($"Error creating counterparty: {ex.Message}");
            }

            return ResponseFactory.Ok<object>(counterparty);
        }

        public async Task<IResponse> DeleteCounterparty(List<counterpartyDeleteDto> counterparty)
        {
            if (counterparty == null || !counterparty.Any())
                return ResponseFactory.Error("This Counterparty empty");

            var ids = counterparty.Select(x => x.Id).ToList();

            var existing = await _context.Counterparties
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (!existing.Any())
                return ResponseFactory.Error("Counterparties not found");

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Counterparties.RemoveRange(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error($"Error deleting counterparty: {ex.Message}");
            }

            return ResponseFactory.Ok<object>(counterparty, "Counterparties deleted");
        }
    }
}