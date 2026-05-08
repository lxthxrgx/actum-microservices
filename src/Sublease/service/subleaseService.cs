using SharedLibraries.Database;
using SharedLibraries.Factory;
using SharedLibraries.model;
using Microsoft.EntityFrameworkCore;
using SharedLibraries.model.dto;

namespace Sublease.service
{
    public class subleaseService
    {
        private readonly DatabaseModel _context;

        public subleaseService(DatabaseModel context)
        {
            _context = context;
        }

        public async Task<IResponse> GetSublease()
        {
            return ResponseFactory.Ok(_context.Subleases.ToList());
        }

        public async Task<IResponse> GetSubleaseById(Guid id)
        {
            var sublease = await _context.Subleases.FindAsync(id);
            if (sublease == null)
            {
                return ResponseFactory.Error("Sublease not found");
            }
            return ResponseFactory.Ok(sublease);
        }

        public async Task<IResponse> CreateSublease(List<subleaseDto> sublease)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                foreach (var item in sublease)
                {
                    var newData = new SubleaseModel
                    {
                        GroupId = item.GroupId,
                        ContractNumber = item.ContractNumber,
                        ContractSigningDate = item.ContractSigningDate,
                        AktDate = item.AktDate,
                        ContractEndDate = item.ContractEndDate,
                        IsContinuation = item.IsContinuation,
                        ContractEndDate2 = item.ContractEndDate2,
                        RentalFee = item.RentalFee,
                        RentalFee2 = item.RentalFee2,
                        Group = null!,
                        Done = item.Done
                    };
                    _context.Subleases.Add(newData);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResponseFactory.Ok(sublease);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> UpdateSublease(List<subleaseDtoResponse> subleases)
        {
            if (subleases == null || !subleases.Any())
            {
                return ResponseFactory.Error("Sublease list cannot be null.");
            }
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var sublease in subleases)
                {
                    var existingSublease = await _context.Subleases.FindAsync(sublease.Id);
                    if (existingSublease == null)
                    {
                        return ResponseFactory.Error($"Sublease with ID {sublease.Id} not found.");
                    }
                    existingSublease.GroupId = sublease.GroupId;
                    existingSublease.ContractNumber = sublease.ContractNumber;
                    existingSublease.ContractSigningDate = sublease.ContractSigningDate;
                    existingSublease.AktDate = sublease.AktDate;
                    existingSublease.ContractEndDate = sublease.ContractEndDate;
                    existingSublease.IsContinuation = sublease.IsContinuation;
                    existingSublease.ContractEndDate2 = sublease.ContractEndDate2;
                    existingSublease.RentalFee = sublease.RentalFee;
                    existingSublease.RentalFee2 = sublease.RentalFee2;
                    existingSublease.Done = sublease.Done;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResponseFactory.Ok("Subleases updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> DeleteSubleases(List<subleaseDtoDelete> subleasesIds)
        {
            if (subleasesIds == null || !subleasesIds.Any())
            {
                return ResponseFactory.Error("Sublease IDs list cannot be null or empty.");
            }
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ids = subleasesIds.Select(g => g.Id).ToList();
                var subleasesToDelete = await _context.Subleases.Where(g => ids.Contains(g.Id)).ToListAsync();
                if (!subleasesToDelete.Any())
                {
                    return ResponseFactory.Error("No subleases found for the provided IDs.");
                }
                _context.Subleases.RemoveRange(subleasesToDelete);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResponseFactory.Ok("Subleases deleted successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }
        public async Task<IResponse> GetSubleaseSummary()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);
                var in30Days = today.AddDays(30);

                var all = await _context.Subleases.ToListAsync();

                var total = all.Count;
                var newThisMonth = all.Count(s => s.ContractSigningDate >= firstDayOfMonth);
                var active = all.Count(s => s.Done == false);
                var expiringIn30 = all.Count(s =>
                                        s.Done == false &&
                                        s.ContractEndDate >= today &&
                                        s.ContractEndDate <= in30Days);
                var done = all.Count(s => s.Done == true);
                var activePercent = total > 0 ? Math.Round((double)active / total * 100, 1) : 0;

                return ResponseFactory.Ok(new subleaseSummaryDto
                {
                    Total = total,
                    NewThisMonth = newThisMonth,
                    Active = active,
                    ActivePercent = activePercent,
                    ExpiringIn30Days = expiringIn30,
                    Done = done,
                });
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }
    }
}
