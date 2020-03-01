using FooWebApp.Client;

namespace FooWebApp.IntegrationTests
{
    public interface IEndToEndTestsFixture
    {
        public IFooServiceClient FooServiceClient { get; }
    }
}
