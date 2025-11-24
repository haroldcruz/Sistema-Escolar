-- Idempotent deployment script for AddCursoOferta migration
-- Creates CursoOfertas and CursoOfertaDocentes tables and adds CursoOfertaId column to Matriculas
-- Also ensures lockout columns exist in Usuarios to avoid login errors
-- Review before running, run in a transaction and backup DB first

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Ensure lockout columns in Usuarios
IF COL_LENGTH('dbo.Usuarios', 'FailedLoginAttempts') IS NULL
BEGIN
 ALTER TABLE dbo.Usuarios ADD FailedLoginAttempts INT NOT NULL CONSTRAINT DF_Usuarios_FailedLoginAttempts DEFAULT(0);
END

IF COL_LENGTH('dbo.Usuarios', 'LockoutEnd') IS NULL
BEGIN
 ALTER TABLE dbo.Usuarios ADD LockoutEnd DATETIME2 NULL;
END

-- Create CursoOfertas if not exists
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CursoOfertas]') AND type = N'U')
BEGIN
 CREATE TABLE [dbo].[CursoOfertas]
 (
 [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
 [CursoId] INT NOT NULL,
 [CuatrimestreId] INT NOT NULL,
 [NombreGrupo] NVARCHAR(50) NULL,
 [Capacidad] INT NULL,
 [FechaCreacion] DATETIME2 NOT NULL
 );

 -- FK to Cursos
 IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CursoOfertas_Cursos_CursoId')
 BEGIN
 ALTER TABLE [dbo].[CursoOfertas]
 ADD CONSTRAINT [FK_CursoOfertas_Cursos_CursoId] FOREIGN KEY([CursoId])
 REFERENCES [dbo].[Cursos]([Id]) ON DELETE CASCADE;
 END

 -- FK to Cuatrimestres
 IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CursoOfertas_Cuatrimestres_CuatrimestreId')
 BEGIN
 ALTER TABLE [dbo].[CursoOfertas]
 ADD CONSTRAINT [FK_CursoOfertas_Cuatrimestres_CuatrimestreId] FOREIGN KEY([CuatrimestreId])
 REFERENCES [dbo].[Cuatrimestres]([Id]) ON DELETE CASCADE;
 END
END

-- Create CursoOfertaDocentes if not exists
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CursoOfertaDocentes]') AND type = N'U')
BEGIN
 CREATE TABLE [dbo].[CursoOfertaDocentes]
 (
 [CursoOfertaId] INT NOT NULL,
 [DocenteId] INT NOT NULL,
 CONSTRAINT PK_CursoOfertaDocentes PRIMARY KEY (CursoOfertaId, DocenteId)
 );

 IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CursoOfertaDocentes_CursoOfertas_CursoOfertaId')
 BEGIN
 ALTER TABLE [dbo].[CursoOfertaDocentes]
 ADD CONSTRAINT [FK_CursoOfertaDocentes_CursoOfertas_CursoOfertaId] FOREIGN KEY([CursoOfertaId])
 REFERENCES [dbo].[CursoOfertas]([Id]) ON DELETE CASCADE;
 END

 IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CursoOfertaDocentes_Usuarios_DocenteId')
 BEGIN
 ALTER TABLE [dbo].[CursoOfertaDocentes]
 ADD CONSTRAINT [FK_CursoOfertaDocentes_Usuarios_DocenteId] FOREIGN KEY([DocenteId])
 REFERENCES [dbo].[Usuarios]([Id]) ON DELETE NO ACTION;
 END
END

-- Add CursoOfertaId column to Matriculas if missing
IF COL_LENGTH('dbo.Matriculas', 'CursoOfertaId') IS NULL
BEGIN
 ALTER TABLE [dbo].[Matriculas] ADD [CursoOfertaId] INT NULL;
END

-- Create indexes if missing
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CursoOfertas_CursoId' AND object_id = OBJECT_ID('dbo.CursoOfertas'))
BEGIN
 CREATE INDEX [IX_CursoOfertas_CursoId] ON [dbo].[CursoOfertas]([CursoId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CursoOfertas_CuatrimestreId' AND object_id = OBJECT_ID('dbo.CursoOfertas'))
BEGIN
 CREATE INDEX [IX_CursoOfertas_CuatrimestreId] ON [dbo].[CursoOfertas]([CuatrimestreId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CursoOfertaDocentes_DocenteId' AND object_id = OBJECT_ID('dbo.CursoOfertaDocentes'))
BEGIN
 CREATE INDEX [IX_CursoOfertaDocentes_DocenteId] ON [dbo].[CursoOfertaDocentes]([DocenteId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Matriculas_CursoOfertaId' AND object_id = OBJECT_ID('dbo.Matriculas'))
BEGIN
 CREATE INDEX [IX_Matriculas_CursoOfertaId] ON [dbo].[Matriculas]([CursoOfertaId]);
END

-- Add foreign key Matriculas.CursoOfertaId -> CursoOfertas.Id if missing
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Matriculas_CursoOfertas_CursoOfertaId')
BEGIN
 ALTER TABLE [dbo].[Matriculas]
 ADD CONSTRAINT [FK_Matriculas_CursoOfertas_CursoOfertaId] FOREIGN KEY([CursoOfertaId])
 REFERENCES [dbo].[CursoOfertas]([Id]) ON DELETE NO ACTION;
END

COMMIT TRANSACTION;

PRINT 'AddCursoOferta script executed.';
