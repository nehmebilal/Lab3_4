using FooWebApp.IntegrationTests;

namespace FooWebApp.DeploymentTests
{
    public class StudentsControllerDeploymentTests : StudentsControllerEndToEndTests<DeploymentTestsFixture>
    {
        public StudentsControllerDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}
