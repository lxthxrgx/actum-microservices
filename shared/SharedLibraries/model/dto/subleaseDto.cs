using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.model.dto
{
    public class subleaseDto
    {
        public Guid GroupId { get; set; }
        public string ContractNumber { get; set; }
        public DateOnly ContractSigningDate { get; set; }
        public DateOnly AktDate { get; set; }
        public DateOnly ContractEndDate { get; set; }
        public bool IsContinuation { get; set; }
        public DateOnly? ContractEndDate2 { get; set; }
        public double RentalFee { get; set; }
        public double RentalFee2 { get; set; }
        public bool? Done { get; set; } = false;
    }

    public class subleaseDtoResponse : subleaseDto
    {
        public Guid Id { get; set; }
    }

    public class subleaseDtoDelete
    {
        public Guid Id { get; set; }
    }
}
