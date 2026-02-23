using Microsoft.EntityFrameworkCore;
using SharedLibraries.Database;
using SharedLibraries.Factory;
using SharedLibraries.model;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace Auth.service
{
    interface ISignIn
    {
        public Task<IResponse> SignInAsync(string email, string password);
    }

    public class SignIn : ISignIn
    {
        private readonly DatabaseModel _context;
        private readonly jwtService _jwtService;
        private readonly httpOnly _httpOnly;

        public SignIn(DatabaseModel context, jwtService jwtService, httpOnly httpOnly)
        {
            _context = context;
            _jwtService = jwtService;
            _httpOnly = httpOnly;
        }

        public async Task<IResponse> SignInAsync(string email, string password)
        {
            try
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

                var userCompany = await _context.CompanyUsers
                    .FirstOrDefaultAsync(cu => cu.UserId == user.Id);

                if (user.Password != password)
                {
                    return ResponseFactory.Error("Incorrect password.");
                }

                var claims = new Claims
                {
                    userId = user.Id,
                    Company = userCompany?.CompanyId ?? Guid.Empty,
                    role = userCompany?.Role ?? CompanyUserRole.Member
                };

                return ResponseFactory.Ok<Claims>(claims, "Sign-in successful");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error($"Sign-in error: {ex}");
            }
        }
    }
}
