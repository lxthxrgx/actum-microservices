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
                        RentalFee = item.RentalFee2,
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
    }
}
