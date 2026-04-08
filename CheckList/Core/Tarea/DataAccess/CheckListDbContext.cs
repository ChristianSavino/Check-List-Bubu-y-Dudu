using Microsoft.EntityFrameworkCore;
using CheckList.Core.Tarea.Domain;

namespace CheckList.Core.Tarea.DataAccess
{
    public class CheckListDbContext : DbContext
    {
        public CheckListDbContext(DbContextOptions<CheckListDbContext> options) : base(options)
        {
        }

        public DbSet<TareaEntity> Tareas { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar Tareas
            modelBuilder.Entity<TareaEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Hora).HasMaxLength(5);
                entity.Property(e => e.Persona).HasMaxLength(50);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configurar AppSettings
            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.Key).IsUnique();
            });
        }
    }
}
