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

        public async Task GetCounterparty(){
            var Counterparty = await _context.Counterparties.ToListAsync();
            return Counterparty;
        }

        public async Task GetCounterpartyById(){
            
        }

        public async Task CreateCounterparty(){
            
        }

        public async Task UpdateCounterparty(){
            
        }

        public async Task DeleteCounterparty(){
            
        }
    }
}