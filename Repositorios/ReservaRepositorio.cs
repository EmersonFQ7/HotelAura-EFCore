
using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    // Listar reservas con huésped, habitación y tipo
    public async Task<List<Reserva>> ListarAsync()
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();
    }

    // Buscar reserva por ID
    public async Task<Reserva?> ObtenerPorIdAsync(int id)
    {
        return await _context.Reservas
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdReserva == id);
    }

    // Habitaciones disponibles en un rango de fechas
    public async Task<List<Habitacion>> DisponiblesAsync(
        DateOnly ingreso, DateOnly salida)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo &&
                !h.Reservas.Any(r =>
                    r.Estado != "Cancelada" &&
                    r.FechaIngreso < salida &&
                    r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    // Comprobar si existe un cruce de reservas
    public async Task<bool> ExisteCruceAsync(
        int idHabitacion,
        DateOnly ingreso,
        DateOnly salida)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r =>
                r.IdHabitacion == idHabitacion &&
                r.Estado != "Cancelada" &&
                r.FechaIngreso < salida &&
                r.FechaSalida > ingreso);
    }

    // Huéspedes activos para el formulario
    public async Task<List<Huesped>> ListarHuespedesAsync()
    {
        return await _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellidos)
            .ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    // Habitaciones activas para el formulario
    public async Task<List<Habitacion>> ListarHabitacionesAsync()
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    // Comprobar que el huésped esté activo
    public async Task<bool> HuespedActivoAsync(int id)
    {
        return await _context.Huespeds
            .AsNoTracking()
            .AnyAsync(h => h.IdHuesped == id && h.Activo);
    }

    // Comprobar que la habitación esté activa
    public async Task<bool> HabitacionActivaAsync(int id)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .AnyAsync(h => h.IdHabitacion == id && h.Activo);
    }

    // Crear reserva
    public async Task CrearAsync(Reserva reserva)
    {
        reserva.Estado = "Pendiente";
        reserva.FechaRegistro = DateTime.Now;

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
    }

    // Cancelar reserva
    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await _context.Reservas
            .FirstOrDefaultAsync(r =>
                r.IdReserva == id &&
                (r.Estado == "Pendiente" ||
                 r.Estado == "Confirmada"));

        if (reserva == null)
            return false;

        reserva.Estado = "Cancelada";

        await _context.SaveChangesAsync();
        return true;
    }
}
