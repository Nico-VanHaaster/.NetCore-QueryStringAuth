using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeyAuthTest
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(o =>
            {
                //override the default policy
                o.DefaultPolicy = new AuthorizationPolicy(new[] { new ClaimsAuthorizationRequirement(QueryStringAuthOptions.QueryStringAuthClaim, new string[0]) }, new[] { QueryStringAuthOptions.QueryStringAuthSchema });

                //or add a policy
                //o.AddPolicy("QueryKeyPolicy", options =>
                //{
                //    options.RequireClaim(QueryStringAuthOptions.QueryStringAuthClaim);
                //    options.AddAuthenticationSchemes(QueryStringAuthOptions.QueryStringAuthSchema);
                //});

            });
            services.AddAuthentication(o =>
            {
                o.SignInScheme = QueryStringAuthOptions.QueryStringAuthSchema;
            }); //adds the auth services
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();


            app.UseQueryStringAuthentication(); //add our query string auth

            //add mvc last
            app.UseMvc();
        }
    }
}
