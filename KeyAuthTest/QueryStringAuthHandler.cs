using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace KeyAuthTest
{
    public class QueryStringAuthOptions : AuthenticationOptions
    {
        public const string QueryStringAuthSchema = "QueryStringAuth";
        public const string QueryStringAuthClaim = "QueryStringKey";

        public QueryStringAuthOptions()
        {
            AuthenticationScheme = QueryStringAuthSchema;
        }

        public string QueryStringKeyParam { get; set; } = "key";

        public string ClaimsTypeName { get; set; } = "QueryStringKey";

        public AuthenticationProperties AuthenticationProperties { get; set; } = new AuthenticationProperties();
    }

    public class QueryStringAuthHandler : AuthenticationHandler<QueryStringAuthOptions>
    {
        /// <summary>
        /// Handle authenticate async
        /// </summary>
        /// <returns></returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Query.TryGetValue(Options.QueryStringKeyParam, out StringValues value) && value.Count > 0)
            {
                var key = value[0];

                //..do your authentication...

                if (!string.IsNullOrWhiteSpace(key))
                {
                    //setup you claim
                    var claimsPrinciple = new ClaimsPrincipal();
                    claimsPrinciple.AddIdentity(new ClaimsIdentity(new[] { new Claim(Options.ClaimsTypeName, key) }, Options.AuthenticationScheme));

                    //create the result ticket
                    var ticket = new AuthenticationTicket(claimsPrinciple, Options.AuthenticationProperties, Options.AuthenticationScheme);
                    var result = AuthenticateResult.Success(ticket);
                    return Task.FromResult(result);
                }
            }
            return Task.FromResult(AuthenticateResult.Fail("Key not found or not valid"));

        }
    }

    public class QueryStringAuthMiddleware : AuthenticationMiddleware<QueryStringAuthOptions>
    {
        public QueryStringAuthMiddleware(RequestDelegate next, IOptions<QueryStringAuthOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder)
            : base(next, options, loggerFactory, encoder)
        {
        }

        protected override AuthenticationHandler<QueryStringAuthOptions> CreateHandler()
        {
            return new QueryStringAuthHandler();
        }
    }


    public static class QueryStringAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseQueryStringAuthentication(this IApplicationBuilder appBuilder)
        {
            if (appBuilder == null)
                throw new ArgumentNullException(nameof(appBuilder));

            var options = new QueryStringAuthOptions();
            return appBuilder.UseQueryStringAuthentication(options);
        }

        public static IApplicationBuilder UseQueryStringAuthentication(this IApplicationBuilder appBuilder, Action<QueryStringAuthOptions> optionsAction)
        {
            if (appBuilder == null)
                throw new ArgumentNullException(nameof(appBuilder));

            var options = new QueryStringAuthOptions();
            optionsAction?.Invoke(options);
            return appBuilder.UseQueryStringAuthentication(options);
        }

        public static IApplicationBuilder UseQueryStringAuthentication(this IApplicationBuilder appBuilder, QueryStringAuthOptions options)
        {
            if (appBuilder == null)
                throw new ArgumentNullException(nameof(appBuilder));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return appBuilder.UseMiddleware<QueryStringAuthMiddleware>(Options.Create(options));
        }
    }
}
