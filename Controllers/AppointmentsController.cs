using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClinicApp.Models;

namespace ClinicApp.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ClinicDbContext _context;

        public AppointmentsController(ClinicDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();

     
            foreach (var appointment in appointments)
            {
                if (appointment.Status != "Cancelled")
                {
                    if (appointment.AppointmentDate < DateTime.Now)
                        appointment.Status = "Completed";
                    else
                        appointment.Status = "Scheduled";
                }
            }

            await _context.SaveChangesAsync();
            return View(appointments);
        }

       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        
        public IActionResult Create()
        {
            ViewBag.PatientId = new SelectList(_context.Patients, "PatientId", "FullName");
            ViewBag.DoctorId = new SelectList(_context.Doctors, "DoctorId", "FullName");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentId,PatientId,DoctorId,AppointmentDate,Reason,Notes")] Appointment appointment)
        {
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                appointment.Status = "Scheduled";

                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            ViewBag.PatientId = new SelectList(_context.Patients, "PatientId", "FullName", appointment.PatientId);
            ViewBag.DoctorId = new SelectList(_context.Doctors, "DoctorId", "FullName", appointment.DoctorId);
            return View(appointment);
        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewBag.PatientId = new SelectList(_context.Patients, "PatientId", "FullName", appointment.PatientId);
            ViewBag.DoctorId = new SelectList(_context.Doctors, "DoctorId", "FullName", appointment.DoctorId);
            return View(appointment);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,PatientId,DoctorId,AppointmentDate,Reason,Notes")] Appointment appointment)
        {
            if (id != appointment.AppointmentId) return NotFound();

            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                try
                {
                    appointment.Status = "Scheduled";

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(e => e.AppointmentId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PatientId = new SelectList(_context.Patients, "PatientId", "FullName", appointment.PatientId);
            ViewBag.DoctorId = new SelectList(_context.Doctors, "DoctorId", "FullName", appointment.DoctorId);
            return View(appointment);
        }

        // GET: Appointments/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
                _context.Appointments.Remove(appointment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
