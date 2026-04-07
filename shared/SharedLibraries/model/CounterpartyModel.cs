using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedLibraries.model
{
    public class CounterpartyModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        //public Guid CompanyId { get; set; }
        //public Company Company { get; set; }

        public string Fullname { get; set; }
        public string ShortName { get; set; }
        public string GroupName { get; set; }
        public string Address { get; set; }
        public string BankAccount { get; set; }

        public string ResPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public CounterpartyStatus Status { get; set; }

        public List<GroupModel> Groups { get; set; } = new List<GroupModel>();
    }

    public class CounterpartyFop : CounterpartyModel
    {
        public string Edryofop { get; set; }

    }

    public class CounterpartyLLC : CounterpartyModel
    {
        public string Rnokpp { get; set; }
        public string Director { get; set; }
        public string ShortNameDirector { get; set; }
    }
}
