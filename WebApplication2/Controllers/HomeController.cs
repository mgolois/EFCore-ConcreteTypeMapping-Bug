using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{

    public class HomeController : Controller
    {
        const string Session_Key1 = "myKey1";
        const string Session_Key2 = "myKey2";
        public IActionResult Index()
        {
           
            return View();
        }

        [HttpPost]
        public IActionResult Form(MyObject myObject, string email)
        {

            HttpContext.AddSession(Session_Key1, myObject);

            myObject.ObjectName += $"(Added at {DateTime.Now.ToShortTimeString()})";

            HttpContext.AddSession(Session_Key2, email);
            return RedirectToAction("Session");
        }
        public IActionResult Session()
        {

          var myObj =  HttpContext.GetSessionObject<MyObject>(Session_Key1);
            var email = HttpContext.GetSessionValue<string>(Session_Key2);
            ViewBag.ID = myObj.ObjectID;
            ViewBag.Name = myObj.ObjectName;
            ViewBag.Email = email;
            return View();
        }

    }
}
