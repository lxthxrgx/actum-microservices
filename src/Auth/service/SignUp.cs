using SharedLibraries.Factory;
using SharedLibraries.Database;
using Microsoft.EntityFrameworkCore;
using SharedLibraries.model;

namespace Auth.service
{
    interface ISignUp
    {
        public Task<IResponse> SignUpAsync(string email, string password, string name);
    }

    public class SignUp : ISignUp
    {
        private readonly DatabaseModel _context;

        public SignUp(DatabaseModel context)
        {
            _context = context;
        }

        public async Task<IResponse> SignUpAsync(string email, string password, string name)
        {
            if (string.IsNullOrEmpty(email))
            {
                return ResponseFactory.Error("Email cannot be empty.");
            }

            if (string.IsNullOrEmpty(password))
            {
                return ResponseFactory.Error("Password cannot be empty.");
            }

            if (string.IsNullOrEmpty(name))
            {
                return ResponseFactory.Error("Name cannot be empty.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                return ResponseFactory.Error("Email is already registered.");
            }

            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var newUser = new Users
                {
                    Email = email,
                    Password = password,
                    Name = name
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error during sign-up: {ex.Message}");
            }

            return ResponseFactory.Ok<object>("Sign-up successful.");
        }
    }
}
