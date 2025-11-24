-- SQL to populate the database with initial data
-- Run this after creating the database schema (e.g., after running the app once)

-- Insert roles
INSERT INTO Roles (Nombre) VALUES ('Administrador');
INSERT INTO Roles (Nombre) VALUES ('Docente');
INSERT INTO Roles (Nombre) VALUES ('Estudiante');

-- Insert permisos
INSERT INTO Permisos (Codigo, Descripcion) VALUES ('Usuarios.Gestion', 'Gestionar usuarios');
INSERT INTO Permisos (Codigo, Descripcion) VALUES ('Cursos.Ver', 'Ver cursos');
INSERT INTO Permisos (Codigo, Descripcion) VALUES ('Cursos.AsignarDocente', 'Asignar docentes');
INSERT INTO Permisos (Codigo, Descripcion) VALUES ('Historial.Ver', 'Ver historial');
INSERT INTO Permisos (Codigo, Descripcion) VALUES ('Bitacora.Ver', 'Ver bitacora');
INSERT INTO Permisos (Codigo, Descripcion) VALUES ('Evaluaciones.Crear', 'Crear evaluaciones');

-- Insert rol permisos (admin has all, docente some, estudiante some)
INSERT INTO RolPermisos (RolId, PermisoId) VALUES (1,1); -- Admin Usuarios.Gestion
INSERT INTO RolPermisos (RolId, PermisoId) VALUES (1,2); -- Admin Cursos.Ver
INSERT INTO RolPermisos (RolId, PermisoId) VALUES (1,3); -- Admin Cursos.AsignarDocente
INSERT INTO RolPermisos (RolId, PermisoId) VALUES (1,4); -- Admin Historial.Ver
INSERT INTO RolPermisos (RolId, PermisoId) VALUES (1,5); -- Admin Bitacora.Ver
INSERT INTO RolPermisos (RolId, PermisoId) VALUES (1,6); -- Admin Evaluaciones.Crear

INSERT INTO RolPermisos (RolId, PermisoId) VALUES (2,2); -- Docente Cursos.Ver
INSERT INTO RolPermisos (RolId, PermisoId) VALUES (2,6); -- Docente Evaluaciones.Crear

INSERT INTO RolPermisos (RolId, PermisoId) VALUES (3,4); -- Estudiante Historial.Ver

-- Insert users
INSERT INTO Usuarios (Nombre, Apellidos, Email, Identificacion, PasswordHash, PasswordSalt, IsActive) VALUES 
('Admin', 'Sistema', 'admin@sistema.edu', '0001', 0xB9647D9EBF7AD955B61C3D71D0ED18676F9D38293AA591CA258151BBC80A4B7B7BF4729ABFE2171F130D2E0B9C0CDEE6697E5C5E00769F1FD45251DE279E69D3, 0xAA3F7BC7C3E9D51EE1417332678F757F8B604C34260743C746DE63CFD1A1FC5EA0969DAFF2494CABA6359D725EB85D357161E88E1479823BB66D42EE3D32016E18928ABF6A28E4B10BD68F5A970941D4C1561E62E90D1BD68E241A63BA891BDED4247E9A110F2259DF557493040F41F8D2E0F165F36DCF4B581435A101F00CE9, 1);

INSERT INTO Usuarios (Nombre, Apellidos, Email, Identificacion, PasswordHash, PasswordSalt, IsActive) VALUES 
('Maria', 'Garcia', 'maria.garcia@sistema.edu', 'D-100', 0x8DECC79B23C64019B896E28C8921E83B56F067A6E2600378D60C00B1E4C712FE2CAB692CA43AE455E44FEFBD2270C60C6715FD5B729FCC63040390DDBA729D53, 0x1D43A62E4478229943B2FC3EE282E88E63CB03F6B083DFB07B7643FC2A1B662B0E3A55D2148B9E59325BC8FEDE4437859530F8763F9E9D58293B2E849FE42170F8ED408C76DD6EF4AEB3B004D398F2E86105C4587301696BF96B642A0C33D24A268790AC81E622511A8049A68AD407E7433180A47BA23F1E3507C846311F0DE9, 1);

INSERT INTO Usuarios (Nombre, Apellidos, Email, Identificacion, PasswordHash, PasswordSalt, IsActive) VALUES 
('Juan', 'Perez', 'juan.perez@sistema.edu', 'E-200', 0x77BB3067BD3DEF40F9CC30842B44E4451F98FFE5B7C497C72DB03A3FC0B2478DEBC98706F39F0D58B6475C7355F4B3442C220FC827A836A3F6FBBFC97E20159D, 0xAABDC32A8EBE5389B504D6C71CF2628FECEE5A391DF795EB952017A88D785C3AA661A2F9A92B640CEAFCDF35CFED8C6604648B6F487343B51F677E8F432EB310C34D9CE3F7BFDFE5C3D296DC540A802D78FB6F5BAA4AA75155B14274D7DE1697B93B278E6A125C3B699227F2CC06CFC9B5F7CB4A983F4B03766DD6C487CF3597, 1);

-- Assign roles to users
INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES (1,1); -- Admin
INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES (2,2); -- Docente
INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES (3,3); -- Estudiante

-- Insert cuatrimestre
INSERT INTO Cuatrimestres (Nombre) VALUES ('2025-1');

-- Insert curso
INSERT INTO Cursos (Codigo, Nombre, Creditos, CuatrimestreId, FechaCreacion) VALUES ('MAT101', 'Matemáticas I', 4, 1, GETDATE());

-- Assign docente to curso
INSERT INTO CursoDocentes (CursoId, DocenteId, Activo) VALUES (1, 2, 1);

-- Enroll estudiante
INSERT INTO Matriculas (CursoId, CuatrimestreId, EstudianteId, FechaMatricula) VALUES (1, 1, 3, GETDATE());

-- Add evaluation
INSERT INTO Evaluaciones (MatriculaId, Nota, Estado, Participacion, Observaciones, FechaRegistro, UsuarioRegistro) VALUES (1, 85.5, 'Aprobado', 'Buena', 'Evaluación inicial', GETDATE(), 2);