using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.model
{
    public enum CompanyUserRole
    {
        Admin,
        Member,
        Owner
    }

    public class CompanyUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public Users User { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
        public DateTime DateExit { get; set; }
        public bool IsActive { get; set; } = true;

        public CompanyUserRole Role { get; set; } = CompanyUserRole.Member;

    }
}
