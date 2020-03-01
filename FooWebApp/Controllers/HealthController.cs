using Microsoft.AspNetCore.Mvc;

namespace FooWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        public IActionResult IsHealthy()
        {
            return Ok();
        }
    }
}
