using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{

    public class HomeController : Controller
    {
        public IActionResult Index()

        {
            var db = new MyContext();
            var data = db.Table1s.ToList(); //Exception is thrown here
            return View();
        }

    }
}
