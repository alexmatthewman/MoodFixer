using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AIRelief.Models;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Controllers
{
    public class HomeController : Controller
    {
        private readonly AIReliefContext _context;

        public HomeController(AIReliefContext context)
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

        public IActionResult CausalReasoning()
        {
            return View();
        }

        public IActionResult CognitiveReflection()
        {
            return View();
        }

        public IActionResult Metacognition()
        {
            return View();
        }

        public IActionResult ReadingComprehension()
        {
            return View();
        }

        public IActionResult ShortTermMemory()
        {
            return View();
        }

        public IActionResult ConfidenceCalibration()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}
