using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.model.dto
{
    public class subleaseSummaryDto
    {
        public int Total { get; set; }
        public int NewThisMonth { get; set; }
        public int Active { get; set; }
        public double ActivePercent { get; set; }
        public int ExpiringIn30Days { get; set; }
        public int Done { get; set; }
    }
}
