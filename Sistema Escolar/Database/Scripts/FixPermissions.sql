-- Fix required permissions and role-permissions (idempotent)
SET NOCOUNT ON;

-- Asegurar roles base (por si no existen)
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Administrador') INSERT INTO dbo.Roles (Nombre) VALUES ('Administrador');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Docente') INSERT INTO dbo.Roles (Nombre) VALUES ('Docente');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Estudiante') INSERT INTO dbo.Roles (Nombre) VALUES ('Estudiante');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Coordinador') INSERT INTO dbo.Roles (Nombre) VALUES ('Coordinador');

-- Permisos base
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Usuarios.Gestion') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Usuarios.Gestion','CRUD usuarios');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Cursos.Ver') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Cursos.Ver','Listar cursos');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Cursos.Crear') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Cursos.Crear','Crear cursos');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Cursos.Editar') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Cursos.Editar','Editar cursos');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Cursos.Eliminar') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Cursos.Eliminar','Eliminar cursos');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Cursos.AsignarDocente') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Cursos.AsignarDocente','Asignar docentes');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Historial.Ver') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Historial.Ver','Ver historial académico');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Bitacora.Ver') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Bitacora.Ver','Ver bitácora');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'Seguridad.Gestion') INSERT INTO dbo.Permisos (Codigo,Descripcion) VALUES ('Seguridad.Gestion','Administrar roles y permisos');

DECLARE @AdminId INT = (SELECT TOP(1) Id FROM dbo.Roles WHERE Nombre='Administrador');
DECLARE @CoordId INT = (SELECT TOP(1) Id FROM dbo.Roles WHERE Nombre='Coordinador');
DECLARE @DocId INT = (SELECT TOP(1) Id FROM dbo.Roles WHERE Nombre='Docente');
DECLARE @EstId INT = (SELECT TOP(1) Id FROM dbo.Roles WHERE Nombre='Estudiante');

-- Asignar todos los permisos al rol Administrador
IF @AdminId IS NOT NULL
BEGIN
 INSERT INTO dbo.RolPermisos (RolId, PermisoId)
 SELECT @AdminId, p.Id
 FROM dbo.Permisos p
 WHERE NOT EXISTS (
 SELECT 1 FROM dbo.RolPermisos rp WHERE rp.RolId = @AdminId AND rp.PermisoId = p.Id
 );
END;

-- Coordinador
IF @CoordId IS NOT NULL
BEGIN
 INSERT INTO dbo.RolPermisos (RolId, PermisoId)
 SELECT @CoordId, p.Id
 FROM dbo.Permisos p
 WHERE p.Codigo IN ('Cursos.Ver','Cursos.Crear','Cursos.Editar','Cursos.AsignarDocente','Historial.Ver')
 AND NOT EXISTS (
 SELECT 1 FROM dbo.RolPermisos rp WHERE rp.RolId = @CoordId AND rp.PermisoId = p.Id
 );
END;

-- Docente
IF @DocId IS NOT NULL
BEGIN
 INSERT INTO dbo.RolPermisos (RolId, PermisoId)
 SELECT @DocId, p.Id
 FROM dbo.Permisos p
 WHERE p.Codigo IN ('Cursos.Ver','Historial.Ver')
 AND NOT EXISTS (
 SELECT 1 FROM dbo.RolPermisos rp WHERE rp.RolId = @DocId AND rp.PermisoId = p.Id
 );
END;

-- Estudiante
IF @EstId IS NOT NULL
BEGIN
 INSERT INTO dbo.RolPermisos (RolId, PermisoId)
 SELECT @EstId, p.Id
 FROM dbo.Permisos p
 WHERE p.Codigo IN ('Historial.Ver')
 AND NOT EXISTS (
 SELECT 1 FROM dbo.RolPermisos rp WHERE rp.RolId = @EstId AND rp.PermisoId = p.Id
 );
END;
