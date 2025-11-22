// Consulta historial vía API
function consultar() {
    const id = document.getElementById('estudianteId').value;
    fetch('/api/historial/' + id)
    .then(r => r.json())
    .then(data => {
      const tbody = document.querySelector('#tablaHistorial tbody');
  tbody.innerHTML = "";
   data.registros.forEach(h => {
       const tr = document.createElement('tr');
       tr.innerHTML = `
 <td>${h.curso}</td>
     <td>${h.cuatrimestre}</td>
     <td>${h.nota}</td>
     <td>${h.estado}</td>
     <td>${h.participacion}</td>
     <td>${h.observaciones}</td>
     <td>${h.fecha}</td>
 `;
 tbody.appendChild(tr);
  });
    });
}