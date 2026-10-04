using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TPLudoteca.Data.Modelos;

namespace TPLudoteca.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<CategoriaJuego> CategoriaJuegos { get; set; }
        public DbSet<Juego> Juegos { get; set; }
        public DbSet<Alquiler> Alquileres { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CategoriaJuego>().OwnsOne(c => c.Audit);

            builder.Entity<Juego>()
                .HasOne(j => j.CategoriaJuego)
                .WithMany()
                .HasForeignKey(j => j.IdCategoriaJuego);
            builder.Entity<Juego>().OwnsOne(j => j.Audit);

            builder.Entity<Alquiler>()
                .HasOne(a => a.Juego)
                .WithMany()
                .HasForeignKey(a => a.IdJuego);
            builder.Entity<Alquiler>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.IdUsuario);
            builder.Entity<Alquiler>().OwnsOne(a => a.Audit);
        }
    }
}
