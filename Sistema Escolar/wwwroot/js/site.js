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
 // No bloquear si usamos cookies
 }
 }
})();

// Lógica específica para Bitácora (UX mejorada)
(function(){
 document.addEventListener('DOMContentLoaded', function(){
 if (window.location.pathname !== "/Bitacora") return;

 const tbody = document.querySelector('#tabla tbody');
 const pag = document.getElementById('paginacion');
 const txtUsuario = document.getElementById('usuario');
 const txtModulo = document.getElementById('modulo');
 const txtAccion = document.getElementById('accion');
 const txtDesde = document.getElementById('desde');
 const txtHasta = document.getElementById('hasta');
 const btnBuscar = document.querySelector('button[onclick^="buscar"]');

 let abortController = null;
 let loading = false;
 let lastDurationMs =0;

 let state = {
 page:1,
 pageSize:20,
 sort: 'fecha',
 dir: 'desc',
 data: [],
 hasNext:false
 };

 function qs(){
 const q = new URLSearchParams();
 q.set('page', state.page);
 q.set('pageSize', state.pageSize);
 if (txtUsuario && txtUsuario.value) q.set('usuario', txtUsuario.value);
 if (txtModulo && txtModulo.value) q.set('modulo', txtModulo.value);
 if (txtAccion && txtAccion.value) q.set('accion', txtAccion.value);
 if (txtDesde && txtDesde.value) q.set('desde', txtDesde.value);
 if (txtHasta && txtHasta.value) q.set('hasta', txtHasta.value);
 q.set('sort', state.sort);
 q.set('dir', state.dir);
 return q.toString();
 }

 function setLoading(on){
 loading = on;
 if (btnBuscar){
 btnBuscar.disabled = on;
 btnBuscar.innerHTML = on ? '<span class="spinner-border spinner-border-sm me-1"></span>Buscando...' : 'Buscar';
 }
 if (on){
 tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Cargando datos de bitácora...</td></tr>`;
 } else if (!state.data.length){
 tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Sin resultados</td></tr>`;
 }
 }

 async function cargar(){
 if (loading) return; // evitar solicitudes simultáneas
 setLoading(true);
 // Abort previo
 if (abortController){ abortController.abort(); }
 abortController = new AbortController();
 const signal = abortController.signal;
 const inicio = performance.now();
 try {
 const url = `/api/bitacora/paged?${qs()}`;
 const r = await fetch(url, { credentials: 'include', signal });
 if (!r.ok){
 tbody.innerHTML = `<tr><td colspan="5" class="text-danger">Error ${r.status}</td></tr>`;
 return;
 }
 const json = await r.json();
 state.data = Array.isArray(json) ? json : (json.data ?? []);
 state.hasNext = state.data.length === state.pageSize;
 lastDurationMs = performance.now() - inicio;
 render();
 } catch(e){
 if (e.name === 'AbortError'){ tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Solicitud cancelada...</td></tr>`; }
 else { tbody.innerHTML = `<tr><td colspan="5" class="text-danger">${e.message}</td></tr>`; }
 } finally {
 setLoading(false);
 mostrarResumen();
 }
 }

 function mostrarResumen(){
 // Mostrar tiempo y filtros aplicados bajo la tabla
 let resumen = document.getElementById('bitacoraResumen');
 if (!resumen){
 resumen = document.createElement('div');
 resumen.id = 'bitacoraResumen';
 resumen.className = 'small text-muted mt-2';
 tbody.parentElement.parentElement.appendChild(resumen); // debajo nav
 }
 const filtros = [];
 if (txtUsuario.value) filtros.push(`Usuario: ${escapeHtml(txtUsuario.value)}`);
 if (txtModulo.value) filtros.push(`Módulo: ${escapeHtml(txtModulo.value)}`);
 if (txtAccion.value) filtros.push(`Acción: ${escapeHtml(txtAccion.value)}`);
 if (txtDesde.value) filtros.push(`Desde: ${txtDesde.value}`);
 if (txtHasta.value) filtros.push(`Hasta: ${txtHasta.value}`);
 const filtrosStr = filtros.length ? filtros.join(' | ') : 'Sin filtros';
 resumen.innerHTML = `Resultados: ${state.data.length} | Tiempo: ${Math.round(lastDurationMs)} ms | ${filtrosStr}`;
 }

 function render(){
 let rows = [...state.data];
 const s = state.sort, d = state.dir;
 rows.sort((a,b)=>{
 const va = (a?.[s] ?? '').toString().toLowerCase();
 const vb = (b?.[s] ?? '').toString().toLowerCase();
 if (va < vb) return d==='asc'? -1:1;
 if (va > vb) return d==='asc'?1: -1;
 return0;
 });
 if (!rows.length){
 tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Sin resultados</td></tr>`;
 } else {
 tbody.innerHTML = rows.map(it => `
 <tr>
 <td>${escapeHtml(it.usuario ?? '')}</td>
 <td>${escapeHtml(it.accion ?? '')}</td>
 <td>${escapeHtml(it.modulo ?? '')}</td>
 <td>${escapeHtml(it.ip ?? '')}</td>
 <td>${escapeHtml(it.fecha ?? '')}</td>
 </tr>`).join('');
 }
 renderPag();
 }

 function renderPag(){
 const paginaActual = state.page;
 const hasNext = state.hasNext;
 pag.innerHTML = `
 <li class="page-item ${paginaActual===1? 'disabled': ''}"><a class="page-link" href="#" data-nav="prev">«</a></li>
 <li class="page-item active"><span class="page-link">${paginaActual}</span></li>
 <li class="page-item ${hasNext? '': 'disabled'}"><a class="page-link" href="#" data-nav="next">»</a></li>`;
 pag.querySelectorAll('a[data-nav]').forEach(a => a.addEventListener('click', ev => {
 ev.preventDefault();
 if (loading) return;
 const nav = ev.currentTarget.getAttribute('data-nav');
 if (nav === 'prev' && state.page>1){ state.page--; cargar(); }
 if (nav === 'next' && state.hasNext){ state.page++; cargar(); }
 }));
 }

 function escapeHtml(s){
 return (s||'').toString().replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;','\'':'&#39;'}[c]));
 }

 window.ordenar = function(campo){
 if (state.sort === campo){ state.dir = (state.dir === 'asc') ? 'desc' : 'asc'; }
 else { state.sort = campo; state.dir = 'asc'; }
 render();
 mostrarResumen();
 };
 window.buscar = function(page){
 state.page = page ||1;
 cargar();
 };

 cargar();
 });
})();

// Site-wide JS helpers
(function(){
 function showToast(message, type){
 const toastEl = document.getElementById('appToast');
 if(!toastEl) return alert(message);
 // set body
 const body = toastEl.querySelector('.toast-body');
 body.textContent = message;
 // adjust classes
 toastEl.className = toastEl.className.replace(/text-bg-(\w+)/g, '');
 toastEl.classList.add('text-bg-' + (type || 'info'));
 const toast = new bootstrap.Toast(toastEl, { delay:5000 });
 toast.show();
 }
 // expose globally
 window.appShowToast = showToast;

 // Helper to post JSON and handle response
 async function postJson(url){
 const res = await fetch(url, { method: 'POST', headers: {'Content-Type':'application/json'} });
 if(res.ok) return await res.json();
 let err;
 try{ err = await res.json(); }catch(e){ err = { message: res.statusText }; }
 throw err;
 }

 async function deleteJson(url){
 const res = await fetch(url, { method: 'DELETE' });
 if(res.ok) return await res.json();
 let err;
 try{ err = await res.json(); }catch(e){ err = { message: res.statusText }; }
 throw err;
 }

 // Attach click handlers for elements with class .btn-assign-docente and .btn-unassign-docente
 document.addEventListener('click', function(e){
 const assignBtn = e.target.closest && e.target.closest('.btn-assign-docente');
 if(assignBtn){
 e.preventDefault();
 const cursoId = assignBtn.getAttribute('data-curso-id');
 const docenteId = assignBtn.getAttribute('data-docente-id');
 if(!cursoId || !docenteId) return showToast('Datos incompletos', 'danger');
 postJson(`/api/cursos/${cursoId}/docentes/${docenteId}`)
 .then(j=>{ showToast(j.message || 'Docente asignado', 'success');
 // optional: reload to reflect changes
 setTimeout(()=> location.reload(),800);
 })
 .catch(err=>{ showToast(err?.message || 'Error al asignar', 'danger'); });
 return;
 }
 const unassignBtn = e.target.closest && e.target.closest('.btn-unassign-docente');
 if(unassignBtn){
 e.preventDefault();
 const cursoId = unassignBtn.getAttribute('data-curso-id');
 const docenteId = unassignBtn.getAttribute('data-docente-id');
 if(!cursoId || !docenteId) return showToast('Datos incompletos', 'danger');
 if(!confirm('¿Confirma quitar el docente del curso?')) return;
 deleteJson(`/api/cursos/${cursoId}/docentes/${docenteId}`)
 .then(j=>{ showToast(j.message || 'Docente quitado', 'success'); setTimeout(()=> location.reload(),800); })
 .catch(err=>{ showToast(err?.message || 'Error al quitar', 'danger'); });
 return;
 }
 });

 // Ensure appShowToast exists early
 function ensureToast(){ if (typeof window.appShowToast !== 'function'){ window.appShowToast = function(message, type){ alert(message); }; } }
 ensureToast();

 // Intercept protected links and show friendly toast when user likely lacks permission
 document.addEventListener('click', function(e){
 var a = e.target.closest && e.target.closest('a[data-protect]');
 if (!a) return;
 // Do a simple check: if link has attribute data-protect and also class 'disabled-for-user' then prevent default and show toast
 if (a.classList.contains('disabled-for-user')){
 e.preventDefault();
 window.appShowToast('No tiene permiso para acceder a esta sección', 'warning');
 return;
 }
 // Otherwise allow navigation; server still authorizes
 });
})();
