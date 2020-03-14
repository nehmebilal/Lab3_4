using FooWebApp.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace FooWebApp.IntegrationTests
{
    public class IntegrationTestFixture : IEndToEndTestsFixture
    {
        public IntegrationTestFixture()
        {
            TestServer testServer = new TestServer(Program.CreateWebHostBuilder(new string[] { }).UseEnvironment("Development"));
            var httpClient = testServer.CreateClient();
            FooServiceClient = new FooServiceClient(httpClient);
        }

        public IFooServiceClient FooServiceClient { get; }
    }
}
