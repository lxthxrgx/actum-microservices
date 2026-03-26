using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.model.dto
{
    public class counterpartyDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Fullname { get; set; }
        public string ShortName { get; set; }
        public string GroupName { get; set; }
        public string Address { get; set; }
        public string BankAccount { get; set; }

        public string ResPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public CounterpartyStatus Status { get; set; }
    }

    public class counterpartyIdDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class counterpartyDeleteDto : counterpartyIdDto
    {
    }
}
