using SistemaEscolar.Data;
using SistemaEscolar.Models;
using SistemaEscolar.Models.Academico;
using SistemaEscolar.Models.Bitacora;
using SistemaEscolar.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;

namespace SistemaEscolar.Data
{
    public static class DataSeeder
    {
        public static void Seed(ApplicationDbContext ctx)
        {
            // Evitar múltiples ejecuciones simultáneas
            if (ctx.Database.CurrentTransaction != null) return;
            using var tx = ctx.Database.BeginTransaction();
            try
            {
                //1. Roles base (idempotente)
                foreach(var rol in new[]{"Administrador","Docente","Estudiante","Coordinador"}) EnsureRole(ctx, rol);
                ctx.SaveChanges();
                var roles = ctx.Roles.ToDictionary(r=>r.Nombre, r=>r.Id);
                //2. Permisos requeridos (idempotente)
                string[] permisoCodigos = {
                    "Usuarios.Gestion","Cursos.Ver","Cursos.Crear","Cursos.Editar","Cursos.Eliminar","Cursos.AsignarDocente","Historial.Ver","Bitacora.Ver","Seguridad.Gestion"
                };
                foreach(var cod in permisoCodigos) EnsurePermiso(ctx, cod, $"Permiso {cod}");
                ctx.SaveChanges();
                //3. Asignar permisos faltantes a cada rol según matriz
                AsignarPermisosRol(ctx, roles["Administrador"], permisoCodigos); // todos
                AsignarPermisosRol(ctx, roles["Coordinador"], new[]{"Cursos.Ver","Cursos.Crear","Cursos.Editar","Cursos.AsignarDocente","Historial.Ver"});
                AsignarPermisosRol(ctx, roles["Docente"], new[]{"Cursos.Ver","Historial.Ver"});
                AsignarPermisosRol(ctx, roles["Estudiante"], new[]{"Historial.Ver"});
                ctx.SaveChanges();
                //4. Usuarios esenciales (crear si faltan)
                var adminUser = EnsureUsuario(ctx, "Admin", "Sistema", "admin@sistema.edu", "123456789", "Admin123!");
                var docente1 = EnsureUsuario(ctx, "Juan", "Pérez", "juan.perez@sistema.edu", "111111111", "Docente123!");
                var docente2 = EnsureUsuario(ctx, "María", "García", "maria.garcia@sistema.edu", "222222222", "Docente123!");
                var estudiante1 = EnsureUsuario(ctx, "Carlos", "López", "carlos.lopez@sistema.edu", "333333333", "Estudiante123!");
                var estudiante2 = EnsureUsuario(ctx, "Ana", "Martínez", "ana.martinez@sistema.edu", "444444444", "Estudiante123!");
                var coordinador = EnsureUsuario(ctx, "Luis", "Rodríguez", "luis.rodriguez@sistema.edu", "555555555", "Coordinador123!");
                ctx.SaveChanges();
                //5. Asignar roles a usuarios (solo si falta relación)
                EnsureUsuarioRol(ctx, adminUser.Id, roles["Administrador"]);
                EnsureUsuarioRol(ctx, docente1.Id, roles["Docente"]);
                EnsureUsuarioRol(ctx, docente2.Id, roles["Docente"]);
                EnsureUsuarioRol(ctx, estudiante1.Id, roles["Estudiante"]);
                EnsureUsuarioRol(ctx, estudiante2.Id, roles["Estudiante"]);
                EnsureUsuarioRol(ctx, coordinador.Id, roles["Coordinador"]);
                ctx.SaveChanges();
                //6. Datos académicos demo (solo si no existen cursos)
                if(!ctx.Cursos.Any())
                {
                    var cuatrimestre1 = new Cuatrimestre { Nombre = "2024-1", Anio =2024 };
                    var cuatrimestre2 = new Cuatrimestre { Nombre = "2024-2", Anio =2024 };
                    var cuatrimestre3 = new Cuatrimestre { Nombre = "2025-1", Anio =2025 };
                    ctx.Cuatrimestres.AddRange(cuatrimestre1, cuatrimestre2, cuatrimestre3); ctx.SaveChanges();
                    var c1 = CrearCurso("MAT101","Matemáticas Básicas",3, cuatrimestre1.Id, adminUser.Id);
                    var c2 = CrearCurso("FIS101","Física General",4, cuatrimestre1.Id, adminUser.Id);
                    var c3 = CrearCurso("QUI101","Química Orgánica",3, cuatrimestre2.Id, adminUser.Id);
                    var c4 = CrearCurso("PRO101","Programación I",4, cuatrimestre2.Id, adminUser.Id);
                    ctx.Cursos.AddRange(c1,c2,c3,c4); ctx.SaveChanges();
                    ctx.CursoDocentes.AddRange(
                        new CursoDocente{ CursoId=c1.Id, DocenteId=docente1.Id, Activo=true},
                        new CursoDocente{ CursoId=c2.Id, DocenteId=docente1.Id, Activo=true},
                        new CursoDocente{ CursoId=c3.Id, DocenteId=docente2.Id, Activo=true},
                        new CursoDocente{ CursoId=c4.Id, DocenteId=docente2.Id, Activo=true}
                    ); ctx.SaveChanges();
                    var m1 = new Matricula{ EstudianteId = estudiante1.Id, CursoId = c1.Id, CuatrimestreId = cuatrimestre1.Id, FechaMatricula = DateTime.UtcNow.AddDays(-30)};
                    var m2 = new Matricula{ EstudianteId = estudiante1.Id, CursoId = c2.Id, CuatrimestreId = cuatrimestre1.Id, FechaMatricula = DateTime.UtcNow.AddDays(-30)};
                    var m3 = new Matricula{ EstudianteId = estudiante2.Id, CursoId = c3.Id, CuatrimestreId = cuatrimestre2.Id, FechaMatricula = DateTime.UtcNow.AddDays(-15)};
                    var m4 = new Matricula{ EstudianteId = estudiante2.Id, CursoId = c4.Id, CuatrimestreId = cuatrimestre2.Id, FechaMatricula = DateTime.UtcNow.AddDays(-15)};
                    ctx.Matriculas.AddRange(m1,m2,m3,m4); ctx.SaveChanges();
                    ctx.Evaluaciones.AddRange(
                        CrearEval(m1, docente1.Id,85.50m, "Buen desempeño", "Activa", "Aprobado"),
                        CrearEval(m2, docente1.Id,92.00m, "Excelente trabajo", "Muy activa", "Aprobado"),
                        CrearEval(m3, docente2.Id,78.25m, "Necesita mejorar en prácticas", "Regular", "Aprobado"),
                        CrearEval(m4, docente2.Id,65.00m, "Debe esforzarse más", "Baja", "Reprobado")
                    ); ctx.SaveChanges();
                }
                // Bitácora inicial mínima
                if(!ctx.BitacoraEntries.Any())
                {
                    ctx.BitacoraEntries.AddRange(
                        new BitacoraEntry{ UsuarioId = adminUser.Id, Accion="Seed: Creación admin", Modulo="Usuarios", Ip="127.0.0.1", Fecha = DateTime.UtcNow },
                        new BitacoraEntry{ UsuarioId = docente1.Id, Accion="Seed: Evaluaciones iniciales", Modulo="Evaluaciones", Ip="127.0.0.1", Fecha = DateTime.UtcNow }
                    ); ctx.SaveChanges();
                }
                tx.Commit();
            }
            catch(Exception ex)
            {
                // En caso de error revertir cambios parciales
                tx.Rollback();
                Console.WriteLine($"[DataSeeder] Error: {ex.Message}");
            }
        }

        private static void EnsureRole(ApplicationDbContext ctx, string nombre){ if(!ctx.Roles.Any(r=>r.Nombre==nombre)) ctx.Roles.Add(new Rol{ Nombre=nombre}); }
        private static void EnsurePermiso(ApplicationDbContext ctx, string codigo, string desc){ if(!ctx.Permisos.Any(p=>p.Codigo==codigo)) ctx.Permisos.Add(new Permiso{ Codigo=codigo, Descripcion=desc}); }
        private static void AsignarPermisosRol(ApplicationDbContext ctx, int rolId, IEnumerable<string> codigos){ var permisoIds = ctx.Permisos.Where(p=> codigos.Contains(p.Codigo)).Select(p=>p.Id).ToList(); foreach(var pid in permisoIds) if(!ctx.RolPermisos.Any(rp=>rp.RolId==rolId && rp.PermisoId==pid)) ctx.RolPermisos.Add(new RolPermiso{ RolId=rolId, PermisoId=pid}); }
        private static Usuario EnsureUsuario(ApplicationDbContext ctx, string nombre,string apellidos,string email,string identificacion,string password){ var u = ctx.Usuarios.FirstOrDefault(x=>x.Email==email); if(u!=null) return u; PasswordHasher.CreatePasswordHash(password,out var hash,out var salt); u = new Usuario{ Nombre=nombre, Apellidos=apellidos, Email=email, Identificacion=identificacion, Activo=true, FechaCreacion=DateTime.UtcNow, PasswordHash=hash, PasswordSalt=salt }; ctx.Usuarios.Add(u); return u; }
        private static void EnsureUsuarioRol(ApplicationDbContext ctx, int usuarioId, int rolId){ if(!ctx.UsuarioRoles.Any(ur=>ur.UsuarioId==usuarioId && ur.RolId==rolId)) ctx.UsuarioRoles.Add(new UsuarioRol{ UsuarioId=usuarioId, RolId=rolId}); }
        private static Curso CrearCurso(string codigo,string nombre,int creditos,int cuatrimestreId,int usuarioCreacion)
            => new Curso{ Codigo=codigo, Nombre=nombre, Descripcion=$"Curso de {nombre}", Creditos=creditos, CuatrimestreId=cuatrimestreId, FechaCreacion=DateTime.UtcNow, UsuarioCreacion=usuarioCreacion };
        private static Evaluacion CrearEval(Matricula m,int usuarioRegistro,decimal nota,string obs,string participacion,string estado)
            => new Evaluacion{ MatriculaId=m.Id, Matricula=m, UsuarioRegistro=usuarioRegistro, Nota=nota, Observaciones=obs, Participacion=participacion, Estado=estado, FechaRegistro=DateTime.UtcNow };
    }
}