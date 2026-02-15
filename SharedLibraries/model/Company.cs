using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.model
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }

        public int UserOwnerId { get; set; }
        public User UserOwner { get; set; } = new User();

        public bool isActive { get; set; } = true;
    }
}
