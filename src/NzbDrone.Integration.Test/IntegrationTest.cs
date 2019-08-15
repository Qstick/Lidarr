using NLog;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Test.Common;
using Lidarr.Http.ClientSchema;

namespace NzbDrone.Integration.Test
{
    public abstract class IntegrationTest : IntegrationTestBase
    {
        protected NzbDroneRunner _runner;

        public override string ArtistRootFolder => GetTempDirectory("ArtistRootFolder");

        protected override string RootUrl => "http://localhost:8686/";

        protected override string ApiKey => _runner.ApiKey;

        protected override void StartTestTarget()
        {
            _runner = new NzbDroneRunner(LogManager.GetCurrentClassLogger());
            _runner.KillAll();

            _runner.Start();
        }

        protected override void InitializeTestTarget()
        {
            Indexers.Post(new Lidarr.Api.V1.Indexers.IndexerResource
            {
                EnableRss = false,
                EnableInteractiveSearch = false,
                EnableAutomaticSearch = false,
                ConfigContract = nameof(NewznabSettings),
                Implementation = nameof(Newznab),
                Name = "NewznabTest",
                Protocol = Core.Indexers.DownloadProtocol.Usenet,
                Fields = SchemaBuilder.ToSchema(new NewznabSettings())
            });

            // Change Console Log Level to Debug so we get more details.
            // Disable sentry so we don't get events for unreleased code.
            var config = HostConfig.Get(1);
            config.ConsoleLogLevel = "Debug";
            config.AnalyticsEnabled = false;
            HostConfig.Put(config);
        }

        protected override void StopTestTarget()
        {
            _runner.KillAll();
        }
    }
}
