using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;
using SharedLibraries.Factory;
using SharedLibraries.model;
using SharedLibraries.model.dto;

namespace Groups.service
{
    public class GroupInfoService
    {
        private readonly DatabaseModel _context;

        public GroupInfoService(DatabaseModel context)
        {
            _context = context;
        }
        public async Task<IResponse> GetAllGroupInfo()
        {
            var data = await _context.GroupRentInfos.ToListAsync();
            if (data == null || !data.Any())
                return ResponseFactory.Error("No group information found.");

            return ResponseFactory.Ok(data);
        }

        public async Task<IResponse> GetGroupInfo(Guid groupId)
        {
            var data = await _context.GroupRentInfos
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (data == null)
                return ResponseFactory.Error("Group information not found.");

            return data switch
            {
                SubleaseRentInfo s => ResponseFactory.Ok(new SubleaseRentInfoDtoResponse
                {
                    Id = s.Id,
                    GroupId = s.GroupId,
                    RentNumber = s.RentNumber,
                    StartDate = s.StartDate,
                    Person = s.Person,
                    Rnokpp = s.Rnokpp,
                    Edrpou = s.Edrpou,
                    RentTypeDiscriminator = GroupRentType.Sublease
                }),
                RentType1Info t1 => ResponseFactory.Ok(new RentType1InfoDtoResponse
                {
                    Id = t1.Id,
                    GroupId = t1.GroupId,
                    CertNumber = t1.CertNumber,
                    SeriesCert = t1.SeriesCert,
                    Issued = t1.Issued,
                    RentTypeDiscriminator = GroupRentType.Type1
                }),
                RentType2Info t2 => ResponseFactory.Ok(new RentType2InfoDtoResponse
                {
                    Id = t2.Id,
                    GroupId = t2.GroupId,
                    Date = t2.Date,
                    Num = t2.Num,
                    RentTypeDiscriminator = GroupRentType.Type2
                }),
                _ => ResponseFactory.Error("Unknown rent type.")
            };
        }

        public async Task<IResponse> CreateSubleaseRentInfo(Guid groupId, SubleaseRentInfoDto dto)
        {
            if (dto == null)
                return ResponseFactory.Error("Rent info cannot be null.");
            try
            {
                var group = await _context.Groups.FindAsync(groupId);
                if (group == null)
                    return ResponseFactory.Error("Group not found.");

                var entity = new SubleaseRentInfo
                {
                    GroupId = groupId,
                    RentNumber = dto.RentNumber,
                    StartDate = dto.StartDate,
                    Person = dto.Person,
                    Rnokpp = dto.Rnokpp,
                    Edrpou = dto.Edrpou,
                };

                group.RentType = GroupRentType.Sublease;

                _context.GroupRentInfos.Add(entity);
                await _context.SaveChangesAsync();

                return ResponseFactory.Ok(new SubleaseRentInfoDtoResponse
                {
                    Id = entity.Id,
                    GroupId = entity.GroupId,
                    RentNumber = entity.RentNumber,
                    StartDate = entity.StartDate,
                    Person = entity.Person,
                    Rnokpp = entity.Rnokpp,
                    Edrpou = entity.Edrpou,
                });
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> CreateRentType1Info(Guid groupId, RentType1InfoDto dto)
        {
            if (dto == null)
                return ResponseFactory.Error("Rent info cannot be null.");
            try
            {
                var group = await _context.Groups.FindAsync(groupId);
                if (group == null)
                    return ResponseFactory.Error("Group not found.");

                var entity = new RentType1Info
                {
                    GroupId = groupId,
                    CertNumber = dto.CertNumber,
                    SeriesCert = dto.SeriesCert,
                    Issued = dto.Issued,
                };

                group.RentType = GroupRentType.Type1;

                _context.GroupRentInfos.Add(entity);
                await _context.SaveChangesAsync();

                return ResponseFactory.Ok(new RentType1InfoDtoResponse
                {
                    Id = entity.Id,
                    GroupId = entity.GroupId,
                    CertNumber = entity.CertNumber,
                    SeriesCert = entity.SeriesCert,
                    Issued = entity.Issued,
                });
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> CreateRentType2Info(Guid groupId, RentType2InfoDto dto)
        {
            if (dto == null)
                return ResponseFactory.Error("Rent info cannot be null.");
            try
            {
                var group = await _context.Groups.FindAsync(groupId);
                if (group == null)
                    return ResponseFactory.Error("Group not found.");

                var entity = new RentType2Info
                {
                    GroupId = groupId,
                    Date = dto.Date,
                    Num = dto.Num,
                };

                group.RentType = GroupRentType.Type2;

                _context.GroupRentInfos.Add(entity);
                await _context.SaveChangesAsync();

                return ResponseFactory.Ok(new RentType2InfoDtoResponse
                {
                    Id = entity.Id,
                    GroupId = entity.GroupId,
                    Date = entity.Date,
                    Num = entity.Num,
                });
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> UpdateSubleaseRentInfo(Guid groupId, SubleaseRentInfoDto dto)
        {
            if (dto == null)
                return ResponseFactory.Error("Rent info cannot be null.");
            try
            {
                var entity = await _context.GroupRentInfos
                    .OfType<SubleaseRentInfo>()
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                if (entity == null)
                    return ResponseFactory.Error("Sublease rent info not found.");

                entity.RentNumber = dto.RentNumber;
                entity.StartDate = dto.StartDate;
                entity.Person = dto.Person;
                entity.Rnokpp = dto.Rnokpp;
                entity.Edrpou = dto.Edrpou;

                await _context.SaveChangesAsync();

                return ResponseFactory.Ok(new SubleaseRentInfoDtoResponse
                {
                    Id = entity.Id,
                    GroupId = entity.GroupId,
                    RentNumber = entity.RentNumber,
                    StartDate = entity.StartDate,
                    Person = entity.Person,
                    Rnokpp = entity.Rnokpp,
                    Edrpou = entity.Edrpou,
                });
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> UpdateRentType1Info(Guid groupId, RentType1InfoDto dto)
        {
            if (dto == null)
                return ResponseFactory.Error("Rent info cannot be null.");
            try
            {
                var entity = await _context.GroupRentInfos
                    .OfType<RentType1Info>()
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                if (entity == null)
                    return ResponseFactory.Error("Rent type 1 info not found.");

                entity.CertNumber = dto.CertNumber;
                entity.SeriesCert = dto.SeriesCert;
                entity.Issued = dto.Issued;

                await _context.SaveChangesAsync();

                return ResponseFactory.Ok(new RentType1InfoDtoResponse
                {
                    Id = entity.Id,
                    GroupId = entity.GroupId,
                    CertNumber = entity.CertNumber,
                    SeriesCert = entity.SeriesCert,
                    Issued = entity.Issued,
                });
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> UpdateRentType2Info(Guid groupId, RentType2InfoDto dto)
        {
            if (dto == null)
                return ResponseFactory.Error("Rent info cannot be null.");
            try
            {
                var entity = await _context.GroupRentInfos
                    .OfType<RentType2Info>()
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                if (entity == null)
                    return ResponseFactory.Error("Rent type 2 info not found.");

                entity.Date = dto.Date;
                entity.Num = dto.Num;

                await _context.SaveChangesAsync();

                return ResponseFactory.Ok(new RentType2InfoDtoResponse
                {
                    Id = entity.Id,
                    GroupId = entity.GroupId,
                    Date = entity.Date,
                    Num = entity.Num,
                });
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }

        public async Task<IResponse> DeleteGroupRentInfo(Guid groupId)
        {
            try
            {
                var entity = await _context.GroupRentInfos
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                if (entity == null)
                    return ResponseFactory.Error("Rent info not found.");

                var group = await _context.Groups.FindAsync(groupId);
                if (group != null)
                    group.RentType = GroupRentType.None;

                _context.GroupRentInfos.Remove(entity);
                await _context.SaveChangesAsync();

                return ResponseFactory.Ok("Rent info deleted successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(ex.Message);
            }
        }
    }
}
