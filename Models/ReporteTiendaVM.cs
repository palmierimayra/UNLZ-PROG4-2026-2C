namespace TPLudoteca.Models
{
    public class ReporteTiendaVM
    {
        public int CantidadSocios { get; set; }
        public int SociosActivos { get; set; }
        public int SociosConAlquilerEnCurso { get; set; }
        public int CantidadJuegos { get; set; }
        public int CopiasTotales { get; set; }
        public int CopiasAlquiladas { get; set; }
        public int AlquileresVencidos { get; set; }
        public int TotalAlquileres { get; set; }
        public string? JuegoMasAlquilado { get; set; }
        public int VecesJuegoMasAlquilado { get; set; }
        public decimal IngresosDelMes { get; set; }
        public decimal RecaudadoPorMora { get; set; }
        public List<string> CategoriasNombres { get; set; } = new List<string>();
        public List<int> CategoriasCantidades { get; set; } = new List<int>();
        public List<string> TopJuegosNombres { get; set; } = new List<string>();
        public List<int> TopJuegosCantidades { get; set; } = new List<int>();
        public List<string> MesesNombres { get; set; } = new List<string>();
        public List<int> MesesCantidades { get; set; } = new List<int>();
    }
}
