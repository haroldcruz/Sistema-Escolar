Lista de acciones sugeridas para limpiar el repositorio:

- Eliminar la carpeta `obj/` que contiene archivos generados por la build. No deben incluirse en el control de versiones.
- Revisar y eliminar archivos `*.ide.g.cs` generados por Visual Studio dentro de `Views/` si no son necesarios (son archivos temporales de diseño).
- Verificar que la carpeta `wwwroot/` (si existe) no tenga archivos flotantes no usados.
- Ejecutar `git status` y `git clean -fd` localmente para eliminar archivos ignorados/redundantes y luego confirmar cambios.
- Añadir/actualizar `.gitignore` para excluir `bin/`, `obj/`, `*.user`, `.vs/`, `*.ide.g.cs`, y otros archivos generados.
- Validar que `Program.cs` y archivos de configuración no referencien archivos o vistas que serán eliminadas.

IMPORTANTE: No se eliminaron archivos fuente que formen parte del código (Controllers, Models, Views, Migrations) salvo `obj` generados de ejemplo. Revisión manual requerida para confirmar que ninguna vista o recurso está en uso antes de eliminarlos permanentemente.