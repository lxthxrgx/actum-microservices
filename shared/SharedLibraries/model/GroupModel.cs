using SharedLibraries.model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedLibraries.model
{
    public class GroupModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int NumberGroup { get; set; }

        public Guid CounterpartyId { get; set; }
        public CounterpartyModel Counterparty { get; set; } = new CounterpartyModel();

        //public Guid CompanyId { get; set; }
        //public Company Company { get; set; }

        public string Address { get; set; }
        public double Area { get; set; } = 1.0;
        public bool IsAlert { get; set; } = false;
        public DateOnly? DateCloseDepartment { get; set; }

        public GroupRentType RentType { get; set; } = GroupRentType.None;
        public GroupRentInfo? RentInfo{get; set;}

        public List<GuardModel> Guard { get; set; } = new List<GuardModel>();
        public List<SubleaseModel> Sublease { get; set; } = new List<SubleaseModel>();

    }

    public abstract class GroupRentInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GroupId { get; set; }
        public GroupModel Group { get; set; } = null!;

        [NotMapped]
        public abstract GroupRentType RentType { get; }
    }

    public class SubleaseRentInfo : GroupRentInfo
    {
        [NotMapped]
        public override GroupRentType RentType => GroupRentType.Sublease;

        public string RentNumber { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public string Person { get; set; } = string.Empty;
        public string Rnokpp { get; set; } = string.Empty;
        public string Edrpou { get; set; } = string.Empty;
    }

    public class RentType1Info : GroupRentInfo
    {
        [NotMapped]
        public override GroupRentType RentType => GroupRentType.Type1;

        public string CertNumber { get; set; } = string.Empty;
        public string SeriesCert { get; set; } = string.Empty;
        public string Issued { get; set; } = string.Empty;
    }

    public class RentType2Info : GroupRentInfo
    {
        [NotMapped]
        public override GroupRentType RentType => GroupRentType.Type2;

        public DateOnly Date { get; set; }
        public string Num { get; set; } = string.Empty;
    }

    public enum GroupRentType
    {
        None,
        Sublease,
        Type1,
        Type2
    }

    //public class GroupFiles
    //{
    //    public Guid Id { get; set; } = Guid.NewGuid();

    //    public Guid GroupId { get; set; }
    //    public GroupModel Group { get; set; }

    //    public string? FilePath { get; set; }
    //}

}
