
using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _repositorio;

    public HabitacionesController(HabitacionRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: Habitaciones
    public async Task<IActionResult> Index(int? idTipo)
    {
        ViewBag.Tipos = new SelectList(
            await _repositorio.ListarTiposAsync(),
            "IdTipoHabitacion",
            "Nombre",
            idTipo
        );

        ViewBag.IdTipo = idTipo;

        var habitaciones = await _repositorio.ListarAsync(idTipo);

        return View(habitaciones);
    }

    // GET: Habitaciones/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var habitacion = await _repositorio.ObtenerPorIdAsync(id);

        if (habitacion == null)
            return NotFound();

        return View(habitacion);
    }

    // GET: Habitaciones/Create
    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new Habitacion { Activo = true });
    }

    // POST: Habitaciones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        ModelState.Remove("IdTipoHabitacionNavigation");
        ModelState.Remove("Reservas");

        if (await _repositorio.ExisteNumeroAsync(habitacion.Numero))
        {
            ModelState.AddModelError(
                "Numero",
                "Ya existe una habitación con este número."
            );
        }

        if (ModelState.IsValid)
        {
            habitacion.Activo = true;

            await _repositorio.CrearAsync(habitacion);

            TempData["Exito"] = "Habitación registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    // GET: Habitaciones/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var habitacion = await _repositorio.ObtenerPorIdAsync(id);

        if (habitacion == null || !habitacion.Activo)
            return NotFound();

        await CargarTiposAsync(habitacion.IdTipoHabitacion);

        return View(habitacion);
    }

    // POST: Habitaciones/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion)
            return BadRequest();

        ModelState.Remove("IdTipoHabitacionNavigation");
        ModelState.Remove("Reservas");

        if (await _repositorio.ExisteNumeroAsync(
            habitacion.Numero, habitacion.IdHabitacion))
        {
            ModelState.AddModelError(
                "Numero",
                "Ya existe otra habitación con este número."
            );
        }

        if (ModelState.IsValid)
        {
            var actualizado = await _repositorio.EditarAsync(habitacion);

            if (!actualizado)
                return NotFound();

            TempData["Exito"] = "Habitación actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    // GET: Habitaciones/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var habitacion = await _repositorio.ObtenerPorIdAsync(id);

        if (habitacion == null || !habitacion.Activo)
            return NotFound();

        return View(habitacion);
    }

    // POST: Habitaciones/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var desactivado = await _repositorio.DesactivarAsync(id);

        if (!desactivado)
            return NotFound();

        TempData["Exito"] = "Habitación desactivada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // Cargar tipos de habitación en formularios
    private async Task CargarTiposAsync(int? seleccionado = null)
    {
        var tipos = await _repositorio.ListarTiposAsync();

        ViewBag.Tipos = new SelectList(
            tipos,
            "IdTipoHabitacion",
            "Nombre",
            seleccionado
        );
    }
}
