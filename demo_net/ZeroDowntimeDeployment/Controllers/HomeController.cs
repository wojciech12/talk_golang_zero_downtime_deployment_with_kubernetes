using Microsoft.AspNetCore.Mvc;

namespace ZeroDowntimeDeployment.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("/model")]
        public ActionResult Get()
        {
            return Ok();
        }
    }
}
