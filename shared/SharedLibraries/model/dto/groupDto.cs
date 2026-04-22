using SharedLibraries.model.dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.model.dto
{
    public class GroupDto
    {
        public int NumberGroup { get; set; }
        public Guid CounterpartyId { get; set; }
        //public Guid CompanyId { get; set; }
        public string Address { get; set; } = string.Empty;
        public double Area { get; set; } = 1.0;
        public bool IsAlert { get; set; } = false;
        public DateOnly? DateCloseDepartment { get; set; }
    }

    public class GroupDtoResponse : GroupDto
    {
        public Guid Id { get; set; }
    }

    public class GroupDtoDelete
    {
        public Guid Id { get; set; }
    }

    public class SubleaseRentInfoDto 
    {
        public string RentNumber { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public string Person { get; set; } = string.Empty;
        public string Rnokpp { get; set; } = string.Empty;
        public string Edrpou { get; set; } = string.Empty;
    }

    public class RentType1InfoDto
    {
        public string CertNumber { get; set; } = string.Empty;
        public string SeriesCert { get; set; } = string.Empty;
        public string Issued { get; set; } = string.Empty;
    }

    public class RentType2InfoDto
    {
        public DateOnly Date { get; set; }
        public string Num { get; set; } = string.Empty;
    }

    public class SubleaseRentInfoDtoResponse : SubleaseRentInfoDto
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public GroupRentType RentTypeDiscriminator { get; set; } = GroupRentType.Sublease;
    }

    public class RentType1InfoDtoResponse : RentType1InfoDto
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public GroupRentType RentTypeDiscriminator { get; set; } = GroupRentType.Type1;
    }

    public class RentType2InfoDtoResponse : RentType2InfoDto
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public GroupRentType RentTypeDiscriminator { get; set; } = GroupRentType.Type2;
    }

}