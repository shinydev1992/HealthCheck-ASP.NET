using HealthChecks.Elasticsearch;

namespace UnitTests
{
    public class elastic_search_healthcheck_should
    {
        [Fact]
        public void create_client_with_user_configured_request_timeout()
        {
            var services = new ServiceCollection();
            var settings = new ElasticsearchOptions();
            services.AddHealthChecks().AddElasticsearch(setup =>
            {
                setup = settings;
                setup.RequestTimeout = new TimeSpan(0, 0, 6);
            });

            //Ensure no further modifications were carried by extension method
            settings.RequestTimeout.ShouldNotBeNull();
            settings.RequestTimeout.ShouldBe(new TimeSpan(0, 0, 6));
        }

        [Fact]
        public void create_client_with_configured_healthcheck_timeout_when_no_request_timeout_is_configured()
        {
            var services = new ServiceCollection();
            var settings = new ElasticsearchOptions();
            services.AddHealthChecks().AddElasticsearch(setup => settings = setup, timeout: new TimeSpan(0, 0, 7));

            settings.RequestTimeout.ShouldNotBeNull();
            settings.RequestTimeout.ShouldBe(new TimeSpan(0, 0, 7));
        }

        [Fact]
        public void create_client_with_no_timeout_when_no_option_is_configured()
        {
            var services = new ServiceCollection();
            var settings = new ElasticsearchOptions();
            services.AddHealthChecks().AddElasticsearch(setup => settings = setup);

            settings.RequestTimeout.ShouldBeNull();
        }
    }
}
