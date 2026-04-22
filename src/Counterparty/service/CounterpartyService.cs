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

        public async Task<IResponse> GetCounterparty()
        {
            var list = await _context.Counterparties.ToListAsync();

            if (list == null || !list.Any())
                return ResponseFactory.Error("No counterparties found");

            var result = list.Select(x =>
            {
                var dto = new counterpartyDto
                {
                    Id = x.Id,
                    Fullname = x.Fullname,
                    ShortName = x.ShortName,
                    GroupName = x.GroupName,
                    Address = x.Address,
                    BankAccount = x.BankAccount,
                    ResPerson = x.ResPerson,
                    Phone = x.Phone,
                    Email = x.Email,
                    Status = x.Status,
                };

                switch (x)
                {
                    case CounterpartyFop fop:
                        dto.Edryofop = fop.Edryofop;
                        dto.Rnokpp = fop.Rnokpp;
                        break;
                    case CounterpartyLLC llc:
                        dto.Rnokpp = llc.Rnokpp;
                        dto.Director = llc.Director;
                        dto.ShortNameDirector = llc.ShortNameDirector;
                        break;
                }

                return dto;
            }).ToList();

            return ResponseFactory.Ok(result);
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
                foreach (var dto in counterparty)
                {
                    CounterpartyModel newCounterparty;
                    Console.WriteLine($">>> Status int value: {(int)dto.Status}");
                    Console.WriteLine($">>> Is Fop: {dto.Status == CounterpartyStatus.Fop}");

                    if (dto.Status == CounterpartyStatus.Fop)
                    {
                        newCounterparty = new CounterpartyFop
                        {
                            Edryofop = dto.Edryofop,
                        };
                    }
                    else
                    {
                        newCounterparty = new CounterpartyLLC
                        {
                            Director = dto.Director,
                            ShortNameDirector = dto.ShortNameDirector
                        };
                    }

                    newCounterparty.Fullname = dto.Fullname;
                    newCounterparty.ShortName = dto.ShortName;
                    newCounterparty.GroupName = dto.GroupName;
                    newCounterparty.Address = dto.Address;
                    newCounterparty.BankAccount = dto.BankAccount;
                    newCounterparty.ResPerson = dto.ResPerson;
                    newCounterparty.Phone = dto.Phone;
                    newCounterparty.Email = dto.Email;
                    newCounterparty.Status = dto.Status;
                    newCounterparty.Rnokpp = dto.Rnokpp;

                    await _context.Counterparties.AddAsync(newCounterparty);
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
                    entity.Rnokpp = item.Rnokpp;

                    if (entity is CounterpartyFop fop)
                    {
                        fop.Edryofop = item.Edryofop;
                    }
                    else if (entity is CounterpartyLLC llc)
                    {
                        llc.Director = item.Director;
                        llc.ShortNameDirector = item.ShortNameDirector;
                    }

                    _context.Counterparties.Update(entity);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error($"Error updating counterparty: {ex.Message}");
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