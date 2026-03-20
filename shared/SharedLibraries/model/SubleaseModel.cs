using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedLibraries.model
{
    public class SubleaseModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GroupId { get; set; }
        public GroupModel Group { get; set; } = new GroupModel();

        public string ContractNumber { get; set; }
        public DateTime ContractSigningDate { get; set; }
        public DateTime AktDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public DateTime? ContractEndDate2 { get; set; }
        public double RentalFee { get; set; }
        public double RentalFee2 { get; set; }
        public bool IsContinuation { get; set; }
        public bool? Done { get; set; } = false;

        public List<SubleaseNotes> SubleaseNotes { get; set; } = new List<SubleaseNotes>();
        public List<SubleaseFiles> SubleaseFiles { get; set; } = new List<SubleaseFiles>();
    }

    public class SubleaseNotes
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SubleaseId { get;  set; }
        public SubleaseModel Sublease { get; set; } = new SubleaseModel();

        public string? Note { get; set; }
    }

    public class SubleaseFiles
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SubleaseId { get; set; }
        public SubleaseModel Sublease { get; set; } = new SubleaseModel();

        public string? FilePath { get; set; }
    }
}
