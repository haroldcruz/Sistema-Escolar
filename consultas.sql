SELECT u.Email, r.Nombre AS Rol
FROM Usuarios u
JOIN UsuarioRoles ur ON ur.UsuarioId = u.Id
JOIN Roles r ON r.Id = ur.RolId
WHERE u.Email = 'admin@sistema.edu';