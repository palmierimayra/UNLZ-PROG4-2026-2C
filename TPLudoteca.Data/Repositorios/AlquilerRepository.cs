using Microsoft.EntityFrameworkCore;
using TPLudoteca.Data.Modelos;

namespace TPLudoteca.Data.Repositorios
{
    public interface IAlquilerRepository
    {
        List<Alquiler> ObtenerAlquileres();
        List<Alquiler> ObtenerAlquileresPorUsuario(string idUsuario);
        Alquiler? ObtenerAlquilerPorId(int idAlquiler);
        bool AgregarAlquiler(Alquiler alquiler);
        void ActualizarAlquiler(Alquiler alquiler);
        bool BorrarAlquiler(Alquiler alquiler);
    }

    public class AlquilerRepository : IAlquilerRepository
    {
        private readonly ApplicationDbContext _db;

        public AlquilerRepository(ApplicationDbContext context)
        {
            _db = context;
        }

        public List<Alquiler> ObtenerAlquileres()
        {
            return _db.Alquileres
                .Include(a => a.Juego)
                .ThenInclude(j => j!.CategoriaJuego)
                .OrderByDescending(a => a.FechaRetiro)
                .ToList();
        }

        public List<Alquiler> ObtenerAlquileresPorUsuario(string idUsuario)
        {
            return _db.Alquileres
                .Include(a => a.Juego)
                .Where(a => a.IdUsuario == idUsuario)
                .OrderByDescending(a => a.FechaRetiro)
                .ToList();
        }

        public Alquiler? ObtenerAlquilerPorId(int idAlquiler)
        {
            return _db.Alquileres
                .Include(a => a.Juego)
                .FirstOrDefault(a => a.IdAlquiler == idAlquiler);
        }

        public bool AgregarAlquiler(Alquiler alquiler)
        {
            try
            {
                _db.Alquileres.Add(alquiler);
                _db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ActualizarAlquiler(Alquiler alquiler)
        {
            _db.Alquileres.Update(alquiler);
            _db.SaveChanges();
        }

        public bool BorrarAlquiler(Alquiler alquiler)
        {
            try
            {
                _db.Alquileres.Update(alquiler);
                _db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
