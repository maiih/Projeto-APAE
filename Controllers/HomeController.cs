using Microsoft.AspNetCore.Mvc;

namespace APAEApplication.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}