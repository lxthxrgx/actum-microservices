using Auth.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.Factory;
using SharedLibraries.model;

namespace Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class authController : ControllerBase
    {
        private readonly SignIn _signInService;
        private readonly SignUp _signUpService;
        private readonly jwtService _jwtService;
        private readonly httpOnly _httpOnly;

        public authController(SignIn signInService, SignUp signUpService,
            jwtService jwtService, httpOnly httpOnly)
        {
            _signInService = signInService;
            _signUpService = signUpService;
            _httpOnly = httpOnly;
            _jwtService = jwtService;
        }

        public class SignInDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class SignUpDto : SignInDto
        {
            public string Name { get; set; }
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInDto signin)
        {
            var response = await _signInService.SignInAsync(signin.Email, signin.Password);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            if (!(response is IOk<Claims> ok) || ok.Data == null || !ok.Data.Any())
            {
                return BadRequest("Claims data is missing.");
            }

            var claims = ok.Data.First();

            Console.WriteLine($"Claims for user {signin.Email}: userId={claims.userId}, companyId={claims.companyId}, role={claims.role}");
            string accessToken;

            try
            {
                 accessToken = _jwtService.GenerateAccessTokenAsync(claims);
            }
            catch (Exception ex) 
            {
                accessToken = "";
                Console.WriteLine(ex);
            }


            string refreshToken ;

            try
            {
                refreshToken = _jwtService.GenerateRefreshTokenAsync();
            }
            catch (Exception ex)
            {
                refreshToken = "";
                Console.WriteLine(ex);
            }

            _httpOnly.SetHttpOnlyCookie(accessToken, refreshToken, Response);

            return Ok(new
            {
                message = response.Message,
                accessToken = accessToken,
                refreshToken = refreshToken
            });
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp(SignUpDto signup)
        {
            var response = await _signUpService.SignUpAsync(signup.Email, signup.Password, signup.Name);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
