using Microsoft.AspNetCore.Mvc;

namespace APAEApplication.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username)
        {
            if (!string.IsNullOrEmpty(username) && username.ToUpper() == "APAE")
            {
                // Redireciona para a nova Home do Admin
                return RedirectToAction("Dashboard");
            }

            return RedirectToAction("Index", "Event");
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}