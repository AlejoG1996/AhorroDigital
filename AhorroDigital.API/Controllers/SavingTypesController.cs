﻿using AhorroDigital.API.Data;
using AhorroDigital.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Vereyon.Web;

namespace AhorroDigital.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SavingTypesController:Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public SavingTypesController(DataContext context, IFlashMessage flasher)
        {
            _context = context;
            _flashMessage = flasher;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.SavingTypes.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavingType savingType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(savingType);
                    await _context.SaveChangesAsync();
                    _flashMessage.Info(string.Empty, "Se registro exitosamente el  tipo de ahorro.");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {

                    if (dbUpdateException.InnerException.Message.Contains("Duplicate"))
                    {
                        _flashMessage.Danger(string.Empty, "Ya existe este tipo de ahorro.");

                    }
                    else
                    {
                        _flashMessage.Danger(string.Empty, dbUpdateException.InnerException.Message);


                    }
                }
                catch (Exception exception)
                {
                    _flashMessage.Danger(string.Empty, exception.Message);


                }
            }

            return View(savingType);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SavingTypes == null)
            {
                return NotFound();
            }

            SavingType savingType = await _context.SavingTypes.FindAsync(id);
            if (savingType == null)
            {
                return NotFound();
            }
            return View(savingType);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SavingType savingType)
        {
            if (id != savingType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(savingType);
                    await _context.SaveChangesAsync();
                    _flashMessage.Info(string.Empty, "Se editó exitosamente el  tipo de ahorro.");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {

                    if (dbUpdateException.InnerException.Message.Contains("Duplicate"))
                    {
                        _flashMessage.Danger(string.Empty, "Ya existe este tipo de ahorro.");

                    }
                    else
                    {
                        _flashMessage.Danger(string.Empty, dbUpdateException.InnerException.Message);


                    }
                }
                catch (Exception exception)
                {
                    _flashMessage.Danger(string.Empty, exception.Message);


                }

            }
            return View(savingType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SavingType savingType = await _context.SavingTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (savingType == null)
            {
                return NotFound();
            }

            _context.SavingTypes.Remove(savingType);
            await _context.SaveChangesAsync();
            _flashMessage.Info(string.Empty, "Se elimino exitosamente el  tipo de ahorro.");
            return RedirectToAction(nameof(Index));

        }
    }
}