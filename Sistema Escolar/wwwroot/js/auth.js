(function(){
 document.addEventListener('DOMContentLoaded', function(){
 const form = document.querySelector('#loginForm');
 if(!form) return;
 form.addEventListener('submit', async function(ev){
 ev.preventDefault();
 const email = form.querySelector('input[name="Email"]').value;
 const password = form.querySelector('input[name="Password"]').value;
 const btn = form.querySelector('button[type="submit"]');
 btn.disabled = true; btn.innerHTML = 'Ingresando...';
 try{
 const r = await fetch(form.action, {
 method: 'POST',
 headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
 body: JSON.stringify({ email, password }),
 credentials: 'same-origin'
 });
 if(r.ok){
 const json = await r.json();
 // Si el servidor retorna una redirección, usarla; si retorna token, guardar cookie/local
 if(json.redirect){ window.location.href = json.redirect; return; }
 if(json.token){ localStorage.setItem('token', json.token); }
 window.location.reload();
 } else {
 const err = await r.json().catch(()=>({message:'Error al autenticar'}));
 showError(err.message || 'Credenciales incorrectas');
 }
 } catch(e){ showError(e.message || 'Error de red'); }
 finally{ btn.disabled = false; btn.innerHTML = 'Ingresar'; }
 });
 
 function showError(msg){
 let el = document.getElementById('loginError');
 if(!el){ el = document.createElement('div'); el.id='loginError'; el.className='alert alert-danger mt-3'; document.querySelector('#loginForm').appendChild(el); }
 el.textContent = msg;
 }
 });
})();