-- Ejecutar UNA SOLA VEZ en la base de datos usada por la aplicación (ej: SistemaAcademicoDB)
-- Crea tablas BloqueEvaluaciones y CalificacionBloques si no existen
BEGIN TRANSACTION;

-- Crear BloqueEvaluaciones
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BloqueEvaluaciones]') AND type in (N'U'))
BEGIN
 CREATE TABLE [dbo].[BloqueEvaluaciones]
 (
 [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
 [CursoId] INT NOT NULL,
 [CuatrimestreId] INT NOT NULL,
 [Nombre] NVARCHAR(200) NOT NULL,
 [Tipo] NVARCHAR(100) NULL,
 [Peso] DECIMAL(18,2) NULL,
 [FechaCreacion] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
 [CreadoPorId] INT NULL
 );

 ALTER TABLE [dbo].[BloqueEvaluaciones]
 ADD CONSTRAINT [FK_BloqueEvaluaciones_Cursos_CursoId] FOREIGN KEY ([CursoId]) REFERENCES [dbo].[Cursos]([Id]) ON DELETE CASCADE;

 ALTER TABLE [dbo].[BloqueEvaluaciones]
 ADD CONSTRAINT [FK_BloqueEvaluaciones_Cuatrimestres_CuatrimestreId] FOREIGN KEY ([CuatrimestreId]) REFERENCES [dbo].[Cuatrimestres]([Id]) ON DELETE CASCADE;
END

-- Crear CalificacionBloques
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CalificacionBloques]') AND type in (N'U'))
BEGIN
 CREATE TABLE [dbo].[CalificacionBloques]
 (
 [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
 [BloqueEvaluacionId] INT NOT NULL,
 [MatriculaId] INT NOT NULL,
 [Nota] DECIMAL(5,2) NOT NULL,
 [Observaciones] NVARCHAR(2000) NOT NULL DEFAULT(''),
 [Estado] NVARCHAR(50) NOT NULL DEFAULT(''),
 [FechaRegistro] DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
 [UsuarioRegistro] INT NOT NULL
 );

 ALTER TABLE [dbo].[CalificacionBloques]
 ADD CONSTRAINT [FK_CalificacionBloques_BloqueEvaluaciones_BloqueEvaluacionId] FOREIGN KEY ([BloqueEvaluacionId]) REFERENCES [dbo].[BloqueEvaluaciones]([Id]) ON DELETE CASCADE;

 ALTER TABLE [dbo].[CalificacionBloques]
 ADD CONSTRAINT [FK_CalificacionBloques_Matriculas_MatriculaId] FOREIGN KEY ([MatriculaId]) REFERENCES [dbo].[Matriculas]([Id]) ON DELETE NO ACTION;

 CREATE INDEX IX_CalificacionBloques_Bloque ON [dbo].[CalificacionBloques]([BloqueEvaluacionId]);
 CREATE INDEX IX_CalificacionBloques_Matricula ON [dbo].[CalificacionBloques]([MatriculaId]);
END

COMMIT;

-- NOTA:
--1) Ejecutar este script UNA sola vez (por ejemplo desde SSMS o sqlcmd) contra la base que usa la app.
--2) Si en el futuro trabajas con EF Migrations, recuerda que EF no conocerá estas tablas a menos que crees una migración
-- consistente o insertes manualmente una fila en __EFMigrationsHistory para mantener sincronía.
--3) Si necesitas que genere la migración EF (recomendado), puedo darte los pasos para instalar la versión correcta de dotnet-ef
-- y crear la migración; actualmente en tu entorno la herramienta dotnet-ef instaló una versión incompatible.
