using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.components
{
    public static class ConfigHelper
    {
        public static IConfiguration Configuration { get; set; }
    }
}
