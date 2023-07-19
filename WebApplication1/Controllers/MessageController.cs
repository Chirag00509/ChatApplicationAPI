﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Modal;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ChatContext _context;

        public MessageController(ChatContext context)
        {
            _context = context;
        }

        // GET: api/Message
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessage()
        {
            return await _context.Message.ToListAsync();
        }

        // PUT: api/Message/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(int id, Message message)
        {
            var userId = GetUserId(HttpContext);
            if(userId == -1)
            {
                return Unauthorized(new { message = "Unauthorized message" });
            }

            if(!ModelState.IsValid) 
            {
                return BadRequest(new { message = "message editing failed due to validation errors." });
            }

            var messages = await _context.Message.FirstOrDefaultAsync(u => u.Id == id);

            if (messages == null)
            {
                return NotFound(new { message = "message not found" });
            }

            //_context.Entry(message).State = EntityState.Modified;

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

            int userId = GetUserId(HttpContext);

            if (userId == -1) 
            {
                return Unauthorized(new { message = "Unauthorized access" });
             }

            message.SenderId = userId;
            message.Timestemp = DateTime.Now;

            _context.Message.Add(message);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMessage", new { id = message.Id }, message);

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

            var userId = GetUserId(HttpContext);

            if(userId == -1)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            _context.Message.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message deleted Successfully" });
        }

        private int GetUserId(HttpContext context)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            var token = authorizationHeader?.Replace("Bearer ", "");

            var user = _context.User.FirstOrDefault(u => u.accessToken == token);

            return user?.Id ?? -1;
        }


        private bool MessageExists(int id)
        {
            return _context.Message.Any(e => e.Id == id);
        }
    }
}