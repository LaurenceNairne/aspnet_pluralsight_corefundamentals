using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OdeToFood.Controllers
{
    [Route("pages/[controller]/bob/[action]")]
    public class AboutController : Controller
    {
        public string Phone()
        {
            return "+44 07777 777 777";
        }

        public string Address()
        {
            return "123 Fake Street,\nMadeupville,\nNowhere";
        }
    }
}
