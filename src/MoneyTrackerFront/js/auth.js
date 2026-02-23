const API_BASE_URL = 'http://localhost:5183/api';

// Обработка формы регистрации
const registerForm = document.getElementById('register-form');
if (registerForm) {
    registerForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirm-password').value;
        const registerButton = document.getElementById('register-button');

        if (password !== confirmPassword) {
            showError('Пароли не совпадают');
            return;
        }

        const userData = {
            userName: document.getElementById('username').value,
            email: document.getElementById('email').value,
            password: password
        };

        // Показываем состояние загрузки
        registerButton.classList.add('loading');
        registerButton.disabled = true;

        try {
            const response = await fetch(`${API_BASE_URL}/auth/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(userData)
            });

            const data = await response.json();

            if (response.ok) {
                showSuccess('Регистрация успешна! Перенаправляем...');
                localStorage.setItem('token', data.token);
                localStorage.setItem('user', JSON.stringify({
                    id: data.userId,
                    name: data.userName,
                    email: data.email
                }));

                setTimeout(() => {
                    window.location.href = 'index.html';
                }, 1500);
            } else {
                showError(data.message || 'Ошибка регистрации');
                registerButton.classList.remove('loading');
                registerButton.disabled = false;
            }
        } catch (error) {
            console.error('Ошибка:', error);
            showError('Ошибка соединения с сервером');
            registerButton.classList.remove('loading');
            registerButton.disabled = false;
        }
    });
}

// Обработка формы входа
const loginForm = document.getElementById('login-form');
if (loginForm) {
    loginForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const loginData = {
            email: document.getElementById('email').value,
            password: document.getElementById('password').value
        };
        const loginButton = document.getElementById('login-button');

        // Показываем состояние загрузки
        loginButton.classList.add('loading');
        loginButton.disabled = true;

        try {
            const response = await fetch(`${API_BASE_URL}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(loginData)
            });

            const data = await response.json();

            if (response.ok) {
                showSuccess('Вход выполнен! Перенаправляем...');
                localStorage.setItem('token', data.token);
                localStorage.setItem('user', JSON.stringify({
                    id: data.userId,
                    name: data.userName,
                    email: data.email
                }));

                setTimeout(() => {
                    window.location.href = 'index.html';
                }, 1500);
            } else {
                showError(data.message || 'Неверный email или пароль');
                loginButton.classList.remove('loading');
                loginButton.disabled = false;
            }
        } catch (error) {
            console.error('Ошибка:', error);
            showError('Ошибка соединения с сервером');
            loginButton.classList.remove('loading');
            loginButton.disabled = false;
        }
    });
}

function showError(message) {
    const existingError = document.querySelector('.error-message');
    if (existingError) existingError.remove();

    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.textContent = message;

    const form = document.querySelector('form');
    form.parentNode.insertBefore(errorDiv, form);
}

function showSuccess(message) {
    const existingSuccess = document.querySelector('.success-message');
    if (existingSuccess) existingSuccess.remove();

    const successDiv = document.createElement('div');
    successDiv.className = 'success-message';
    successDiv.textContent = message;

    const form = document.querySelector('form');
    form.parentNode.insertBefore(successDiv, form);
}