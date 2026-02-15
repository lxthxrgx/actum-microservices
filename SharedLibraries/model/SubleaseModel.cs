using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedLibraries.model
{
    public class SubleaseModel
    {
        public int Id { get; set; }

        public int GroupId { get; private set; }
        public GroupModel GroupTable { get; private set; } = new GroupModel();

        public string Address { get; private set; } = "Address";
        public string ContractNumber { get; private set; } = "ContractNumber";
        public DateTime ContractSigningDate { get; private set; }
        public DateTime AktDate { get; private set; }
        public DateTime ContractEndDate { get; private set; }
        public DateTime ContractEndDate2 { get; private set; }
        public double RentalFee { get; private set; }
        public double RentalFee2 { get; private set; }
        public bool? Done { get; set; } = false;
        public ICollection<GroupModel> Group { get; } = new List<GroupModel>();
    }

    public class SubleaseNotes
    {
        public int Id { get; private set; }
        public int SubleaseId { get; private set; }
        public SubleaseModel Sublease { get; private set; } = new SubleaseModel();
        public string? PathToPdfFile_Sublease { get; private set; }
    }
}
