using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoodFix.Models;

namespace MoodFix.Controllers
{
    public class FixesController : Controller
    {
        private readonly MoodFixContext _context;
        private readonly IHostingEnvironment _env;
        private readonly string _rootpath;

        public FixesController(IHostingEnvironment env, MoodFixContext context)
        {
            _env = env;
            _rootpath = _env.WebRootPath;
            _context = context;
        }

        // GET: Fixes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Fix.OrderByDescending(m => m.ordering).ToListAsync());
        }

        // GET: Fixes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fix = await _context.Fix
                .FirstOrDefaultAsync(m => m.ID == id);
            if (fix == null)
            {
                return NotFound();
            }

            return View(fix);
        }

        // GET: Fixes/Create
        public IActionResult Create()
        {
            FixViewModel fvm = new FixViewModel();
            
            return View(fvm);
        }

        // POST: Fixes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FixViewModel fix)
        {
            if (ModelState.IsValid)
            {
                Fix fvm = new Fix
                {
                    ordering = fix.ordering,
                    heading = fix.heading,
                    maintext = fix.maintext,
                    nextvalue = fix.nextvalue,
                    backvalue = fix.backvalue
                };

                IFormFile img = fix.image;
                if (img != null)
                {
                    var filePath = Path.Combine(_rootpath, "images", img.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }
                    //The file has been saved to disk - now save the file name to the DB
                    fvm.image = img.FileName;
                }

                _context.Add(fvm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fix);
        }

        // GET: Fixes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fix = await _context.Fix.FindAsync(id);
            if (fix == null)
            {
                return NotFound();
            }
            FixViewModel fvm = new FixViewModel
            {
                ordering = fix.ordering,
                heading = fix.heading,
                maintext = fix.maintext,
                nextvalue = fix.nextvalue,
                backvalue = fix.backvalue
            };
            return View(fvm);
        }

        // POST: Fixes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FixViewModel fvm)
        {
            if (id != fvm.ID)
            {
                return NotFound();
            }
            //Initialise a new companysegment
            Fix fix = await _context.Fix.FindAsync(id);
            if (ModelState.IsValid)
            {
                try
                {
                    
                    fix.ID = id;
                    fix.ordering = fvm.ordering;
                    fix.heading = fvm.heading;
                    fix.maintext = fvm.maintext;
                    fix.nextvalue = fvm.nextvalue;
                    fix.backvalue = fvm.backvalue;

                    IFormFile img = fvm.image;
                    if (img != null)
                    {
                        var filePath = Path.Combine(_rootpath, "images", img.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await img.CopyToAsync(stream);
                        }
                        //The file has been saved to disk - now save the file name to the DB
                        fix.image = img.FileName;
                    }

                    _context.Update(fix);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FixExists(fix.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fix);
        }

        // GET: Fixes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fix = await _context.Fix
                .FirstOrDefaultAsync(m => m.ID == id);
            if (fix == null)
            {
                return NotFound();
            }

            return View(fix);
        }

        // POST: Fixes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fix = await _context.Fix.FindAsync(id);
            _context.Fix.Remove(fix);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FixExists(int id)
        {
            return _context.Fix.Any(e => e.ID == id);
        }
    }
}