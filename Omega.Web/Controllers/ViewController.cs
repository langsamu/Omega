// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using Microsoft.AspNetCore.Mvc;

    [Route("view")]
    public class ViewController : Controller
    {
        public IActionResult Index() =>
            this.View();
    }
}
