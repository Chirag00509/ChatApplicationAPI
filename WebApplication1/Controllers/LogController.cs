using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Middlewares;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LogController : ControllerBase
    {
        private readonly LoggingMiddleware _loggingMiddleware;
        public LogController(LoggingMiddleware loggingMiddleware)
        {
            _loggingMiddleware = loggingMiddleware;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var logs = _loggingMiddleware.GetLogs();
            return Ok(logs);
        }
    }
}
