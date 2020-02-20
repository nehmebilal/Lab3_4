using FooWebApp.Client;
using Microsoft.AspNetCore.TestHost;

namespace FooWebApp.IntegrationTests
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
