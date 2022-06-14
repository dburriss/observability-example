using Serilog;
using Serilog.Events;
using Serilog.Sinks.Datadog.Logs;
using StatsdClient;
using System.Linq;

namespace ObservabilityProject.Api
{
    public static class TelemetrySetupExtension
    {
        public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder webApplicationBuilder)
        {
            CheckEnvVars();

            return webApplicationBuilder
                .AddLogging()
                .AddMetrics();
        }

        private static void CheckEnvVars()
        {
            List<(string,string)> missing = new List<(string, string)>();
            missing.EnvVarCheck("DD_API_KEY", "Set `DD_API_KEY` with your Datadog API key.");
            missing.EnvVarCheck("DD_ENV", "Set `DD_ENV` with the name of the current environment eg. prod");
            missing.EnvVarCheck("DD_SERVICE", "Set `DD_SERVICE` with the name of this service.");
            missing.EnvVarCheck("DD_VERSION", "Set `DD_VERSION` with the version op this application");
            missing.EnvVarCheck("DD_SITE", "Set `DD_SITE` with the site to send to eg. datadoghq.eu");
            missing.EnvVarCheck("DD_LOGS_DIRECT_SUBMISSION_INTEGRATIONS", "Set `DD_LOGS_DIRECT_SUBMISSION_INTEGRATIONS` to `Serilog`");
            missing.EnvVarCheck("DD_LOGS_INJECTION", "Set `DD_LOGS_INJECTION` to `true`");
            missing.EnvVarCheck("DD_TRACE_ENABLED", "Set `DD_TRACE_ENABLED` to `true`");

            if(missing.Count > 0)
            {
                var keys = missing.Select(x => x.Item1);
                var summary = string.Join(',', keys);
                var data = new Dictionary<string, string>();
                var msg = $"The following expected environment variables are missing: {summary}";
                var ex = new Exception(msg);
                foreach(var (k,v) in missing)
                {
                    ex.Data.Add(k, v);
                }
                throw ex;
            }
        }

        private static void EnvVarCheck(this List<(string,string)> missing, string key, string errorMessage)
        {
            if (Environment.GetEnvironmentVariable(key) == null)
            {
                missing.Add((key, errorMessage));
            }
        }

        private static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((ctx, cfg) => {
                cfg
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.DatadogLogs(
                    Environment.GetEnvironmentVariable("DD_API_KEY"),
                    service: Environment.GetEnvironmentVariable("DD_SERVICE"),
                    host: Environment.UserDomainName ?? "unknown",
                    configuration: new DatadogConfiguration(url: "tcp-intake.logs.datadoghq.eu", port: 443, useSSL: true, useTCP: true)
                );
            });
            return builder;
        }

        private static WebApplicationBuilder AddMetrics(this WebApplicationBuilder builder)
        {
            try
            {
                var config = new StatsdConfig {
                    ConstantTags = new string[] { 
                        $"service:{Environment.GetEnvironmentVariable("DD_SERVICE")}",
                        $"env:{Environment.GetEnvironmentVariable("DD_ENV")}",
                        $"version:{Environment.GetEnvironmentVariable("DD_VERSION")}"
                    } 
                };
                // Configure your DogStatsd client and configure any tags
                DogStatsd.Configure(config);
            }
            catch (Exception ex)
            {
                // An exception is thrown by the Configure call if the necessary environment variables are not present.
                // These environment variables are present in Azure App Service, but
                // need to be set in order to test your custom metrics: DD_API_KEY:{api_key}, DD_AGENT_HOST:localhost
                // Ignore or log the exception as it suits you
                Console.WriteLine(ex);
                Log.Logger.Error(ex, "DD metrics failed");
            }
            return builder;
        }
    }
}
