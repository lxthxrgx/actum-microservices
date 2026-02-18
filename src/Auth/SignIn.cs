using SharedLibraries.Database;

namespace Auth
{
    interface ISignIn
    {
        public Task<string> SignInAsync(string email, string password);
    }

    public class SignIn : ISignIn
    {
        private readonly DatabaseModel _context;

        public SignIn(DatabaseModel context)
        {
            _context = context;
        }

        public async Task<string> SignInAsync(string email, string password)
        {
            return string.Empty;
        }
    }
}
