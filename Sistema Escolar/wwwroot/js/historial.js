// Consulta historial vía API
(function(){
 document.addEventListener('DOMContentLoaded', function(){
 const input = document.getElementById('buscarEstudiante');
 const results = document.getElementById('resultadosEstudiantes');
 const formEval = document.getElementById('formEvaluacion');
 const formMat = document.getElementById('formMatricular');
 const selectCurso = formMat ? formMat.querySelector('select[name="CursoId"]') : null;
 const selectCuatr = formMat ? formMat.querySelector('select[name="CuatrimestreId"]') : null;
 const matDetalle = document.getElementById('matriculaDetalle');
 if(!input || !results) return;
 let timer =0;
 input.addEventListener('input', function(){
 clearTimeout(timer);
 const term = this.value.trim();
 if(!term){ results.innerHTML=''; return; }
 timer = setTimeout(()=> buscar(term),300);
 });

 async function buscar(term){
 try{
 const r = await fetch(`/api/historial/buscar?term=${encodeURIComponent(term)}`, { credentials: 'same-origin' });
 if(!r.ok){ results.innerHTML = '<div class="text-danger">Error búsqueda</div>'; return; }
 const data = await r.json();
 render(data);
 }catch(e){ results.innerHTML = '<div class="text-danger">Error red</div>'; }
 }

 function render(items){
 if(!items || !items.length){ results.innerHTML = '<div class="text-muted">Sin resultados</div>'; return; }
 results.innerHTML = items.map(it=>`<div class="card mb-1 p-2 estudiante-item" data-id="${it.id}"><strong>${escapeHtml(it.nombreCompleto)}</strong><div class="small text-muted">${escapeHtml(it.identificacion)} | ${escapeHtml(it.email)}</div></div>`).join('');
 document.querySelectorAll('.estudiante-item').forEach(el=> el.addEventListener('click', ev=> select(parseInt(el.getAttribute('data-id')))));
 }

 async function select(id){
 // Cargar historial o datos básicos
 try{
 const r = await fetch(`/api/historial/${id}`, { credentials: 'same-origin' });
 if(!r.ok) return;
 const data = await r.json();
 // mostrar resumen sencillo en la UI
 const detalle = document.getElementById('estudianteDetalle');
 if(detalle){ detalle.innerHTML = `<h5>${escapeHtml(data.nombreCompleto)}</h5><p>Registros: ${data.registros?.length ??0}</p>`; }
 // preparar formEvaluacion con primera matrícula disponible si aplica
 if(formEval){ 
 const mid = formEval.querySelector('input[name="MatriculaId"]');
 if(mid){
 const firstMatId = (data.matriculas && data.matriculas.length) ? data.matriculas[0].id : '';
 mid.value = firstMatId || '';
 }
 formEval.classList.remove('d-none');
 }
 // preparar formulario de matricula: poblar selects de cursos y cuatrimestres
 if(formMat){
 formMat.querySelector('input[name="EstudianteId"]').value = id;
 formMat.classList.remove('d-none');
 await poblarCatalogos();
 // mostrar matrículas actuales
 if(data.matriculas && data.matriculas.length){
 matDetalle.innerHTML = '<ul class="mb-0">' + data.matriculas.map(m=>`<li>${escapeHtml(m.curso)} — ${escapeHtml(m.cuatrimestre)}</li>`).join('') + '</ul>';
 } else { matDetalle.innerHTML = 'Sin matrículas actualmente'; }
 }
 }catch(e){ console.error(e); }
 }

 async function poblarCatalogos(){
 try{
 // traer catalogos (cuatrimestres y cursos)
 const [cursosR, cuatR] = await Promise.all([
 fetch('/api/cursos', { credentials: 'same-origin' }),
 fetch('/api/catalogo/cuatrimestres', { credentials: 'same-origin' })
 ]);
 if(cursosR.ok){ const cursos = await cursosR.json(); if(selectCurso){ selectCurso.innerHTML = cursos.map(c=>`<option value="${c.id}">${escapeHtml(c.nombre)}</option>`).join(''); } }
 if(cuatR.ok){ const cuats = await cuatR.json(); if(selectCuatr){ selectCuatr.innerHTML = cuats.map(c=>`<option value="${c.id}">${escapeHtml(c.nombre)}</option>`).join(''); } }
 }catch(e){ console.error(e); }
 }

 // enviar evaluación via AJAX
 if(formEval){ formEval.addEventListener('submit', async function(ev){ ev.preventDefault(); const btn = this.querySelector('button[type=submit]'); btn.disabled=true; try{ const tokenEl = this.querySelector('input[name="__RequestVerificationToken"]'); const antiforgery = tokenEl? tokenEl.value : null; const dto = { MatriculaId: parseInt(this.querySelector('input[name=MatriculaId]').value||'0'), Nota: parseFloat(this.querySelector('input[name=Nota]').value||'0'), Observaciones: this.querySelector('textarea[name=Observaciones]').value||'', Participacion: this.querySelector('input[name=Participacion]').value||'', Estado: this.querySelector('select[name=Estado]').value||'' }; const headers = { 'Content-Type':'application/json' }; if(antiforgery) headers['RequestVerificationToken'] = antiforgery; const r = await fetch('/api/evaluaciones',{ method:'POST', headers, body: JSON.stringify(dto), credentials:'same-origin' }); const json = await r.json().catch(()=>({message: r.statusText})); if(r.ok){ showAlert('success', json.message || 'Evaluación registrada'); } else { showAlert('danger', json.message || 'Error al registrar'); } } catch(e){ showAlert('danger', e.message||'Error red'); } finally{ btn.disabled=false; } }); }

 // enviar matricula via AJAX
 if(formMat){ formMat.addEventListener('submit', async function(ev){ ev.preventDefault(); const btn = this.querySelector('button[type=submit]'); btn.disabled=true; try{ const tokenEl = this.querySelector('input[name="__RequestVerificationToken"]'); const antiforgery = tokenEl? tokenEl.value : null; const dto = { EstudianteId: parseInt(this.querySelector('input[name=EstudianteId]').value||'0'), CursoId: parseInt(this.querySelector('select[name=CursoId]').value||'0'), CuatrimestreId: parseInt(this.querySelector('select[name=CuatrimestreId]').value||'0') }; const headers = { 'Content-Type':'application/json' }; if(antiforgery) headers['RequestVerificationToken'] = antiforgery; const r = await fetch('/api/matriculas',{ method:'POST', headers, body: JSON.stringify(dto), credentials:'same-origin' }); const json = await r.json().catch(()=>({message: r.statusText})); if(r.ok){ showAlert('success', json.message || 'Matriculado correctamente'); // actualizar UI con la matrícula nueva
 if(json.matricula){ const li = document.createElement('li'); li.textContent = `${json.matricula.curso} — ${json.matricula.cuatrimestre}`; if(matDetalle && matDetalle.querySelector('ul')) matDetalle.querySelector('ul').appendChild(li); else if(matDetalle) matDetalle.innerHTML = '<ul class="mb-0"></ul>'; if(matDetalle.querySelector('ul')) matDetalle.querySelector('ul').appendChild(li); }
 } else { showAlert('danger', json.message || 'Error al matricular'); } } catch(e){ showAlert('danger', e.message||'Error red'); } finally{ btn.disabled=false; } }); }

 function showAlert(type,msg){ const a = document.createElement('div'); a.className = `alert alert-${type} mt-2`; a.textContent = msg; document.querySelector('main.container').prepend(a); setTimeout(()=> a.remove(),5000); }

 function escapeHtml(s){ return (s||'').toString().replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;','\' : '&#39;'}[c])); }

 });
})();