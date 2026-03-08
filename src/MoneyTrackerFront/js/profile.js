// profile.js — страница профиля пользователя

// ─── Утилита: защита от XSS ──────────────────────────────────────
function _esc(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

// ─── fetchWithAuth (использует API_BASE_URL из config.js) ────────
async function fetchWithAuth(url, options = {}) {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'login.html';
        return null;
    }
    try {
        const res = await fetch(url, {
            ...options,
            headers: {
                ...options.headers,
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
        });
        if (res.status === 401) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = 'login.html';
            return null;
        }
        return res;
    } catch (err) {
        console.error('Ошибка сети:', err);
        throw err;
    }
}

// ─── Загрузка профиля ────────────────────────────────────────────
async function loadProfile() {
    try {
        const res = await fetchWithAuth(`${API_BASE_URL}/profile`);
        if (!res || !res.ok) return;

        const profile = await res.json();

        // Аватар — первая буква имени
        const avatar = document.getElementById('profile-avatar');
        if (avatar) avatar.textContent = (profile.userName || '?')[0].toUpperCase();

        const heroName = document.getElementById('profile-hero-name');
        const heroEmail = document.getElementById('profile-hero-email');
        const heroDate = document.getElementById('profile-hero-date');
        const currentUN = document.getElementById('current-username');
        const infoEmail = document.getElementById('info-email');
        const infoCreated = document.getElementById('info-created');

        const createdStr = profile.createdAt
            ? new Date(profile.createdAt).toLocaleDateString('ru-RU', { day: '2-digit', month: 'long', year: 'numeric' })
            : '—';

        if (heroName) heroName.textContent = _esc(profile.userName);
        if (heroEmail) heroEmail.textContent = _esc(profile.email);
        if (heroDate) heroDate.textContent = 'С нами с ' + createdStr;
        if (currentUN) currentUN.textContent = _esc(profile.userName);
        if (infoEmail) infoEmail.textContent = _esc(profile.email);
        if (infoCreated) infoCreated.textContent = createdStr;

        // Обновляем шапку
        const headerName = document.getElementById('user-name');
        if (headerName) headerName.textContent = _esc(profile.userName);

    } catch (err) {
        console.error('Ошибка загрузки профиля:', err);
    }
}

// ─── Сохранить имя ───────────────────────────────────────────────
async function saveUsername() {
    const input = document.getElementById('new-username');
    const btn = document.getElementById('save-username-btn');
    const fb = document.getElementById('username-feedback');

    const newName = input ? input.value.trim() : '';
    if (!newName) {
        showFeedback(fb, 'Введите новое имя', 'error');
        return;
    }

    setLoading(btn, true);
    hideFeedback(fb);

    try {
        const res = await fetchWithAuth(`${API_BASE_URL}/profile/username`, {
            method: 'PUT',
            body: JSON.stringify({ userName: newName }),
        });

        if (!res) return;

        if (res.ok) {
            const profile = await res.json();

            // Обновляем localStorage
            const stored = JSON.parse(localStorage.getItem('user') || '{}');
            stored.name = profile.userName;
            localStorage.setItem('user', JSON.stringify(stored));

            // Обновляем UI
            const currentUN = document.getElementById('current-username');
            const heroName = document.getElementById('profile-hero-name');
            const headerName = document.getElementById('user-name');
            const avatar = document.getElementById('profile-avatar');

            if (currentUN) currentUN.textContent = _esc(profile.userName);
            if (heroName) heroName.textContent = _esc(profile.userName);
            if (headerName) headerName.textContent = _esc(profile.userName);
            if (avatar) avatar.textContent = (profile.userName || '?')[0].toUpperCase();

            if (input) input.value = '';
            showFeedback(fb, '✓ Имя успешно изменено', 'success');
            showToast('Имя обновлено', 'success');
        } else {
            const data = await res.json().catch(() => ({}));
            showFeedback(fb, data.message || 'Ошибка при сохранении', 'error');
        }
    } catch (err) {
        showFeedback(fb, 'Ошибка соединения с сервером', 'error');
    } finally {
        setLoading(btn, false);
    }
}

// ─── Сохранить пароль ────────────────────────────────────────────
async function savePassword() {
    const currentPwd = document.getElementById('current-password');
    const newPwd = document.getElementById('new-password');
    const confirmPwd = document.getElementById('confirm-password');
    const btn = document.getElementById('save-password-btn');
    const fb = document.getElementById('password-feedback');

    const current = currentPwd ? currentPwd.value : '';
    const next = newPwd ? newPwd.value : '';
    const confirm = confirmPwd ? confirmPwd.value : '';

    if (!current) { showFeedback(fb, 'Введите текущий пароль', 'error'); return; }
    if (next.length < 6) { showFeedback(fb, 'Новый пароль должен содержать минимум 6 символов', 'error'); return; }
    if (next !== confirm) { showFeedback(fb, 'Пароли не совпадают', 'error'); return; }

    setLoading(btn, true);
    hideFeedback(fb);

    try {
        const res = await fetchWithAuth(`${API_BASE_URL}/profile/password`, {
            method: 'PUT',
            body: JSON.stringify({ currentPassword: current, newPassword: next }),
        });

        if (!res) return;

        if (res.ok) {
            if (currentPwd) currentPwd.value = '';
            if (newPwd) newPwd.value = '';
            if (confirmPwd) confirmPwd.value = '';
            updateStrength('');
            showFeedback(fb, '✓ Пароль успешно изменён', 'success');
            showToast('Пароль изменён', 'success');
        } else {
            const data = await res.json().catch(() => ({}));
            showFeedback(fb, data.message || 'Ошибка при смене пароля', 'error');
        }
    } catch (err) {
        showFeedback(fb, 'Ошибка соединения с сервером', 'error');
    } finally {
        setLoading(btn, false);
    }
}

// ─── Индикатор надёжности пароля ─────────────────────────────────
function updateStrength(password) {
    const bar = document.getElementById('strength-fill');
    const label = document.getElementById('strength-label');
    const wrap = document.getElementById('password-strength');

    if (!bar || !label || !wrap) return;

    if (!password) {
        wrap.classList.add('hidden');
        bar.style.width = '0%';
        return;
    }

    wrap.classList.remove('hidden');

    let score = 0;
    if (password.length >= 6) score++;
    if (password.length >= 10) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;

    const levels = [
        { pct: '20%', color: '#dc3545', text: 'Слабый' },
        { pct: '40%', color: '#fd7e14', text: 'Слабый' },
        { pct: '60%', color: '#ffc107', text: 'Средний' },
        { pct: '80%', color: '#20c997', text: 'Хороший' },
        { pct: '100%', color: '#28a745', text: 'Сильный' },
    ];

    const lvl = levels[Math.max(0, score - 1)] || levels[0];
    bar.style.width = lvl.pct;
    bar.style.backgroundColor = lvl.color;
    label.textContent = lvl.text;
    label.style.color = lvl.color;
}

// ─── Вспомогательные ─────────────────────────────────────────────
function showFeedback(el, msg, type) {
    if (!el) return;
    el.textContent = msg;
    el.className = `profile-feedback ${type}`;
    el.classList.remove('hidden');
}

function hideFeedback(el) {
    if (!el) return;
    el.classList.add('hidden');
    el.textContent = '';
}

function setLoading(btn, on) {
    if (!btn) return;
    btn.disabled = on;
    if (on) btn.classList.add('loading');
    else btn.classList.remove('loading');
}

// ─── Инициализация ────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', async () => {
    await loadProfile();

    // Кнопки сохранения
    document.getElementById('save-username-btn')?.addEventListener('click', saveUsername);
    document.getElementById('save-password-btn')?.addEventListener('click', savePassword);

    // Показать/скрыть пароль
    document.querySelectorAll('.toggle-password').forEach(btn => {
        btn.addEventListener('click', () => {
            const targetId = btn.dataset.target;
            const inp = document.getElementById(targetId);
            if (!inp) return;
            inp.type = inp.type === 'password' ? 'text' : 'password';
            btn.textContent = inp.type === 'password' ? '👁' : '🙈';
        });
    });

    // Индикатор надёжности
    document.getElementById('new-password')?.addEventListener('input', e => {
        updateStrength(e.target.value);
        // Сбрасываем фидбек при вводе
        hideFeedback(document.getElementById('password-feedback'));
    });

    // Выход
    document.getElementById('logout-btn')?.addEventListener('click', () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = 'login.html';
    });

    // Enter в поле имени
    document.getElementById('new-username')?.addEventListener('keydown', e => {
        if (e.key === 'Enter') saveUsername();
    });
});