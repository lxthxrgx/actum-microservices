using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;
using SharedLibraries.Factory;
using SharedLibraries.model.dto;
using SharedLibraries.model;

namespace Groups.service
{
    public class groupsService
    {
        private readonly DatabaseModel _context;

        public groupsService(DatabaseModel context)
        {
            _context = context;
        }

        public async Task<IResponse> GetGroups()
        {
            try
            {
                var data = await _context.Groups.ToListAsync();
                return ResponseFactory.Ok(data);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> CreateGroups(List<GroupDto> groups)
        {
            if (groups == null || !groups.Any())
            {
                return ResponseFactory.Error("Groups list cannot be null.");
            }

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var group in groups)
                {
                    var newGroup = new GroupModel
                    {
                        NumberGroup = group.NumberGroup,
                        CounterpartyId = group.CounterpartyId,
                        CompanyId = group.CompanyId,
                        Address = group.Address,
                        Area = group.Area,
                        IsAlert = group.IsAlert,
                        DateCloseDepartment = group.DateCloseDepartment,
                        Counterparty = null!,
                        Company = null!
                    };

                    var entry = _context.Groups.Add(newGroup);
                    entry.Reference(g => g.Counterparty).IsLoaded = true;
                    entry.Reference(g => g.Company).IsLoaded = true;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResponseFactory.Ok("Groups created successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> UpdateGroups(List<GroupDtoResponse> groups)
        {
            if (groups == null || !groups.Any())
            {
                return ResponseFactory.Error("Groups list cannot be null.");
            }
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var group in groups)
                {
                    var existingGroup = await _context.Groups.FindAsync(group.Id);
                    if (existingGroup == null)
                    {
                        return ResponseFactory.Error($"Group with ID {group.Id} not found.");
                    }
                    existingGroup.NumberGroup = group.NumberGroup;
                    existingGroup.CounterpartyId = group.CounterpartyId;
                    existingGroup.CompanyId = group.CompanyId;
                    existingGroup.Address = group.Address;
                    existingGroup.Area = group.Area;
                    existingGroup.IsAlert = group.IsAlert;
                    existingGroup.DateCloseDepartment = group.DateCloseDepartment;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResponseFactory.Ok("Groups updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> DeleteGroups(List<GroupDtoDelete> groupIds)
        {
            if (groupIds == null || !groupIds.Any())
            {
                return ResponseFactory.Error("Group IDs list cannot be null or empty.");
            }
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ids = groupIds.Select(g => g.Id).ToList();
                var groupsToDelete = await _context.Groups.Where(g => ids.Contains(g.Id)).ToListAsync();
                if (!groupsToDelete.Any())
                {
                    return ResponseFactory.Error("No groups found for the provided IDs.");
                }
                _context.Groups.RemoveRange(groupsToDelete);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ResponseFactory.Ok("Groups deleted successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error(ex.Message);
            }
        }
    }
}
