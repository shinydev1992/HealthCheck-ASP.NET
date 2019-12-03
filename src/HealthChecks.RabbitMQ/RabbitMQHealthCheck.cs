﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.RabbitMQ
{
    public class RabbitMQHealthCheck
        : IHealthCheck
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _rmqConnection;

        public RabbitMQHealthCheck(string rabbitMqConnectionString, SslOption sslOption = null)
        {
            if (rabbitMqConnectionString == null) throw new ArgumentNullException(nameof(rabbitMqConnectionString));


            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqConnectionString),
                AutomaticRecoveryEnabled = true // Explicitly setting to ensure this is true (in case the default changes)
            };

            if (sslOption != null)
            {
                connectionFactory.Ssl = sslOption;
            }

            _connectionFactory = connectionFactory;
        }

        public RabbitMQHealthCheck(IConnection connection)
        {
            _rmqConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public RabbitMQHealthCheck(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_rmqConnection != null && _rmqConnection.IsOpen == false)
                {
                    _rmqConnection.Close(0);
                    _rmqConnection = null;
                }
                if (_rmqConnection == null)
                {
                    _rmqConnection = CreateConnection(_connectionFactory);
                }

                return TestConnection(_rmqConnection);
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }

        private static Task<HealthCheckResult> TestConnection(IConnection connection)
        {
            using (connection.CreateModel())
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy());
            }
        }

        private static IConnection CreateConnection(IConnectionFactory connectionFactory)
        {
            return connectionFactory.CreateConnection("Health Check Connection");
        }
    }
}
