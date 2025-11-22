// Carga de usuarios via API
fetch('/api/usuarios')
    .then(r => r.json())
    .then(data => {
   const tbody = document.querySelector('#tablaUsuarios tbody');
    data.forEach(u => {
   const tr = document.createElement('tr');
  tr.innerHTML = `
      <td>${u.id}</td>
       <td>${u.nombreCompleto}</td>
       <td>${u.email}</td>
 <td>${u.identificacion}</td>
      <td>${u.roles.join(", ")}</td>
        `;
        tbody.appendChild(tr);
   });
});