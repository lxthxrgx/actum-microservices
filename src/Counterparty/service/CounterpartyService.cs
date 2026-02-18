using SharedLibraries.model;
using SharedLibraries.Database;
using Microsoft.EntityFrameworkCore;

namespace Counterparty.service
{
    public class CounterpartyService
    {
        private readonly DatabaseModel _context;

        public CounterpartyService(DatabaseModel context){
            _context = context;
        }

        public async Task<List<CounterpartyModel>> GetCounterparty(){
            var Counterparty = await _context.Counterparties.ToListAsync();
            return Counterparty;
        }

        public async Task<CounterpartyModel> GetCounterpartyById(Guid Id){
            var Counterparty = await _context.Counterparties.FindAsync(Id);

            if(Counterparty == null)
            {
                return null;
            }

            return Counterparty;
        }

        public async Task<CounterpartyModel> CreateCounterparty(CounterpartyModel counterparty)
        {
            if(counterparty == null)
            {
                return null;
            }

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Counterparties.AddAsync(counterparty);
                await _context.SaveChangesAsync();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error creating counterparty: {ex.Message}");
            }

            return counterparty;
        }

        public async Task UpdateCounterparty(){
            
        }

        public async Task DeleteCounterparty(){
            
        }
    }
}