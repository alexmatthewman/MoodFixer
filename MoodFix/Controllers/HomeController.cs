using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoodFix.Models;
using Microsoft.EntityFrameworkCore;

namespace MoodFix.Controllers
{
    public class HomeController : Controller
    {
        private readonly MoodFixContext _context;

        public HomeController(MoodFixContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult WhichIsYou()
        {
            return View();
        }

        public IActionResult Sad()
        {
            return View();
        }

        public IActionResult SadHeartbreak()
        {
            return View();
        }

        public IActionResult SadLoss()
        {
            return View();
        }

        public IActionResult SadDepressed()
        {
            return View();
        }

        public IActionResult SadGeneral()
        {
            return View();
        }

        public IActionResult Angry()
        {
            return View();
        }

        public IActionResult AngryFamily()
        {
            return View();
        }

        public IActionResult AngryFriends()
        {
            return View();
        }

        public IActionResult AngryWork()
        {
            return View();
        }

        public IActionResult AngryPartner()
        {
            return View();
        }

        public IActionResult AngryGeneral()
        {
            return View();
        }

        public IActionResult Frustrated()
        {
            return View();
        }

        public IActionResult FrustratedInjustice()
        {
            return View();
        }

        public IActionResult FrustratedThoughtlessness()
        {
            return View();
        }

        public IActionResult FrustratedGeneral()
        {
            return View();
        }

        public IActionResult Anxious()
        {
            return View();
        }

        public IActionResult Ennui()
        {
            return View();
        }

        public IActionResult Blurg()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public JsonResult GetNextFix(string order)
        {
            if (!string.IsNullOrWhiteSpace(order))
            {
                var suc = int.TryParse(order, out int ord);
                Fix tester = _context.Fix.FirstOrDefault(m => m.order == ord);
                return Json(tester);
            }
            return null;
        }
    }
}
