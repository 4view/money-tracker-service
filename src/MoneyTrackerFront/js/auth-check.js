// Проверка авторизации для защищенных страниц
const token = localStorage.getItem('token');
const user = JSON.parse(localStorage.getItem('user') || '{}');

if (!token) {
    window.location.href = 'login.html';
}

// Отображение информации о пользователе
document.addEventListener('DOMContentLoaded', function() {
    const userNameElement = document.getElementById('user-name');
    if (userNameElement && user.name) {
        userNameElement.textContent = user.name;
    }
});