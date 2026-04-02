using System;
using System.Collections.Generic;
using System.Text;
using SharedLibraries.model;

namespace SharedLibraries.model
{
    public class GroupModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int NumberGroup { get; set; }

        public Guid CounterpartyId { get; set; }
        public CounterpartyModel Counterparty { get; set; } = new CounterpartyModel();

        public Guid CompanyId { get; set; }
        public Company Company { get; set; }

        public string Address { get; set; }
        public double Area { get; set; } = 1.0;
        public bool IsAlert { get; set; } = false;
        public DateOnly? DateCloseDepartment { get; set; }

        public List<GuardModel> Guard { get; set; } = new List<GuardModel>();
        public List<SubleaseModel> Sublease { get; set; } = new List<SubleaseModel>();

    }

    public class GroupFiles
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GroupId { get; set; }
        public GroupModel Group { get; set; }

        public string? FilePath { get; set; }
    }

}
