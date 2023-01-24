using BackendExam.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExamContext;
using ExamContext.TestData;

namespace BackendExam.Controllers
{

    
    [ApiController]

    public class LoginController : ControllerBase
    {
        public record RegisterRequest(string UserName, string Password, List<string> Roles);
        private readonly IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("/login")]

        public ActionResult Login([FromServices] UserRepository userRepository, [FromBody] LoginDTO loginDTO)
        {

            try
            {
                User user = userRepository.Users.SingleOrDefault(x => x.UserName == loginDTO.UserName);
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


                if (user == null)
                {
                    return Unauthorized();
                }

                if (user.Password == loginDTO.Password)
                {
                    var token = GenerateToken(user);
                    TokenDTO usertoken = new TokenDTO();
                    usertoken.Value = token;
                    return Accepted(usertoken);
                }
                else
                {
                    return StatusCode(statusCode: 403);
                }
            }
            catch
            {

                return Unauthorized();
            }

        }

        // To generate token
        private string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserName),
                new Claim(ClaimTypes.Role,value:"Employee"),
                new Claim(ClaimTypes.Role,value:"Manager"),
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest User)
        {
            UserRepository _userRepository = new UserRepository();

            if (_userRepository.Users.Any(x => x.UserName == User.UserName))
                return BadRequest("User Name Used");
            List<Role> Roles = User.Roles.Select(x => new Role(x)).ToList();
            _userRepository.Users.Add(new User("", User.UserName, User.Password, Roles));
            return Ok();
        }

    }

}

