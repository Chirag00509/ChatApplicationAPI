using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Modal;
using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;
using NuGet.ContentModel;
using System.Diagnostics.Metrics;
using System.Numerics;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ChatContext _context;

        //private readonly ChatContext _configuration;
        IConfiguration _configuration;

        public UserController(ChatContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/User
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            var currentUser = HttpContext.User;

            var id = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Unauthorized( new { message = "Unauthorized access" } );
            }
            var users = await _context.User.Where(u => u.Id != Convert.ToInt32(id))
                .Select(u => new UserProfile
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,

                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("/api/register")]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var email  = _context.User.Any(u => u.Email == user.Email);

            if (email)
            {
                return Conflict(new { message = "Registration failed because the email is already registered." });
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(new { message = "Registration failed due to validation errors." });
            }


            var convertPassword = hashPassword(user.Password);
            
            user.Password = convertPassword;
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var Profile = new UserProfile
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
            };

            return CreatedAtAction("GetUser", new { id = user.Id }, Profile);
        }

        [HttpPost("/api/login")]

        public async Task<ActionResult<Login>> login(Login login) 
        {
            var convertPassword = hashPassword(login.Password);
            var users = await _context.User.FirstOrDefaultAsync(u=> u.Email == login.Email &&  u.Password == convertPassword ); 

            if (users == null)
            {
                return Unauthorized(new { message = "Login failed due to incorrect credentials" });
            }

            if(!ModelState.IsValid) 
            {
                return BadRequest(new { message = "Login failed due to validation errors." });
            }

            var token = getToken(users.Id, users.Name, users.Email);

            var response = new LoginResponse
            {
                Token = token,

                Profile = new UserProfile
                {
                    Id = users.Id,
                    Name = users.Name,  
                    Email = users.Email,
                }
            };

            return Ok(response);
        }
        private string getToken(int id, string name, string email )
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signIn);


            string Token = new JwtSecurityTokenHandler().WriteToken(token);

            return Token;
        }

        private string hashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashBytes);
            }

        }
    }
}
