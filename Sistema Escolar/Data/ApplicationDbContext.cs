using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Models;
using SistemaEscolar.Models.Auth;
using SistemaEscolar.Models.Academico;
using SistemaEscolar.Models.Bitacora;

namespace SistemaEscolar.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablas de seguridad/usuarios
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UsuarioRol> UsuarioRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permiso> Permisos { get; set; } // agregado
        public DbSet<RolPermiso> RolPermisos { get; set; } // agregado

        // Tablas académicas
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Cuatrimestre> Cuatrimestres { get; set; }
        public DbSet<CursoDocente> CursoDocentes { get; set; }
        public DbSet<CursoOferta> CursoOfertas { get; set; }
        public DbSet<CursoOfertaDocente> CursoOfertaDocentes { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }
        public DbSet<Evaluacion> Evaluaciones { get; set; }
        public DbSet<HorarioCurso> HorariosCurso { get; set; } // agregado

        // Bitácora
        public DbSet<BitacoraEntry> BitacoraEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Evaluación: precision
            modelBuilder.Entity<Evaluacion>()
                .Property(e => e.Nota)
                .HasPrecision(5, 2);

            // UsuarioRol — PK compuesta
            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.UsuarioId, ur.RolId });

            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.UsuarioId);

            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.RolId);

            // CursoOferta — PK autogenerada
            modelBuilder.Entity<CursoOferta>()
                .HasOne(co => co.Curso)
                .WithMany()
                .HasForeignKey(co => co.CursoId);

            modelBuilder.Entity<CursoOferta>()
                .HasOne(co => co.Cuatrimestre)
                .WithMany()
                .HasForeignKey(co => co.CuatrimestreId);

            modelBuilder.Entity<CursoOfertaDocente>()
                .HasKey(cod => new { cod.CursoOfertaId, cod.DocenteId });

            modelBuilder.Entity<CursoOfertaDocente>()
                .HasOne(cod => cod.CursoOferta)
                .WithMany(co => co.CursoOfertaDocentes)
                .HasForeignKey(cod => cod.CursoOfertaId);

            modelBuilder.Entity<CursoOfertaDocente>()
                .HasOne(cod => cod.Docente)
                .WithMany()
                .HasForeignKey(cod => cod.DocenteId);

            // RolPermiso — PK compuesta
            modelBuilder.Entity<RolPermiso>()
                .HasKey(rp => new { rp.RolId, rp.PermisoId });

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.RolPermisos)
                .HasForeignKey(rp => rp.RolId);

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolPermisos)
                .HasForeignKey(rp => rp.PermisoId);

            modelBuilder.Entity<Permiso>()
                .HasIndex(p => p.Codigo)
                .IsUnique();

            // Indice único para Código de Curso
            modelBuilder.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            // Relaciones de auditoría de Curso
            modelBuilder.Entity<Curso>()
                .HasOne<Usuario>(c => c.CreadoPor)
                .WithMany()
                .HasForeignKey(c => c.CreadoPorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Curso>()
                .HasOne<Usuario>(c => c.ModificadoPor)
                .WithMany()
                .HasForeignKey(c => c.ModificadoPorId)
                .OnDelete(DeleteBehavior.NoAction);

            // CursoDocente — PK compuesta
            modelBuilder.Entity<CursoDocente>()
                .HasKey(cd => new { cd.CursoId, cd.DocenteId });

            modelBuilder.Entity<CursoDocente>()
                .HasOne(cd => cd.Curso)
                .WithMany(c => c.CursoDocentes)
                .HasForeignKey(cd => cd.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CursoDocente>()
                .HasOne(cd => cd.Docente)
                .WithMany()
                .HasForeignKey(cd => cd.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Matrícula
            modelBuilder.Entity<Matricula>()
                .HasOne(m => m.Estudiante)
                .WithMany()
                .HasForeignKey(m => m.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Nota: Matricula ahora referencia CursoOfertaId; mantener FK a Curso y Cuatrimestre opcional para compatibilidad
            modelBuilder.Entity<Matricula>()
                .HasOne(m => m.Curso)
                .WithMany(c => c.Matriculas)
                .HasForeignKey(m => m.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Matricula>()
                .HasOne(m => m.Cuatrimestre)
                .WithMany(cu => cu.Matriculas)
                .HasForeignKey(m => m.CuatrimestreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Evaluación
            modelBuilder.Entity<Evaluacion>()
                .HasOne(e => e.Matricula)
                .WithMany(m => m.Evaluaciones)
                .HasForeignKey(e => e.MatriculaId)
                .OnDelete(DeleteBehavior.Cascade);

            // RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.Usuario)
                .WithMany()
                .HasForeignKey(rt => rt.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bitácora
            modelBuilder.Entity<BitacoraEntry>()
                .HasOne(b => b.Usuario)
                .WithMany(u => u.Bitacoras)
                .HasForeignKey(b => b.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Horarios: unique por Curso + Dia + Inicio
            modelBuilder.Entity<HorarioCurso>()
                .HasIndex(h => new { h.CursoId, h.DiaSemana, h.HoraInicio })
                .IsUnique();
        }
    }
}
