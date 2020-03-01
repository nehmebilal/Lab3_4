using FooWebApp.Client;
using FooWebApp.IntegrationTests;
using System;

namespace FooWebApp.DeploymentTests
{
    public class DeploymentTestsFixture : IEndToEndTestsFixture
    {
        public DeploymentTestsFixture()
        {
            string serviceUrl = Environment.GetEnvironmentVariable("StudentsServiceDeploymentTestsUrl");
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new Exception("Could not find StudentsServiceUrl environment variable");
            }

            FooServiceClient = new FooServiceClient(new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(serviceUrl)
            });
        }
        public IFooServiceClient FooServiceClient { get; }
    }
}
