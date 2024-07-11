using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CxReports.ApiClient.V1
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCxReportsApiClient(
            this IServiceCollection services,
            CxReportsClientConfig config
        )
        {
            services.AddSingleton(config);
            services.AddHttpClient<ICxReportsClient, CxReportsClient>();
            return services;
        }

        public static IServiceCollection AddCxReportsApiClient(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var config = new CxReportsClientConfig();
            configuration.GetSection("CxReports").Bind(config);
            return services.AddCxReportsApiClient(config);
        }
    }
}
