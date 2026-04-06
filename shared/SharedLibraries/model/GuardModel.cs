using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedLibraries.model
{
    public class GuardModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GroupId { get; set; }
        public GroupModel Group { get; set; } = new GroupModel();

        public string Address { get; set; }
        public string OhronnaComp { get; set; }
        public string NumDog { get; set; }
        public string NumDog2 { get; set; }
        public DateOnly StrokDii { get; set; }
        public DateOnly? StrokDii2 { get; set; }
        public string? ResPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        //public List<GuardNotes> GuardNotes { get; set; } = new List<GuardNotes>();
        //public List<GuardFiles> GuardFiles { get; set; } = new List<GuardFiles>();  
    }

    //public class GuardNotes
    //{
    //    public Guid Id { get; set; } = Guid.NewGuid();

    //    public Guid GuardId { get; set; }
    //    public GuardModel Guard { get; set; } = new GuardModel();

    //    public string? Note { get; set; }
    //}

    //public class GuardFiles
    //{
    //    public Guid Id { get; set; } = Guid.NewGuid();

    //    public Guid GuardId { get; set; }
    //    public GuardModel Guard { get; set; } = new GuardModel();

    //    public string? FilePath { get; set; }
    //}
}
