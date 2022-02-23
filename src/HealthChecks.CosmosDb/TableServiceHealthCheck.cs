using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CosmosDb
{
    public class TableServiceHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, TableServiceClient> _connections = new();

        private readonly string _connectionString;
        private readonly string _tableName;

        private readonly Uri _endpoint;
        private readonly TableSharedKeyCredential _credentials;

        public TableServiceHealthCheck(string connectionString, string tableName)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public TableServiceHealthCheck(Uri endpoint, TableSharedKeyCredential credentials, string tableName)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {

                var tableServiceKey = _connectionString ?? _endpoint.ToString();
                if (!_connections.TryGetValue(tableServiceKey, out var tableServiceClient))
                {
                    tableServiceClient = CreateTableServiceClient();

                    if (!_connections.TryAdd(tableServiceKey, tableServiceClient))
                    {
                        tableServiceClient = _connections[_connectionString];
                    }
                }
                var tableClient = tableServiceClient.GetTableClient(_tableName);
                _ = await tableClient.GetAccessPoliciesAsync();

                return HealthCheckResult.Healthy();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Table with name {_tableName} does not exist.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private TableServiceClient CreateTableServiceClient()
        {
            if (!string.IsNullOrEmpty(_connectionString))
            {
                return new TableServiceClient(_connectionString);
            }

            return new TableServiceClient(_endpoint, _credentials);
        }
    }
}
