using System;
using System.Linq;
using SistemaEscolar.Data;
using SistemaEscolar.Models;
using SistemaEscolar.Helpers;

namespace SistemaEscolar.Data
{
    public static class DataSeeder
    {
        public static void Seed(ApplicationDbContext ctx)
        {
            // If any users exist, assume seeded
            if (ctx.Usuarios.Any()) return;

            // Roles
            var rolAdmin = new Rol { Nombre = "Administrador" };
            var rolDocente = new Rol { Nombre = "Docente" };
            var rolEst = new Rol { Nombre = "Estudiante" };
            ctx.Roles.AddRange(rolAdmin, rolDocente, rolEst);
            ctx.SaveChanges();

            // Permisos
            var permisos = new[] {
                new Permiso { Codigo = "Usuarios.Gestion", Descripcion = "Gestionar usuarios" },
                new Permiso { Codigo = "Cursos.Ver", Descripcion = "Ver cursos" },
                new Permiso { Codigo = "Cursos.AsignarDocente", Descripcion = "Asignar docentes" },
                new Permiso { Codigo = "Historial.Ver", Descripcion = "Ver historial" },
                new Permiso { Codigo = "Bitacora.Ver", Descripcion = "Ver bitacora" },
                new Permiso { Codigo = "Evaluaciones.Crear", Descripcion = "Crear evaluaciones" }
            };
            ctx.Permisos.AddRange(permisos);
            ctx.SaveChanges();

            // RolPermisos
            ctx.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisos[0].Id });
            ctx.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisos[1].Id });
            ctx.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisos[2].Id });
            ctx.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisos[3].Id });
            ctx.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisos[4].Id });
            ctx.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = permisos[5].Id });

            ctx.RolPermisos.Add(new RolPermiso { RolId = rolDocente.Id, PermisoId = permisos[1].Id });
            ctx.RolPermisos.Add(new RolPermiso { RolId = rolDocente.Id, PermisoId = permisos[5].Id });

            ctx.RolPermisos.Add(new RolPermiso { RolId = rolEst.Id, PermisoId = permisos[3].Id });

            ctx.SaveChanges();

            // Users
            var admin = new Usuario { Nombre = "Admin", Apellidos = "Sistema", Email = "admin@sistema.edu", Identificacion = "0001" };
            var docente = new Usuario { Nombre = "Maria", Apellidos = "Garcia", Email = "maria.garcia@sistema.edu", Identificacion = "D-100" };
            var estudiante = new Usuario { Nombre = "Juan", Apellidos = "Perez", Email = "juan.perez@sistema.edu", Identificacion = "E-200" };

            // Set password hashes
            PasswordHasher.CreatePasswordHash("Admin123!", out var hashAdmin, out var saltAdmin);
            admin.PasswordHash = hashAdmin; admin.PasswordSalt = saltAdmin;
            PasswordHasher.CreatePasswordHash("123456", out var hashDoc, out var saltDoc);
            docente.PasswordHash = hashDoc; docente.PasswordSalt = saltDoc;
            PasswordHasher.CreatePasswordHash("estudiante1", out var hashEst, out var saltEst);
            estudiante.PasswordHash = hashEst; estudiante.PasswordSalt = saltEst;

            // Set Activo property dynamically (supports bool or int)
            void SetActivo(object userObj)
            {
                var prop = userObj.GetType().GetProperty("Activo");
                if (prop != null)
                {
                    if (prop.PropertyType == typeof(bool)) prop.SetValue(userObj, true);
                    else if (prop.PropertyType == typeof(int)) prop.SetValue(userObj,1);
                }
            }
            SetActivo(admin); SetActivo(docente); SetActivo(estudiante);

            ctx.Usuarios.AddRange(admin, docente, estudiante);
            ctx.SaveChanges();

            // Assign roles
            ctx.UsuarioRoles.Add(new UsuarioRol { UsuarioId = admin.Id, RolId = rolAdmin.Id });
            ctx.UsuarioRoles.Add(new UsuarioRol { UsuarioId = docente.Id, RolId = rolDocente.Id });
            ctx.UsuarioRoles.Add(new UsuarioRol { UsuarioId = estudiante.Id, RolId = rolEst.Id });
            ctx.SaveChanges();

            // Cuatrimestre, Curso, CursoDocente, Matricula
            var cuatri = new Models.Academico.Cuatrimestre { Nombre = "2025-1" };
            ctx.Cuatrimestres.Add(cuatri);
            ctx.SaveChanges();

            var curso = new Models.Academico.Curso { Codigo = "MAT101", Nombre = "Matemáticas I", Creditos =4, CuatrimestreId = cuatri.Id, CreadoPorId = admin.Id };
            ctx.Cursos.Add(curso);
            ctx.SaveChanges();

            // Assign docente to course
            ctx.CursoDocentes.Add(new Models.Academico.CursoDocente { CursoId = curso.Id, DocenteId = docente.Id, Activo = true });
            ctx.SaveChanges();

            // Enroll student
            var matricula = new Models.Academico.Matricula { CursoId = curso.Id, CuatrimestreId = cuatri.Id, EstudianteId = estudiante.Id, FechaMatricula = DateTime.UtcNow };
            ctx.Matriculas.Add(matricula);
            ctx.SaveChanges();

            // Optionally add a sample evaluation
            var eval = new Models.Academico.Evaluacion { MatriculaId = matricula.Id, Nota =85.5m, Estado = "Aprobado", Participacion = "Buena", Observaciones = "Evaluación inicial", FechaRegistro = DateTime.UtcNow, UsuarioRegistro = docente.Id };
            ctx.Evaluaciones.Add(eval);
            ctx.SaveChanges();
        }
    }
}