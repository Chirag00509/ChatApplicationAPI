using Azure.Core;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Text;
using WebApplication1.Data;
using static System.Reflection.Metadata.BlobBuilder;

namespace WebApplication1.Middlewares
{
    public class LoggingMiddleware : IMiddleware
    {
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly List<string> _logs;

        public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
        {
            _logger = logger;
            _logs = new List<string>();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next) 
        {

            string ip = context.Connection.RemoteIpAddress?.ToString();
            string requestBody = await getRequestBodyAsync(context.Request);
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string username = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "";

            string log = $"IP: {ip}, Username: {username}, Timestamp: {timeStamp}, Request Body: {requestBody}";

            _logger.LogInformation(log);
            _logs.Add(log);

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
        public List<string> GetLogs()
        {
            return _logs;
        }

    }
}
