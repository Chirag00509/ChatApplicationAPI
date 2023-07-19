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
        public async Task<ActionResult<IEnumerable<User>>> GetUser(string token)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.accessToken == token);

            Console.WriteLine(user.Id);

            if (user == null)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            var users =  _context.User.Where(u => u.Id != user.Id).ToList();
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

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        [HttpPost("/api/login")]

        public async Task<ActionResult<User>> login(string email, string password) 
        {
            var convertPassword = hashPassword(password);
            var users = await _context.User.FirstOrDefaultAsync(u=> u.Email == email &&  u.Password == convertPassword ); 

            if (users == null)
            {
                return Unauthorized(new { message = "Login failed due to incorrect credentials" });
            }

            if(!ModelState.IsValid) 
            {
                return BadRequest(new { message = "Login failed due to validation errors." });
            }

            var token = getToken(users.Id, users.Name, users.Email);
            users.accessToken = token;
            await _context.SaveChangesAsync();
            return Ok(users);
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        private string getToken(int id, string name, string email )
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("Id", id.ToString()),
                new Claim("Name", name),
                new Claim("Email", email)
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
