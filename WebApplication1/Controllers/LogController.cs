using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using WebApplication1.Data;
using WebApplication1.Modal;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LogController : ControllerBase
    {
        private readonly ChatContext _context;

        public LogController(ChatContext context)
        {
            _context = context;
        }

        // GET: api/Log
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Logs>>> GetLogs()
        {
            var logs = _context.Logs.Select(u => new 
            {
                Id = u.id,
                Ip = u.Ip,
                Username = u.Username,
                RequestBody = u.RequestBody.Replace("\n","").Replace("\"", "").Replace("\r",""),
                TimeStamp = u.TimeStamp,
            });

            if(logs == null)
            {
                return NotFound(new { message = "Logs not found" });
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            return Ok(logs);

        }
    }
}
