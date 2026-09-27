using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TPLudoteca.Models;

namespace TPLudoteca.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<CategoriaJuegoVM> CategoriaJuegos { get; set; }
        public DbSet<JuegoVM> Juegos { get; set; }
        public DbSet<AlquilerVM> Alquileres { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); 

            builder.Entity<CategoriaJuegoVM>().OwnsOne(c => c.Audit);

            builder.Entity<JuegoVM>()
                .HasOne(j => j.CategoriaJuegoVM)
                .WithMany()
                .HasForeignKey(j => j.IdCategoriaJuego);
            builder.Entity<JuegoVM>().OwnsOne(j => j.Audit);

            builder.Entity<AlquilerVM>()
                .HasOne<JuegoVM>()
                .WithMany()
                .HasForeignKey(a => a.IdJuego);
            builder.Entity<AlquilerVM>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.IdUsuario);
            builder.Entity<AlquilerVM>().OwnsOne(a => a.Audit);
        }
    }
}