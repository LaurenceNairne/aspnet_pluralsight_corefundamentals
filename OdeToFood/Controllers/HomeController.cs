using Microsoft.AspNetCore.Mvc;
using OdeToFood.Models;

namespace OdeToFood.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new Restaurant
            {
                Id = 1,
                Name = "Scott's Pizza Place"
            };

            return View(model);
        }
    }
}
