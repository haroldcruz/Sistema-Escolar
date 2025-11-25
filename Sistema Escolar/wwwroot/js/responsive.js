// Non-invasive helpers to improve responsiveness and UX on mobile devices
document.addEventListener('DOMContentLoaded', function(){
 // Wrap tables in .table-responsive if not already
 document.querySelectorAll('table').forEach(function(t){
 if (t.closest('.table-responsive')) return;
 var wrapper = document.createElement('div');
 wrapper.className = 'table-responsive';
 t.parentNode.insertBefore(wrapper, t);
 wrapper.appendChild(t);
 });

 // Ensure canvases are inside .canvas-container
 document.querySelectorAll('canvas').forEach(function(c){
 if (c.closest('.canvas-container')) return;
 var wrapper = document.createElement('div');
 wrapper.className = 'canvas-container';
 c.parentNode.insertBefore(wrapper, c);
 wrapper.appendChild(c);
 // set style for responsive chart
 c.style.maxWidth = '100%';
 c.style.height = 'auto';
 });

 // Auto-enable AJAX enhancement on known index pages to avoid touching server views
 try {
 var path = (window.location.pathname || '').toLowerCase();
 var enableOn = ['/cursos', '/historial', '/mihistorial', '/bitacora'];
 var shouldEnable = enableOn.some(function(p){ return path === p || path.startsWith(p + '/'); });
 if (shouldEnable){
 document.querySelectorAll('form').forEach(function(f){
 // don't override explicit opt-out
 if (f.hasAttribute('data-no-ajax')) return;
 // Only add attribute to forms that have a submit button and appear to be user-edit forms or filters
 var hasSubmit = !!f.querySelector('button[type=submit], input[type=submit]');
 if (!hasSubmit) return;
 // mark for responsive submit handling (non-invasive)
 f.setAttribute('data-ajax', 'true');
 });
 }
 } catch(e){ console.error('responsive.js enableOn error', e); }

 // Enhance form submits: disable submit buttons while AJAX is pending
 document.querySelectorAll('form[data-ajax="true"]').forEach(function(form){
 form.addEventListener('submit', function(e){
 var btn = form.querySelector('button[type=submit], input[type=submit]');
 if (btn){ btn.disabled = true; var original = btn.innerHTML; try{ btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Procesando...'; }catch(ex){} }
 // re-enable after10s as fallback
 setTimeout(function(){ if (btn){ btn.disabled = false; try{ btn.innerHTML = original; }catch(ex){} } },10000);
 });
 });
});