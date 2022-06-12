# Observability Project

## Azure App Service

[Connect Datadog and Azure](https://docs.datadoghq.com/integrations/azure/?tab=azurecliv20)
[Azure WebApp Extension](https://docs.datadoghq.com/serverless/azure_app_services/?tab=net#overview)

- [x] Azure - Datadog Integration installed
- [x] Extension added
- [x] Add env vars for logging: DD_API_KEY, DD_ENV, DD_SERVICE, DD_SITE, DD_VERSION

### Logging

[Agentless Logging Setup](https://docs.datadoghq.com/logs/log_collection/csharp/?tab=serilog#agentless-logging-with-apm)

We will be piggy backing off the APM to send the logs.

- [x] Add env vars DD_LOGS_DIRECT_SUBMISSION_INTEGRATIONS (ILogger),DD_LOGS_INJECTION

### Metrics

## Tracing

https://docs.datadoghq.com/tracing/setup_overview/setup/dotnet-framework/?tab=code
https://docs.datadoghq.com/tracing/setup_overview/custom_instrumentation/dotnet/

- [x] Env vars: DD_TRACE_ENABLED
- [x] Nuget package Datadog.Trace
- [x] Place a span

## Troubleshooting

- https://docs.datadoghq.com/tracing/faq/why-cant-i-see-my-correlated-logs-in-the-trace-id-panel/?tab=withlogintegration