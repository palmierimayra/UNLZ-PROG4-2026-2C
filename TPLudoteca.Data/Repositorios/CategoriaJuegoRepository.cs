using TPLudoteca.Data.Modelos;

namespace TPLudoteca.Data.Repositorios
{
    public interface ICategoriaJuegoRepository
    {
        List<CategoriaJuego> ObtenerCategorias();
        CategoriaJuego? ObtenerCategoriaPorId(int idCategoriaJuego);
        bool AgregarCategoria(CategoriaJuego categoriaJuego);
        void ActualizarCategoria(CategoriaJuego categoriaJuego);
        bool BorrarCategoria(CategoriaJuego categoriaJuego);
    }

    public class CategoriaJuegoRepository : ICategoriaJuegoRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoriaJuegoRepository(ApplicationDbContext context)
        {
            _db = context;
        }

        public List<CategoriaJuego> ObtenerCategorias()
        {
            return _db.CategoriaJuegos.ToList();
        }

        public CategoriaJuego? ObtenerCategoriaPorId(int idCategoriaJuego)
        {
            return _db.CategoriaJuegos.Find(idCategoriaJuego);
        }

        public bool AgregarCategoria(CategoriaJuego categoriaJuego)
        {
            try
            {
                _db.CategoriaJuegos.Add(categoriaJuego);
                _db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ActualizarCategoria(CategoriaJuego categoriaJuego)
        {
            _db.CategoriaJuegos.Update(categoriaJuego);
            _db.SaveChanges();
        }

        public bool BorrarCategoria(CategoriaJuego categoriaJuego)
        {
            try
            {
                    _db.CategoriaJuegos.Update(categoriaJuego);
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
