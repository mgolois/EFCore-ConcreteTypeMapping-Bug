using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2
{
    public static class SessionServiceExtensions
    {
        public static void AddSession<T>(this HttpContext context, string key, T data)
        {
            var session = (MyDistributedSession)context.Session;
            session.Set(key, data);
        }

        public static T GetSessionObject<T>(this HttpContext context, string key)
        {
            var session = (MyDistributedSession)context.Session;
            
            session.TryGetObject(key, out T data);
            return data;
        }

        public static T GetSessionValue<T>(this HttpContext context, string key)
        {
            var session = (MyDistributedSession)context.Session;

            session.TryGetValue(key, out T data);
            return data;
        }

        public static IServiceCollection AddMySession(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddTransient<ISessionStore, MyDistributedSessionStore>();
            services.AddDataProtection();
            return services;
        }

    }
}
