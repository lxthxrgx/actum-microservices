using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.model.dto
{
    public class guardDto
    {
        public Guid GroupId { get; set; }
        public string Address { get; set; }
        public string OhronnaComp { get; set; }
        public string NumDog { get; set; }
        public string NumDog2 { get; set; }
        public DateOnly StrokDii { get; set; }
        public DateOnly? StrokDii2 { get; set; }
        public string? ResPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
    public class guardDtoResponse : guardDto
    {
        public Guid Id { get; set; }
    }

    public class guardDtoDelete
    {
        public Guid Id { get; set; }
    }
}
