// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using Microsoft.AspNetCore.Mvc;

    [Route("")]
    public class DefaultController : Controller
    {
        [HttpGet]
        public IActionResult Index() =>
            this.View();
    }
}
