using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class HttpLogFixture : IntegrationTest
    {
        [Test]
        public void should_log_on_error()
        {
            var config = HostConfig.Get(1);
            config.LogLevel = "Trace";
            HostConfig.Put(config);

            Thread.Sleep(500);

            var logFile = Path.Combine(_runner.AppData, "logs", "Lidarr.trace.txt");
            var logLines = File.ReadAllLines(logFile);

            var result = Artist.InvalidPost(new Lidarr.Api.V1.Artist.ArtistResource());

            logLines = File.ReadAllLines(logFile).Skip(logLines.Length).ToArray();

            logLines.Should().Contain(v => v.Contains("|Trace|Http|Req"));
            logLines.Should().Contain(v => v.Contains("|Trace|Http|Res"));
            logLines.Should().Contain(v => v.Contains("|Debug|Api|"));
        }
    }
}
