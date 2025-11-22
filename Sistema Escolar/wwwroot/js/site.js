// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Verifica token antes de cargar vistas protegidas
(function() {
    const protectedPaths = [
        "/Usuarios",
        "/Cursos",
        "/Historial",
        "/Bitacora"
    ];
    const current = window.location.pathname;
    if (protectedPaths.includes(current)) {
        const token = localStorage.getItem('token');
        if (!token) {
            window.location.href = "/";
        }
    }
})();
