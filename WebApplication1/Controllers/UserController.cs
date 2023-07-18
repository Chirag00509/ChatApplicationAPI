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

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ChatContext _context;

        public UserController(ChatContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await _context.User.ToListAsync();
        }

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

            return Ok(new { message = "Successfull" });
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
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
