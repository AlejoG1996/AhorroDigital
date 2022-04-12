using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AhorroDigital.API.Data;
using AhorroDigital.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;

namespace AhorroDigital.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TypeOfSavingsController : Controller
    {
        private readonly DataContext _context;

        public TypeOfSavingsController(DataContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            return View(await _context.typeOfSavings.ToListAsync());
        }

     

        
        public IActionResult Create()
        {
            return View();
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( TypeOfSaving typeOfSaving)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(typeOfSaving);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe este tipo de Ahorro.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            return View(typeOfSaving);
        }

       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeOfSaving = await _context.typeOfSavings.FindAsync(id);
            if (typeOfSaving == null)
            {
                return NotFound();
            }
            return View(typeOfSaving);
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TypeOfSaving typeOfSaving)
        {
            if (id != typeOfSaving.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(typeOfSaving);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe este tipo de Ahorro.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }
            return View(typeOfSaving);
        }

      
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TypeOfSaving typeOfSaving = await _context.typeOfSavings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (typeOfSaving == null)
            {
                return NotFound();
            }

            _context.typeOfSavings.Remove(typeOfSaving);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

      
    }
}
