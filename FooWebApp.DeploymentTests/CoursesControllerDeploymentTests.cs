using FooWebApp.IntegrationTests;

namespace FooWebApp.DeploymentTests
{
    public class CoursesControllerDeploymentTests : CoursesControllerEndToEndTests
        <DeploymentTestsFixture>
    {
        public CoursesControllerDeploymentTests(DeploymentTestsFixture fixture) : base(fixture)
        {
        }
    }
}
