using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace BuildingBlocks.Presentation.Middleware
{
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string TraceIdHeaderName = "X-Trace-Id";

        public TraceIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string traceId = context.Request.Headers[TraceIdHeaderName].ToString();

            if (string.IsNullOrWhiteSpace(traceId))
            {
                traceId = Guid.NewGuid().ToString();
                context.Request.Headers[TraceIdHeaderName] = traceId;
            }

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(TraceIdHeaderName))
                {
                    context.Response.Headers.Append(TraceIdHeaderName, traceId);
                }
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("TraceId", traceId))
            {
                context.Items["TraceId"] = traceId;
                await _next(context);
            }
        }
    }
}
