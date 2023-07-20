using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Modal;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class MessageController : ControllerBase
    {
        private readonly ChatContext _context;

        public MessageController(ChatContext context)
        {
            _context = context;
        }

        // GET: api/Message
        [HttpGet]

        public async Task<ActionResult<IEnumerable<Message>>> GetMessage(History history)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "invalid request parameter." });
            }

            var currentTime = DateTime.Now;

            var count  = history.count;

            var query = _context.Message.
                Where(u => u.ReceiverId == history.userId &&
                (history.before.Equals(DateTime.MinValue) ? u.Timestemp < currentTime : u.Timestemp < history.before));
            if(history.sort == "desc") 
            {
                query = query.OrderByDescending(u => u.Timestemp);
            }
            else
            {
                query = query.OrderBy(u => u.Timestemp);
            }

             var messages =  query.Take(count == 0 ? 20 : count)
                .Select(u => new
                {
                    id = u.Id,
                    senderId = u.SenderId,
                    receiverId = u.ReceiverId,
                    content = u.content,
                    timestamp = u.Timestemp
                })
                .ToList();


            if (messages == null)
            {
                return NotFound(new { message = "User or conversation not found" });
            }

            return Ok(messages);
        }

        // PUT: api/Message/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(int id, Message message)
        {
            if(!ModelState.IsValid) 
            {
                return BadRequest(new { message = "message editing failed due to validation errors." });
            }

            if(id != message.SenderId)
            {
                return Unauthorized(new { message = "Unauthorized access" } );
            }

            var messages = await _context.Message.FirstOrDefaultAsync(u => u.Id == id);


            if (messages == null)
            {
                return NotFound(new { message = "message not found" });
            }

            messages.content = message.content;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message edited successfully" });
        }

        // POST: api/Message
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Message>> PostMessage(Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "message sending failed due to validation errors." });
            }

            var currentUser = HttpContext.User;
            var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            message.SenderId = Convert.ToInt32(userId);
            message.Timestemp = DateTime.Now;

            _context.Message.Add(message);
            await _context.SaveChangesAsync();

            var messageResponse = new MessageResponse
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.content,
                Timestemp = message.Timestemp,
            };

            return Ok(messageResponse);

        }

        // DELETE: api/Message/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Message.FindAsync(id);
            if (message == null)
            {
                return NotFound(new { message = "Message not found" });
            }

            _context.Message.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message deleted Successfully" });
        }
    }
}
