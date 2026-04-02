using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;
using SharedLibraries.Factory;
using SharedLibraries.model;
using SharedLibraries.model.dto;
using System.Text.RegularExpressions;

namespace Guard.service
{
    public class GuardService
    {
        private readonly DatabaseModel _context;

        public GuardService(DatabaseModel context)
        {
            _context = context;
        }

        public async Task<IResponse> GetGuard()
        {
            try
            {
                var data = await _context.Guards.ToListAsync();
                return ResponseFactory.Ok(data);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> CreateGuard(List<guardDto> guards)
        {
            if (guards == null || !guards.Any())
            {
                return ResponseFactory.Error("Guards list cannot be null.");
            }

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var guard in guards)
                {
                    var newGuard = new GuardModel
                    {
                        GroupId = guard.GroupId,
                        Address = guard.Address,
                        OhronnaComp = guard.OhronnaComp,
                        NumDog = guard.NumDog,
                        NumDog2 = guard.NumDog2,
                        StrokDii = guard.StrokDii,
                        StrokDii2 = guard.StrokDii2,
                        ResPerson = guard.ResPerson,
                        Phone = guard.Phone,
                        Email = guard.Email,
                        GuardNotes = null!,
                        GuardFiles = null!
                    };

                    var entry = _context.Guards.Add(newGuard);
                    entry.Reference(g => g.GuardNotes).IsLoaded = true;
                    entry.Reference(g => g.GuardFiles).IsLoaded = true;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResponseFactory.Ok("Guards created successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> UpdateGuard(List<guardDtoResponse> guards)
        {
            if (guards == null || !guards.Any())
            {
                return ResponseFactory.Error("Guards list cannot be null.");
            }
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var guard in guards)
                {
                    var existingGuard = await _context.Guards.FindAsync(guard.Id);

                    if (existingGuard == null)
                    {
                        return ResponseFactory.Error($"Guard with ID {guard.Id} not found.");
                    }

                    existingGuard.GroupId = guard.GroupId;
                    existingGuard.Address = guard.Address;
                    existingGuard.OhronnaComp = guard.OhronnaComp;
                    existingGuard.NumDog = guard.NumDog;
                    existingGuard.NumDog2 = guard.NumDog2;
                    existingGuard.StrokDii = guard.StrokDii;
                    existingGuard.StrokDii2 = guard.StrokDii2;
                    existingGuard.ResPerson = guard.ResPerson;
                    existingGuard.Phone = guard.Phone;
                    existingGuard.Email = guard.Email;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResponseFactory.Ok("Guards updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> DeleteGuard(List<guardDtoDelete> guards)
        {
            if (guards == null || !guards.Any())
            {
                return ResponseFactory.Error("Guard IDs list cannot be null or empty.");
            }
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ids = guards.Select(g => g.Id).ToList();
                var guardsToDelete = await _context.Guards.Where(g => ids.Contains(g.Id)).ToListAsync();

                if (!guardsToDelete.Any())
                {
                    return ResponseFactory.Error("No guards found for the provided IDs.");
                }

                _context.Guards.RemoveRange(guardsToDelete);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return ResponseFactory.Ok("Guards deleted successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }
    }
}
