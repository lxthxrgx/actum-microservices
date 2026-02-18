using SharedLibraries.Database;
using SharedLibraries.Factory;
using Microsoft.EntityFrameworkCore;

namespace Auth.service
{
    interface ISignIn
    {
        public Task<IResponse> SignInAsync(string email, string password);
    }

    public class SignIn : ISignIn
    {
        private readonly DatabaseModel _context;

        public SignIn(DatabaseModel context)
        {
            _context = context;
        }

        public async Task<IResponse> SignInAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                return ResponseFactory.Error("Email cannot be empty.");
            }

            if (string.IsNullOrEmpty(password))
            {
                return ResponseFactory.Error("Password cannot be empty.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return ResponseFactory.Error("User not found.");
            }

            if(user.Password != password)
            {
                return ResponseFactory.Error("Incorrect password.");
            }

            return ResponseFactory.Ok<object>("Sign-in successful.");
        }
    }
}
