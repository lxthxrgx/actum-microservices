using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedLibraries.model
{
    public class CounterpartyModel
    {
        public int Id { get; set; }
        public string Fullname { get; private set; } = "Fullname";
        public string ShortName { get; private set; } = "ShortName";
        public string Address { get; private set; } = "Address";
        public string BanckAccount { get; private set; } = "BanckAccount";

        public string ResPerson { get; private set; } = "ResPerson";
        public string Phone { get; private set; } = "Phone";
        public string Email { get; private set; } = "Email";
        public string Status { get; private set; } = "Status";

        
    }

    public class CounterpartyFop : CounterpartyModel
    {
        public string Edryofop { get; private set; } = "Edryofop";

    }
    public class CounterpartyLLC : CounterpartyModel
    {
        public string Rnokpp { get; private set; } = "Rnokpp";
        public string Director { get; private set; } = "Director";
        public string ShortNameDirector { get; private set; } = "ShortNameDirector";
    }
}
