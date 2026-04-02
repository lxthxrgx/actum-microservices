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
        public Guid CompanyId { get; set; }
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
}