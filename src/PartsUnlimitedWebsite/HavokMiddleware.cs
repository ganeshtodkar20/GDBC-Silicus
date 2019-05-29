using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PartsUnlimited.Models;

namespace PartsUnlimited
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class HavokMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _minDelayInMs;
        private readonly int _maxDelayInMs;
        private readonly ThreadLocal<Random> _random;

        public HavokMiddleware(RequestDelegate next,
        TimeSpan min, TimeSpan max)
        {
            _next = next;
            _minDelayInMs = (int)min.TotalMilliseconds;
            _maxDelayInMs = (int)max.TotalMilliseconds;
            _random = new ThreadLocal<Random>(() => new Random());
        }
        public async Task Invoke(HttpContext httpContext,HavokContext context)
        {
            int delayInMs = _random.Value.Next(_minDelayInMs, _maxDelayInMs);
            var myEnt = context.Havoks.First();
          
            if (myEnt.HavokEnabled == true  && myEnt.isScaledOut== false)
            {
                await Task.Delay(delayInMs);
            }
            await _next(httpContext);
        }
    }
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class HavokMiddlewareExtensions
    {
        public static IApplicationBuilder UseHavokMiddleware(
    this IApplicationBuilder app, TimeSpan min, TimeSpan max)
        {
            return app.UseMiddleware(
                typeof(HavokMiddleware),
                min,
                max
            );
        }
    }

}
