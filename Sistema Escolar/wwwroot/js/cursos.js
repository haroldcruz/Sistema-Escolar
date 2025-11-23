(function(){
 // Determinar si es docente mirando el body data attribute agregado por layout
 const rolesAttr = document.body.getAttribute('data-user-role') || '';
 const isDocente = rolesAttr.split(',').map(s=>s.trim()).includes('Docente');
 const url = isDocente ? `/api/mis-cursos` : '/api/cursos';
 fetch(url)
 .then(r => r.json())
 .then(data => {
 const tbody = document.querySelector('#tablaCursos tbody');
 if(!tbody) return;
 tbody.innerHTML = '';
 data.forEach(c => {
 const tr = document.createElement('tr');
 tr.innerHTML = `
 <td>${escapeHtml(c.codigo)}</td>
 <td>${escapeHtml(c.nombre)}</td>
 <td>${escapeHtml(c.creditos)}</td>
 <td>${escapeHtml(c.cuatrimestre)}</td>
 <td>${escapeHtml((c.docentes||[]).join(", "))}</td>
 `;
 tbody.appendChild(tr);
 });
 }).catch(err=>{ console.error(err); });

 function escapeHtml(s){ return (s||'').toString().replace(/[&<>\"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;','\'':'&#39;'}[c])); }
})();