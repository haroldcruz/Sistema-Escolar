(function(){
 const form = document.getElementById('cursoCreateForm');
 if(!form) return;
 const saveKey = 'curso_create_draft_v1';
 // restore draft
 const draft = localStorage.getItem(saveKey);
 if(draft){
 try{ const obj = JSON.parse(draft); for(const k in obj){ const el = form.querySelector('[name="'+k+'"]').value!==undefined? form.querySelector('[name="'+k+'"]'): null; if(el) el.value = obj[k]; } }catch(e){}
 }
 // autosave
 form.addEventListener('input', ()=>{
 const data = {};
 new FormData(form).forEach((v,k)=> data[k]=v);
 localStorage.setItem(saveKey, JSON.stringify(data));
 });
 // ajax submit
 form.addEventListener('submit', function(e){
 e.preventDefault();
 const data = {};
 new FormData(form).forEach((v,k)=> data[k]=v);
 fetch('/api/cursos', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify(data)
 }).then(r=>{
 if(!r.ok) return r.json().then(j=> Promise.reject(j));
 return r.json();
 }).then(j=>{
 localStorage.removeItem(saveKey);
 alert(j.message || 'Curso creado');
 window.location.href = '/Cursos';
 }).catch(err=>{
 alert(err?.message || 'Error al crear curso');
 });
 });
})();
