using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;

namespace Feedme.Api.Utils
{
    public sealed class ProdExceptionHandler
    {
        private readonly RequestDelegate _next;

        public ProdExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            Log.ForContext("Type", "Error")
                .ForContext("Exception", exception, destructureObjects: true)
                .Error(exception, exception.Message);
            string result = JsonConvert.SerializeObject(Envelope.Error("Sorry, an internal error occured"));
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(result);
        }
    }
}