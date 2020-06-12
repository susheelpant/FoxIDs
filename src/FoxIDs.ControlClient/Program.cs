using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FoxIDs.Logic;
using FoxIDs.Infrastructure;
using ITfoxtec.Identity.BlazorWebAssembly.OpenidConnect;
using FoxIDs.Services;

namespace FoxIDs
{
    public class Program
    {
        const string httpClientLogicalName = "FoxIDs.ControlAPI";

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            ConfigureServices(builder.Services, builder.Configuration, builder.HostEnvironment);
   
            await builder.Build().RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services, WebAssemblyHostConfiguration configuration, IWebAssemblyHostEnvironment hostEnvironment)
        {
            services.AddHttpClient(httpClientLogicalName, client => client.BaseAddress = new Uri(hostEnvironment.BaseAddress))
                       .AddHttpMessageHandler<AccessTokenMessageHandler>()
                       .AddHttpMessageHandler<CheckResponseMessageHandler>();

            services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientLogicalName));

            services.AddTenantOpenidConnectPkce(settings =>
            {
                configuration.Bind("IdentitySettings", settings);
            });

            services.AddTransient<CheckResponseMessageHandler>();

            services.AddScoped<TenantService>();
            services.AddScoped<TrackService>();

            services.AddScoped<RouteBindingLogic>();
        }
    }
}