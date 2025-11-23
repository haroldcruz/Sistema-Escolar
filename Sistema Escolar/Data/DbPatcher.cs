using Microsoft.EntityFrameworkCore;

namespace SistemaEscolar.Data
{
 public static class DbPatcher
 {
 public static void Apply(ApplicationDbContext ctx)
 {
 // Parche idempotente para ajustar esquema existente de dbo.Cursos
 var sql = @"
/* Asegurar columnas de auditoría */
IF COL_LENGTH('dbo.Cursos','CreadoPorId') IS NULL
 ALTER TABLE [dbo].[Cursos] ADD [CreadoPorId] INT NULL;
IF COL_LENGTH('dbo.Cursos','FechaCreacion') IS NULL
 ALTER TABLE [dbo].[Cursos] ADD [FechaCreacion] DATETIME2 NOT NULL CONSTRAINT DF_Cursos_FechaCreacion DEFAULT (SYSUTCDATETIME());

/* Normalizar tipo y longitud de Codigo para poder indexar */
DECLARE @data_type sysname, @maxlen int;
SELECT @data_type = DATA_TYPE, @maxlen = CHARACTER_MAXIMUM_LENGTH
 FROM INFORMATION_SCHEMA.COLUMNS
 WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Cursos' AND COLUMN_NAME = 'Codigo';

IF @data_type IS NOT NULL
BEGIN
 -- Si es text/ntext o un tipo no (n)varchar, migrar a NVARCHAR(50) usando columna temporal
 IF @data_type IN ('text','ntext') OR (@data_type NOT IN ('varchar','nvarchar'))
 BEGIN
 IF COL_LENGTH('dbo.Cursos','Codigo_tmp') IS NULL
 ALTER TABLE dbo.Cursos ADD Codigo_tmp NVARCHAR(50) NULL;

 -- Usar SQL dinámico para evitar error de compilación por referencias a columna recién creada en el mismo batch
 EXEC('UPDATE dbo.Cursos SET Codigo_tmp = LEFT(CONVERT(NVARCHAR(50), Codigo),50)');
 EXEC('ALTER TABLE dbo.Cursos DROP COLUMN Codigo');
 EXEC('EXEC sp_rename ''dbo.Cursos.Codigo_tmp'',''Codigo'',''COLUMN''');
 END
 ELSE IF @maxlen = -1 OR @maxlen >50
 BEGIN
 ALTER TABLE dbo.Cursos ALTER COLUMN Codigo NVARCHAR(50) NULL;
 END
END

/* Sincronizar columna legada UsuarioCreacion con nueva CreadoPorId */
IF COL_LENGTH('dbo.Cursos','UsuarioCreacion') IS NOT NULL
BEGIN
 -- Copiar datos legados si CreadoPorId está NULL
 UPDATE C SET CreadoPorId = ISNULL(CreadoPorId, UsuarioCreacion) FROM dbo.Cursos AS C;
 -- Hacer nullable para no bloquear inserts nuevos
 BEGIN TRY
 ALTER TABLE dbo.Cursos ALTER COLUMN UsuarioCreacion INT NULL;
 END TRY BEGIN CATCH END CATCH;
END

/* Asegurar NOT NULL en Codigo para consistencia con el modelo */
UPDATE dbo.Cursos SET Codigo = '' WHERE Codigo IS NULL;
BEGIN TRY
 ALTER TABLE dbo.Cursos ALTER COLUMN Codigo NVARCHAR(50) NOT NULL;
END TRY BEGIN CATCH END CATCH;

/* Crear índice único si no existe */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cursos_Codigo' AND object_id = OBJECT_ID('dbo.Cursos'))
 CREATE UNIQUE INDEX [IX_Cursos_Codigo] ON [dbo].[Cursos]([Codigo]);

/* Agregar FK CreadoPorId si no existe */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Cursos_Usuarios_CreadoPorId') AND COL_LENGTH('dbo.Cursos','CreadoPorId') IS NOT NULL
 ALTER TABLE [dbo].[Cursos] WITH CHECK ADD CONSTRAINT [FK_Cursos_Usuarios_CreadoPorId] FOREIGN KEY([CreadoPorId]) REFERENCES [dbo].[Usuarios] ([Id]);
";
 ctx.Database.ExecuteSqlRaw(sql);
 }
 }
}
