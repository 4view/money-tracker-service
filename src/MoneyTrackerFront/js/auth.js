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

        registerButton.classList.add('loading');
        registerButton.disabled = true;

        try {
            const response = await fetch(`${API_BASE_URL}/auth/register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(userData)
            });

            const data = await response.json();

            if (response.ok) {
                // ✅ ИСПРАВЛЕНО: не сохраняем токен и не пускаем на главную —
                // пользователь должен сначала подтвердить email
                showSuccess('Регистрация успешна! Проверьте почту для подтверждения.');

                // Прячем форму, показываем инструкцию
                registerForm.style.display = 'none';
                showEmailSentHint(userData.email);
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

        loginButton.classList.add('loading');
        loginButton.disabled = true;

        try {
            const response = await fetch(`${API_BASE_URL}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
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
                const message = data.message || 'Неверный email или пароль';
                showError(message);

                // Если email не подтверждён — показываем кнопку повторной отправки
                if (message.includes('не подтверждён')) {
                    showResendButton(document.getElementById('email').value);
                }

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

    // Добавляем ссылку «Забыли пароль?» под кнопкой входа
    const forgotLink = document.createElement('div');
    forgotLink.className = 'auth-switch';
    forgotLink.style.marginTop = '12px';
    forgotLink.innerHTML = '<a href="forgotPassword.html">Забыли пароль?</a>';
    loginForm.after(forgotLink);
}

// ─── Вспомогательные функции ─────────────────────────────────────────────────

function showError(message) {
    const existing = document.querySelector('.error-message');
    if (existing) existing.remove();

    const div = document.createElement('div');
    div.className = 'error-message';
    div.textContent = message;

    const form = document.querySelector('form');
    form.parentNode.insertBefore(div, form);
}

function showSuccess(message) {
    // Убираем и ошибку, и предыдущий успех — показываем только одно сообщение
    document.querySelector('.success-message')?.remove();
    document.querySelector('.error-message')?.remove();
    document.querySelector('.resend-btn')?.remove();

    const div = document.createElement('div');
    div.className = 'success-message';
    div.textContent = message;

    const form = document.querySelector('form');
    form.parentNode.insertBefore(div, form);
}

// Показывает подсказку после регистрации — "письмо отправлено на ..."
function showEmailSentHint(email) {
    const existing = document.querySelector('.email-sent-hint');
    if (existing) existing.remove();

    const div = document.createElement('div');
    div.className = 'email-sent-hint';
    div.innerHTML = `
        <div style="text-align:center; padding: 16px 0;">
            <div style="font-size: 40px; margin-bottom: 12px;">✉️</div>
            <p style="margin-bottom: 8px; color: var(--text-primary); font-weight: 500;">
                Письмо отправлено на<br><strong>${email}</strong>
            </p>
            <p style="font-size: 13px; color: var(--text-secondary); margin-bottom: 16px;">
                Перейдите по ссылке в письме, чтобы активировать аккаунт.<br>
                Проверьте папку «Спам», если письмо не пришло.
            </p>
            <a href="login.html" style="color: var(--accent-color); font-size: 14px;">
                Перейти к входу
            </a>
        </div>
    `;

    const card = document.querySelector('.auth-card') || document.querySelector('form').parentNode;
    card.appendChild(div);
}

// Показывает кнопку «Отправить письмо повторно» на странице входа
function showResendButton(email) {
    const existing = document.querySelector('.resend-btn');
    if (existing) return;

    const btn = document.createElement('button');
    btn.className = 'resend-btn';
    btn.textContent = 'Отправить письмо повторно';
    btn.style.cssText = `
        display: block; width: 100%; margin-top: 8px; padding: 10px;
        background: transparent; color: var(--accent-color);
        border: 1px solid var(--accent-color); border-radius: 8px;
        font-size: 14px; cursor: pointer;
    `;

    btn.addEventListener('click', async () => {
        btn.disabled = true;
        btn.textContent = 'Отправляем...';

        try {
            await fetch(`${API_BASE_URL}/auth/resend-confirmation`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email })
            });
            btn.textContent = '✓ Письмо отправлено';
        } catch {
            btn.textContent = 'Ошибка. Попробуйте позже.';
        }
    });

    const errorDiv = document.querySelector('.error-message');
    if (errorDiv) errorDiv.after(btn);
}