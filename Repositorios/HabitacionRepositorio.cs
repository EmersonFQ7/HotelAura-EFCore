
using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    // Listar habitaciones activas, filtradas y ordenadas
    public async Task<List<Habitacion>> ListarAsync(int? idTipo)
    {
        var consulta = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo);

        if (idTipo.HasValue)
        {
            consulta = consulta.Where(h =>
                h.IdTipoHabitacion == idTipo.Value);
        }

        return await consulta
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    // Buscar habitación por ID
    public async Task<Habitacion?> ObtenerPorIdAsync(int id)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);
    }

    // Obtener tipos activos para listas desplegables
    public async Task<List<TipoHabitacion>> ListarTiposAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    // Comprobar si ya existe un número
    public async Task<bool> ExisteNumeroAsync(
        string numero, int? excluirId = null)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .AnyAsync(h =>
                h.Numero == numero &&
                (!excluirId.HasValue ||
                 h.IdHabitacion != excluirId.Value));
    }

    // Registrar habitación
    public async Task CrearAsync(Habitacion habitacion)
    {
        _context.Habitacions.Add(habitacion);
        await _context.SaveChangesAsync();
    }

    // Actualizar habitación
    public async Task<bool> EditarAsync(Habitacion habitacion)
    {
        var existente = await _context.Habitacions
            .FirstOrDefaultAsync(h =>
                h.IdHabitacion == habitacion.IdHabitacion &&
                h.Activo);

        if (existente == null)
            return false;

        existente.Numero = habitacion.Numero;
        existente.Piso = habitacion.Piso;
        existente.IdTipoHabitacion = habitacion.IdTipoHabitacion;

        await _context.SaveChangesAsync();
        return true;
    }

    // Eliminación lógica
    public async Task<bool> DesactivarAsync(int id)
    {
        var habitacion = await _context.Habitacions
            .FirstOrDefaultAsync(h =>
                h.IdHabitacion == id && h.Activo);

        if (habitacion == null)
            return false;

        habitacion.Activo = false;

        await _context.SaveChangesAsync();
        return true;
    }
}
