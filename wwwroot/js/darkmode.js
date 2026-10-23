// Dark mode toggle. The choice is saved in localStorage so it survives page reloads.
// _Layout.cshtml applies the saved choice in <head> to avoid a white flash.
var DARK_MODE_KEY = 'cedardelivery-dark';

function setDarkModeButton(isDark) {
    var btn = document.getElementById('darkModeBtn');
    if (btn) {
        btn.innerHTML = isDark
            ? '<i class="bi bi-sun"></i>'
            : '<i class="bi bi-moon"></i>';
    }
}

function toggleDarkMode() {
    var isDark = document.documentElement.classList.toggle('dark-mode');
    document.body.classList.toggle('dark-mode', isDark);
    localStorage.setItem(DARK_MODE_KEY, isDark ? '1' : '0');
    setDarkModeButton(isDark);
}

document.addEventListener('DOMContentLoaded', function () {
    var isDark = localStorage.getItem(DARK_MODE_KEY) === '1';
    document.documentElement.classList.toggle('dark-mode', isDark);
    document.body.classList.toggle('dark-mode', isDark);
    setDarkModeButton(isDark);
});
