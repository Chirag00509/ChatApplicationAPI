using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using System.Security.Claims;
using System.Text;
using WebApplication1.Data;
using static System.Reflection.Metadata.BlobBuilder;

namespace WebApplication1.Middlewares
{
    public class LoggingMiddleware : IMiddleware
    {
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly ChatContext _dbcontext;

        public LoggingMiddleware(ILogger<LoggingMiddleware> logger, ChatContext dbcontext)
        {
            _logger = logger;
            _dbcontext = dbcontext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next) 
        {
            var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;

            string ip = context.Connection.RemoteIpAddress?.ToString();
            string RequestBody = await getRequestBodyAsync(context.Request);
            string TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string UserName = userName;


            string log = $"IP: {ip}, Username: {UserName}, Timestamp: {TimeStamp}, Request Body: {RequestBody}";

            _logger.LogInformation(log);

            _dbcontext.Logs.Add(new Modal.Logs
            {
                Ip = ip,
                RequestBody = RequestBody,
                TimeStamp = TimeStamp,
                Username = UserName,    
            });

            await _dbcontext.SaveChangesAsync();

            await next(context);
        }

        public async Task<string> getRequestBodyAsync(HttpRequest req)
        {
            req.EnableBuffering();

            using var reader = new StreamReader(req.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            string requestBody = await reader.ReadToEndAsync();

            req.Body.Position = 0;

            return requestBody;
        }
    }
}
