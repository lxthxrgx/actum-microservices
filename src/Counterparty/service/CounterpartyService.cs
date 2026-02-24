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

            return ResponseFactory.Ok<object>(Counterparty);
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

        public async Task<IResponse> CreateCounterparty(counterpartyDto counterparty)
        {
            if(counterparty == null)
            {
                return ResponseFactory.Error("This Counterparty not found");
            }

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var newCounterpaty = new CounterpartyModel
                {
                    Id = counterparty.Id,
                    CompanyId = counterparty.CompanyId,
                    Fullname = counterparty.Fullname,
                    ShortName = counterparty.ShortName,
                    Address = counterparty.Address,
                    BankAccount = counterparty.BankAccount,
                    ResPerson = counterparty.ResPerson,
                    Phone = counterparty.Phone,
                    Email = counterparty.Email,
                    Status = counterparty.Status
                };

                await _context.Counterparties.AddAsync(newCounterpaty);
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

        public async Task<IResponse> UpdateCounterparty(counterpartyDto counterparty)
        {
            if (counterparty == null)
            {
                return ResponseFactory.Error("This Counterparty empty");
            }

            var user = await _context.Counterparties.FirstOrDefaultAsync(x => x.Id == counterparty.Id);

            if (user == null)
            {
                return ResponseFactory.Error("This Counterparty not found");
            }

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                user.Id = counterparty.Id;
                user.CompanyId = counterparty.CompanyId;
                user.Fullname = counterparty.Fullname;
                user.ShortName = counterparty.ShortName;
                user.Address = counterparty.Address;
                user.BankAccount = counterparty.BankAccount;
                user.ResPerson = counterparty.ResPerson;
                user.Phone = counterparty.Phone;
                user.Email = counterparty.Email;
                user.Status = counterparty.Status;

                _context.Counterparties.Update(user);
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

        public async Task<IResponse> DeleteCounterparty(counterpartyDeleteDto counterparty)
        {
            if (counterparty == null)
            {
                return ResponseFactory.Error("This Counterparty empty");
            }

            var counterpartyToDelete = await _context.Counterparties.FirstOrDefaultAsync(x => x.Id == counterparty.Id);

            if (counterpartyToDelete == null)
            {
                return ResponseFactory.Error("This Counterparty not found");
            }

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Counterparties.Where(x => x.Id == counterparty.Id).ExecuteDeleteAsync();
                await _context.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ResponseFactory.Error($"Error creating counterparty: {ex.Message}");
            }
            return ResponseFactory.Ok<object>(counterparty, "Counterparty deleted");
        }
    }
}