using SistemaEscolar.Data;
using SistemaEscolar.Models;
using SistemaEscolar.Models.Academico;
using SistemaEscolar.Models.Auth;
using SistemaEscolar.Models.Bitacora;
using SistemaEscolar.Helpers;
using System.Linq;

namespace SistemaEscolar.Data
{
    public static class DataSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Asegurar roles base
            EnsureRole(context, "Administrador");
            EnsureRole(context, "Docente");
            EnsureRole(context, "Estudiante");
            EnsureRole(context, "Coordinador");
            context.SaveChanges();

            var adminRol = context.Roles.First(r => r.Nombre == "Administrador");
            var docenteRol = context.Roles.First(r => r.Nombre == "Docente");
            var estudianteRol = context.Roles.First(r => r.Nombre == "Estudiante");
            var coordinadorRol = context.Roles.First(r => r.Nombre == "Coordinador");

            // Permisos requeridos
            string[] permisoCodigos = new[]
            {
                "Usuarios.Gestion","Cursos.Ver","Cursos.Crear","Cursos.Editar","Cursos.Eliminar","Cursos.AsignarDocente","Historial.Ver","Bitacora.Ver","Seguridad.Gestion"
            };
            foreach (var cod in permisoCodigos)
                EnsurePermiso(context, cod, $"Permiso {cod}");
            context.SaveChanges();

            // Asignar permisos si faltan
            AssignAllPermissionsToRole(context, adminRol.Id);
            AssignPermissions(context, coordinadorRol.Id, new[]{"Cursos.Ver","Cursos.Crear","Cursos.Editar","Cursos.AsignarDocente","Historial.Ver"});
            AssignPermissions(context, docenteRol.Id, new[]{"Cursos.Ver","Historial.Ver"});
            AssignPermissions(context, estudianteRol.Id, new[]{"Historial.Ver"});
            context.SaveChanges();

            // Si ya existen usuarios, detener sólo creación inicial de datos académicos duplicados
            if (context.Usuarios.Any()) return;

            // Crear usuarios iniciales
            var adminUser = CrearUsuario("Admin", "Sistema", "admin@sistema.edu", "123456789", "Admin123!");
            var docente1 = CrearUsuario("Juan", "Pérez", "juan.perez@sistema.edu", "111111111", "Docente123!");
            var docente2 = CrearUsuario("María", "García", "maria.garcia@sistema.edu", "222222222", "Docente123!");
            var estudiante1 = CrearUsuario("Carlos", "López", "carlos.lopez@sistema.edu", "333333333", "Estudiante123!");
            var estudiante2 = CrearUsuario("Ana", "Martínez", "ana.martinez@sistema.edu", "444444444", "Estudiante123!");
            var coordinador = CrearUsuario("Luis", "Rodríguez", "luis.rodriguez@sistema.edu", "555555555", "Coordinador123!");
            context.Usuarios.AddRange(adminUser, docente1, docente2, estudiante1, estudiante2, coordinador);
            context.SaveChanges();

            context.UsuarioRoles.AddRange(
                new UsuarioRol { UsuarioId = adminUser.Id, RolId = adminRol.Id },
                new UsuarioRol { UsuarioId = docente1.Id, RolId = docenteRol.Id },
                new UsuarioRol { UsuarioId = docente2.Id, RolId = docenteRol.Id },
                new UsuarioRol { UsuarioId = estudiante1.Id, RolId = estudianteRol.Id },
                new UsuarioRol { UsuarioId = estudiante2.Id, RolId = estudianteRol.Id },
                new UsuarioRol { UsuarioId = coordinador.Id, RolId = coordinadorRol.Id }
            );
            context.SaveChanges();

            // Datos académicos iniciales
            var cuatrimestre1 = new Cuatrimestre { Nombre = "2024-1", Anio =2024 };
            var cuatrimestre2 = new Cuatrimestre { Nombre = "2024-2", Anio =2024 };
            var cuatrimestre3 = new Cuatrimestre { Nombre = "2025-1", Anio =2025 };
            context.Cuatrimestres.AddRange(cuatrimestre1, cuatrimestre2, cuatrimestre3);
            context.SaveChanges();

            var curso1 = CrearCurso("MAT101", "Matemáticas Básicas",3, cuatrimestre1.Id, adminUser.Id);
            var curso2 = CrearCurso("FIS101", "Física General",4, cuatrimestre1.Id, adminUser.Id);
            var curso3 = CrearCurso("QUI101", "Química Orgánica",3, cuatrimestre2.Id, adminUser.Id);
            var curso4 = CrearCurso("PRO101", "Programación I",4, cuatrimestre2.Id, adminUser.Id);
            context.Cursos.AddRange(curso1, curso2, curso3, curso4);
            context.SaveChanges();

            context.CursoDocentes.AddRange(
                new CursoDocente { CursoId = curso1.Id, DocenteId = docente1.Id, Activo = true },
                new CursoDocente { CursoId = curso2.Id, DocenteId = docente1.Id, Activo = true },
                new CursoDocente { CursoId = curso3.Id, DocenteId = docente2.Id, Activo = true },
                new CursoDocente { CursoId = curso4.Id, DocenteId = docente2.Id, Activo = true }
            );
            context.SaveChanges();

            var matricula1 = new Matricula { EstudianteId = estudiante1.Id, CursoId = curso1.Id, CuatrimestreId = cuatrimestre1.Id, FechaMatricula = DateTime.UtcNow.AddDays(-30) };
            var matricula2 = new Matricula { EstudianteId = estudiante1.Id, CursoId = curso2.Id, CuatrimestreId = cuatrimestre1.Id, FechaMatricula = DateTime.UtcNow.AddDays(-30) };
            var matricula3 = new Matricula { EstudianteId = estudiante2.Id, CursoId = curso3.Id, CuatrimestreId = cuatrimestre2.Id, FechaMatricula = DateTime.UtcNow.AddDays(-15) };
            var matricula4 = new Matricula { EstudianteId = estudiante2.Id, CursoId = curso4.Id, CuatrimestreId = cuatrimestre2.Id, FechaMatricula = DateTime.UtcNow.AddDays(-15) };
            context.Matriculas.AddRange(matricula1, matricula2, matricula3, matricula4);
            context.SaveChanges();

            context.Evaluaciones.AddRange(
                CrearEval(matricula1, docente1.Id,85.50m, "Buen desempeño", "Activa", "Aprobado"),
                CrearEval(matricula2, docente1.Id,92.00m, "Excelente trabajo", "Muy activa", "Aprobado"),
                CrearEval(matricula3, docente2.Id,78.25m, "Necesita mejorar en prácticas", "Regular", "Aprobado"),
                CrearEval(matricula4, docente2.Id,65.00m, "Debe esforzarse más", "Baja", "Reprobado")
            );
            context.SaveChanges();

            context.BitacoraEntries.AddRange(
                new BitacoraEntry { UsuarioId = adminUser.Id, Accion = "Seed: Creación de usuario", Modulo = "Usuarios", Ip = "127.0.0.1", Fecha = DateTime.UtcNow.AddDays(-1) },
                new BitacoraEntry { UsuarioId = docente1.Id, Accion = "Seed: Registro de evaluación", Modulo = "Evaluaciones", Ip = "127.0.0.1", Fecha = DateTime.UtcNow.AddHours(-6) }
            );
            context.SaveChanges();
        }

        private static void EnsureRole(ApplicationDbContext ctx, string nombre)
        {
            if (!ctx.Roles.Any(r => r.Nombre == nombre)) ctx.Roles.Add(new Rol { Nombre = nombre });
        }
        private static void EnsurePermiso(ApplicationDbContext ctx, string codigo, string desc)
        {
            if (!ctx.Permisos.Any(p => p.Codigo == codigo)) ctx.Permisos.Add(new Permiso { Codigo = codigo, Descripcion = desc });
        }
        private static void AssignPermissions(ApplicationDbContext ctx, int rolId, IEnumerable<string> codigos)
        {
            var permisoIds = ctx.Permisos.Where(p => codigos.Contains(p.Codigo)).Select(p => p.Id).ToList();
            foreach (var pid in permisoIds)
                if (!ctx.RolPermisos.Any(rp => rp.RolId == rolId && rp.PermisoId == pid))
                    ctx.RolPermisos.Add(new RolPermiso { RolId = rolId, PermisoId = pid });
        }
        private static void AssignAllPermissionsToRole(ApplicationDbContext ctx, int rolId)
        {
            var permisoIds = ctx.Permisos.Select(p => p.Id).ToList();
            foreach (var pid in permisoIds)
                if (!ctx.RolPermisos.Any(rp => rp.RolId == rolId && rp.PermisoId == pid))
                    ctx.RolPermisos.Add(new RolPermiso { RolId = rolId, PermisoId = pid });
        }
        private static Usuario CrearUsuario(string nombre, string apellidos, string email, string identificacion, string password)
        {
            PasswordHasher.CreatePasswordHash(password, out var hash, out var salt);
            return new Usuario
            {
                Nombre = nombre,
                Apellidos = apellidos,
                Email = email,
                Identificacion = identificacion,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                PasswordHash = hash,
                PasswordSalt = salt
            };
        }
        private static Curso CrearCurso(string codigo, string nombre, int creditos, int cuatrimestreId, int usuarioCreacion)
        {
            return new Curso
            {
                Codigo = codigo,
                Nombre = nombre,
                Descripcion = $"Curso de {nombre}",
                Creditos = creditos,
                CuatrimestreId = cuatrimestreId,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = usuarioCreacion
            };
        }
        private static Evaluacion CrearEval(Matricula matricula, int usuarioRegistro, decimal nota, string obs, string participacion, string estado)
        {
            return new Evaluacion
            {
                MatriculaId = matricula.Id,
                Matricula = matricula,
                UsuarioRegistro = usuarioRegistro,
                Nota = nota,
                Observaciones = obs,
                Participacion = participacion,
                Estado = estado,
                FechaRegistro = DateTime.UtcNow
            };
        }
    }
}