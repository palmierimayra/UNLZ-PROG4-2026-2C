using Microsoft.EntityFrameworkCore;
using TPLudoteca.Data.Modelos;

namespace TPLudoteca.Data.Repositorios
{
    public interface IJuegoRepository
    {
        List<Juego> ObtenerJuegos();
        Juego? ObtenerJuegoPorId(int idJuego);
        bool AgregarJuego(Juego juego);
        void ActualizarJuego(Juego juego);
        bool BorrarJuego(Juego juego);
    }

    public class JuegoRepository : IJuegoRepository
    {
        private readonly ApplicationDbContext _db;

        public JuegoRepository(ApplicationDbContext context)
        {
            _db = context;
        }

        public List<Juego> ObtenerJuegos()
        {
            return _db.Juegos
                .Include(j => j.CategoriaJuego)
                .ToList();
        }

        public Juego? ObtenerJuegoPorId(int idJuego)
        {
            return _db.Juegos
                .Include(j => j.CategoriaJuego)
                .FirstOrDefault(j => j.IdJuego == idJuego);
        }

        public bool AgregarJuego(Juego juego)
        {
            try
            {
                _db.Juegos.Add(juego);
                _db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ActualizarJuego(Juego juego)
        {
            _db.Juegos.Update(juego);
            _db.SaveChanges();
        }

        public bool BorrarJuego(Juego juego)
        {
            try
            {
                    _db.Juegos.Update(juego);
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
