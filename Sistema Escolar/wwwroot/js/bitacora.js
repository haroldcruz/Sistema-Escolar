// Carga bitácora completa
fetch('/api/bitacora')
    .then(r => r.json())
    .then(data => {
   const tbody = document.querySelector('#tablaBitacora tbody');
    data.forEach(b => {
        const tr = document.createElement('tr');
      tr.innerHTML = `
      <td>${b.usuario}</td>
 <td>${b.accion}</td>
      <td>${b.modulo}</td>
 <td>${b.ip}</td>
 <td>${b.fecha}</td>
        `;
   tbody.appendChild(tr);
   });
    });