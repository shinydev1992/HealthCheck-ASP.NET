using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace HealthChecks.Network.Tests.Functional
{
    public class tcp_healthcheck_should
    {
        [Fact]
        public async Task respect_configured_timeout_and_throw_operation_cancelled_exception()
        {
            var options = new TcpHealthCheckOptions();

            options.AddHost("invalid", 5555);
            options.AddressFamily = AddressFamily.InterNetworkV6;

            var tcpHealthCheck = new TcpHealthCheck(options);

            var result = await tcpHealthCheck.CheckHealthAsync(new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("tcp", instance: tcpHealthCheck, failureStatus: HealthStatus.Degraded,
                    null, timeout: null)
            }, new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

            result.Exception.Should().BeOfType<OperationCanceledException>();
        }
    }
}
