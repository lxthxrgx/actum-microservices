using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.model
{
    public class Users
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DateRegistration { get; set; } = DateTime.UtcNow;

        //public List<CompanyUser> CompanyUsers { get; set; } = new List<CompanyUser>();
    }
}
