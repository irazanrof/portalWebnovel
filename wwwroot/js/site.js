
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('textarea.capitulo-texto-input').forEach(function (ta) {
        var ajustar = function () {
            var bloco = ta.closest('form') || ta.parentElement;
            var rolagem = window.scrollY;
            bloco.style.minHeight = bloco.offsetHeight + 'px';
            ta.style.height = 'auto';
            ta.style.height = ta.scrollHeight + 'px';
            bloco.style.minHeight = '';
            window.scrollTo(0, rolagem);
        };
        ta.addEventListener('input', ajustar);
        window.addEventListener('resize', ajustar);
        ajustar();
    });
});
