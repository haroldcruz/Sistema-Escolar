// Carga de cursos via API
fetch('/api/cursos')
    .then(r => r.json())
    .then(data => {
    const tbody = document.querySelector('#tablaCursos tbody');
        data.forEach(c => {
      const tr = document.createElement('tr');
   tr.innerHTML = `
 <td>${c.codigo}</td>
      <td>${c.nombre}</td>
 <td>${c.creditos}</td>
       <td>${c.cuatrimestre}</td>
 <td>${c.docentes.join(", ")}</td>
       `;
 tbody.appendChild(tr);
  });
    });