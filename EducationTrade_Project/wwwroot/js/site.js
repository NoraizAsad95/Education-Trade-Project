// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Password visibility toggle — works for ALL password fields globally, including HTMX-injected modals
document.addEventListener('click', function (e) {
    const btn = e.target.closest('.toggle-password-btn');
    if (!btn) return;

    // Find the sibling input relative to the clicked button — no IDs needed
    const input = btn.closest('.input-group').querySelector('input');
    const icon  = btn.querySelector('i');

    const isPassword = input.type === 'password';
    input.type = isPassword ? 'text' : 'password';

    // Toggle the Bootstrap eye icons cleanly
    icon.classList.toggle('bi-eye',       isPassword);
    icon.classList.toggle('bi-eye-slash', !isPassword);
});
