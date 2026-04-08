using System.Text.RegularExpressions;

namespace SharedLibraries.components
{
    public static class DeleteSpace
    {
        public static string Deletespace(string p)
        {
            try
            {
                if (p is not null)
                {
                    var cleanedData = Regex.Replace(p, @"[\r\n]+", " ").Trim();
                    return cleanedData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deletespace: {ex.Message}");
            }
            return null;
        }
    }
}
