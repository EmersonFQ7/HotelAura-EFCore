
using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _repositorio;

    public ReservasController(ReservaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    // GET: Reservas
    public async Task<IActionResult> Index()
    {
        var reservas = await _repositorio.ListarAsync();
        return View(reservas);
    }

    // GET: Reservas/Disponibles
    [HttpGet]
    public async Task<IActionResult> Disponibles(
        DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.Ingreso = ingreso;
        ViewBag.Salida = salida;

        if (!ingreso.HasValue && !salida.HasValue)
        {
            return View(new List<Habitacion>());
        }

        if (!ingreso.HasValue || !salida.HasValue)
        {
            ModelState.AddModelError(
                "",
                "Debes seleccionar ambas fechas.");

            return View(new List<Habitacion>());
        }

        if (ingreso.Value < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(
                "",
                "La fecha de ingreso no puede ser anterior a hoy.");

            return View(new List<Habitacion>());
        }

        if (salida.Value <= ingreso.Value)
        {
            ModelState.AddModelError(
                "",
                "La fecha de salida debe ser posterior al ingreso.");

            return View(new List<Habitacion>());
        }

        var habitaciones = await _repositorio
            .DisponiblesAsync(ingreso.Value, salida.Value);

        ViewBag.Noches = salida.Value.DayNumber -
                         ingreso.Value.DayNumber;

        return View(habitaciones);
    }

    // GET: Reservas/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CargarListasAsync();

        return View(new Reserva
        {
            FechaIngreso = DateOnly.FromDateTime(DateTime.Today),
            FechaSalida = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Estado = "Pendiente"
        });
    }

    // POST: Reservas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
        ModelState.Remove("IdHuespedNavigation");
        ModelState.Remove("IdHabitacionNavigation");
        ModelState.Remove("Estado");

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        if (reserva.FechaIngreso < hoy)
        {
            ModelState.AddModelError(
                nameof(reserva.FechaIngreso),
                "El ingreso no puede ser anterior a hoy.");
        }

        if (reserva.FechaSalida <= reserva.FechaIngreso)
        {
            ModelState.AddModelError(
                nameof(reserva.FechaSalida),
                "La salida debe ser posterior al ingreso.");
        }

        if (!await _repositorio.HuespedActivoAsync(reserva.IdHuesped))
        {
            ModelState.AddModelError(
                nameof(reserva.IdHuesped),
                "Selecciona un huésped activo.");
        }

        if (!await _repositorio.HabitacionActivaAsync(reserva.IdHabitacion))
        {
            ModelState.AddModelError(
                nameof(reserva.IdHabitacion),
                "Selecciona una habitación activa.");
        }

        if (reserva.FechaSalida > reserva.FechaIngreso &&
            reserva.FechaIngreso >= hoy &&
            await _repositorio.ExisteCruceAsync(
                reserva.IdHabitacion,
                reserva.FechaIngreso,
                reserva.FechaSalida))
        {
            ModelState.AddModelError(
                nameof(reserva.IdHabitacion),
                "La habitación ya está reservada en esas fechas.");
        }

        if (ModelState.IsValid)
        {
            reserva.Estado = "Pendiente";

            await _repositorio.CrearAsync(reserva);

            TempData["Exito"] = "Reserva registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        await CargarListasAsync(
            reserva.IdHuesped,
            reserva.IdHabitacion);

        return View(reserva);
    }

    // POST: Reservas/Cancelar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cancelada = await _repositorio.CancelarAsync(id);

        TempData[cancelada ? "Exito" : "Error"] =
            cancelada
                ? "Reserva cancelada correctamente."
                : "La reserva no se puede cancelar.";

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync(
        int? idHuesped = null,
        int? idHabitacion = null)
    {
        var huespedes = await _repositorio.ListarHuespedesAsync();
        var habitaciones = await _repositorio.ListarHabitacionesAsync();

        ViewBag.Huespedes = new SelectList(
            huespedes.Select(h => new
            {
                h.IdHuesped,
                NombreCompleto = h.Nombres + " " + h.Apellidos
            }),
            "IdHuesped",
            "NombreCompleto",
            idHuesped
        );

        ViewBag.Habitaciones = new SelectList(
            habitaciones.Select(h => new
            {
                h.IdHabitacion,
                Descripcion = h.Numero + " - " +
                              h.IdTipoHabitacionNavigation.Nombre
            }),
            "IdHabitacion",
            "Descripcion",
            idHabitacion
        );
    }
}
