using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Acme.BookStore.Controllers
{
    [AllowAnonymous]
    [Route("api/[Controller]")]
    public class DemoController : AbpController
    {
        public DemoController()
        {
            
        }
        [HttpGet]
        public IActionResult GetHello()
        {
            return Ok("Hello");
        }
    }
}
