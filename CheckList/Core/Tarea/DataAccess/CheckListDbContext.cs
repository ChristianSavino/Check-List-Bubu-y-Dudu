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

            modelBuilder.Entity<TareaEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(255);

                // Guardar el enum como string legible en la DB
                entity.Property(e => e.Tipo)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasConversion(
                        v => v.ToString().ToLower(),
                        v => Enum.Parse<TipoTarea>(v, true)
                    );

                entity.Property(e => e.DiaSemana)
                    .HasMaxLength(10)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString() : null,
                         v => v != null ? (DayOfWeek?)Enum.Parse<DayOfWeek>(v, true) : null
                    );

                entity.Property(e => e.Hora).HasMaxLength(5);
                entity.Property(e => e.Persona).HasMaxLength(50);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

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