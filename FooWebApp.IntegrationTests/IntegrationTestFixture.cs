using FooWebApp;
using FooWebApp.Client;
using Microsoft.AspNetCore.TestHost;

namespace FooWebAppIntegrationTests
{
    public class IntegrationTestFixture
    {
        public IntegrationTestFixture()
        {
            TestServer testServer = new TestServer(Program.CreateWebHostBuilder(new string[] { }));
            var httpClient = testServer.CreateClient();
            FooServiceClient = new FooServiceClient(httpClient);
        }

        public IFooServiceClient FooServiceClient { get; }
    }
}
