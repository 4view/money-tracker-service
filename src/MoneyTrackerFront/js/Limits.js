// limits.js — управление лимитами категорий

const API_BASE_URL = 'http://localhost:5183/api';

let allCategories = [];
let allLimits = [];       // AddedLimitDto[]  (id, categoryId, limit)
let calculatedLimits = {}; // { limitId: ReturnedLimitDto }
let currentDeleteId = null;
let currentPeriod = { month: null, year: null };

// ─── ИНИЦИАЛИЗАЦИЯ ───────────────────────────────────────────
document.addEventListener('DOMContentLoaded', async () => {
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const userNameEl = document.getElementById('user-name');
    if (userNameEl && user.name) userNameEl.textContent = user.name;

    // Текущий месяц / год по умолчанию
    const now = new Date();
    currentPeriod.month = now.getMonth() + 1;
    currentPeriod.year  = now.getFullYear();

    fillYearSelect();
    setSelectedPeriod();
    initEventListeners();

    await loadCategories();
    await loadAndRenderLimits();
});

// ─── ВСПОМОГАТЕЛЬНЫЕ ─────────────────────────────────────────
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

/** Unix-метка начала месяца (UTC, ms) */
function periodToTimestamps(year, month) {
    const from = new Date(Date.UTC(year, month - 1, 1)).getTime();
    const to   = new Date(Date.UTC(year, month, 1)).getTime(); // начало следующего месяца
    return { from, to };
}

function fillYearSelect() {
    const sel = document.getElementById('period-year');
    const cur = new Date().getFullYear();
    for (let y = cur; y >= cur - 5; y--) {
        const opt = document.createElement('option');
        opt.value = y;
        opt.textContent = y;
        sel.appendChild(opt);
    }
}

function setSelectedPeriod() {
    document.getElementById('period-month').value = currentPeriod.month;
    document.getElementById('period-year').value  = currentPeriod.year;
}

// ─── ЗАГРУЗКА ДАННЫХ ─────────────────────────────────────────
async function loadCategories() {
    try {
        const res = await fetchWithAuth(`${API_BASE_URL}/category`);
        if (!res || !res.ok) return;
        allCategories = await res.json();
        fillCategorySelect('new-limit-category', allCategories);
    } catch (e) {
        console.error('loadCategories:', e);
    }
}

function fillCategorySelect(selectId, categories) {
    const sel = document.getElementById(selectId);
    if (!sel) return;
    // Оставляем первый option-заглушку
    const firstOpt = sel.querySelector('option[value=""]');
    sel.innerHTML = '';
    if (firstOpt) sel.appendChild(firstOpt);
    categories.forEach(cat => {
        const opt = document.createElement('option');
        opt.value = cat.id;
        opt.textContent = cat.name;
        sel.appendChild(opt);
    });
}

async function loadAndRenderLimits() {
    renderSkeleton();
    try {
        // 1. Загружаем список лимитов
        const res = await fetchWithAuth(`${API_BASE_URL}/limit`);
        if (!res) return;
        if (!res.ok) {
            showEmptyState('Ошибка загрузки лимитов');
            return;
        }
        allLimits = await res.json(); // AddedLimitDto[]

        if (allLimits.length === 0) {
            showEmptyState('Нет лимитов. Добавьте первый!');
            filterCategorySelect(); // убрать недоступные категории
            return;
        }

        // 2. Для каждого лимита рассчитываем остаток за выбранный период
        await calculateAllLimits();

        // 3. Рендерим
        renderLimits();
        filterCategorySelect();
    } catch (e) {
        console.error('loadAndRenderLimits:', e);
        showEmptyState('Ошибка загрузки');
    }
}

async function calculateAllLimits() {
    const { from, to } = periodToTimestamps(currentPeriod.year, currentPeriod.month);
    calculatedLimits = {};

    await Promise.all(allLimits.map(async (lim) => {
        try {
            const url = `${API_BASE_URL}/limit/${lim.id}/calculate`
                + `?categoryId=${lim.categoryId}&startDate=${from}&endDate=${to}`;
            const res = await fetchWithAuth(url);
            if (res && res.ok) {
                calculatedLimits[lim.id] = await res.json();
            }
        } catch (e) {
            console.warn('calculateLimit error:', lim.id, e);
        }
    }));
}

// ─── РЕНДЕР ──────────────────────────────────────────────────
function renderSkeleton() {
    const list = document.getElementById('limits-list');
    list.innerHTML = '';
    for (let i = 0; i < 3; i++) {
        const el = document.createElement('div');
        el.className = 'limit-item skeleton';
        el.innerHTML = `
            <div class="limit-header">
                <div class="limit-category-name" style="width:60%;height:14px;">&nbsp;</div>
            </div>
            <div class="limit-progress-wrap">&nbsp;</div>
            <div class="limit-amounts" style="height:12px;">&nbsp;</div>
        `;
        list.appendChild(el);
    }
}

function showEmptyState(msg) {
    document.getElementById('limits-list').innerHTML =
        `<div class="limits-empty">${msg}</div>`;
}

function renderLimits() {
    const list = document.getElementById('limits-list');
    list.innerHTML = '';

    allLimits.forEach((lim, idx) => {
        const calc = calculatedLimits[lim.id];
        const card = buildLimitCard(lim, calc, idx);
        list.appendChild(card);

        // Анимируем прогресс-бар после вставки в DOM
        requestAnimationFrame(() => {
            const bar = card.querySelector('.limit-progress-bar');
            if (bar) {
                bar.style.width = bar.dataset.width;
            }
        });
    });
}

function buildLimitCard(lim, calc, idx) {
    const catName = getCategoryName(lim.categoryId);

    let spent    = 0;
    let remaining = lim.limit;
    let hasCalc  = false;

    if (calc) {
        spent     = lim.limit - calc.remaining;
        remaining = calc.remaining;
        hasCalc   = true;
    }

    const pct     = hasCalc ? Math.min(100, Math.max(0, (spent / lim.limit) * 100)) : 0;
    const status  = pct >= 100 ? 'danger' : pct >= 75 ? 'warning' : 'ok';
    const badgeText = pct >= 100 ? '🔴 Превышен' : pct >= 75 ? '🟡 Близко' : '🟢 В норме';

    const el = document.createElement('div');
    el.className = `limit-item${status !== 'ok' ? ' ' + (status === 'danger' ? 'over-budget' : 'near-budget') : ''}`;
    el.dataset.limitId = lim.id;
    el.style.animationDelay = `${idx * 0.05}s`;

    el.innerHTML = `
        <div class="limit-header">
            <span class="limit-category-name">${catName}</span>
            <span class="limit-badge ${status}">${badgeText}</span>
            <div class="limit-actions">
                <button class="limit-edit" title="Изменить">✎ Ред.</button>
                <button class="limit-delete" title="Удалить">🗑 Удал.</button>
            </div>
        </div>

        <div class="limit-progress-wrap">
            <div class="limit-progress-bar ${status}"
                 style="width: 0%"
                 data-width="${pct.toFixed(1)}%">
            </div>
        </div>

        <div class="limit-amounts">
            <span class="limit-spent">
                Потрачено: <strong>${hasCalc ? spent.toFixed(2) : '—'} ₽</strong>
                из ${lim.limit.toFixed(2)} ₽
            </span>
            <span class="limit-remaining ${status}">
                ${hasCalc
                    ? (remaining >= 0
                        ? `Осталось: ${remaining.toFixed(2)} ₽`
                        : `Перерасход: ${Math.abs(remaining).toFixed(2)} ₽`)
                    : '—'}
            </span>
            <span class="limit-total">${pct.toFixed(0)}% использовано</span>
        </div>
    `;

    // Обработчики кнопок
    el.querySelector('.limit-edit').addEventListener('click', () => openEditModal(lim));
    el.querySelector('.limit-delete').addEventListener('click', () => openDeleteModal(lim, catName));

    return el;
}

function getCategoryName(categoryId) {
    const cat = allCategories.find(c => String(c.id) === String(categoryId));
    return cat ? cat.name : 'Неизвестно';
}

/** Скрыть из select категории, у которых уже есть лимит */
function filterCategorySelect() {
    const sel = document.getElementById('new-limit-category');
    if (!sel) return;

    const usedIds = new Set(allLimits.map(l => String(l.categoryId)));

    Array.from(sel.options).forEach(opt => {
        if (!opt.value) return; // заглушка
        opt.disabled = usedIds.has(opt.value);
        opt.textContent = usedIds.has(opt.value)
            ? `${opt.textContent.replace(' (лимит уже есть)', '')} (лимит уже есть)`
            : opt.textContent.replace(' (лимит уже есть)', '');
    });
}

// ─── ДОБАВЛЕНИЕ ЛИМИТА ───────────────────────────────────────
async function addLimit() {
    const catSel = document.getElementById('new-limit-category');
    const amtInp = document.getElementById('new-limit-amount');
    const btn    = document.getElementById('add-limit-btn');

    const categoryId = catSel.value;
    const limit      = parseFloat(amtInp.value);

    if (!categoryId) { showToast('Выберите категорию', 'warning'); return; }
    if (!limit || limit <= 0) { showToast('Укажите корректный лимит', 'warning'); return; }

    btn.disabled    = true;
    btn.textContent = '...';

    try {
        const res = await fetchWithAuth(`${API_BASE_URL}/limit`, {
            method: 'POST',
            body: JSON.stringify({ categoryId, limit }),
        });
        if (!res) return;

        if (res.ok) {
            catSel.value = '';
            amtInp.value = '';
            await loadAndRenderLimits();
        } else {
            const err = await res.json().catch(() => ({}));
            showToast(err.message || `Ошибка ${res.status}`, 'error');
        }
    } catch (e) {
        showToast('Ошибка соединения', 'error');
    } finally {
        btn.disabled    = false;
        btn.textContent = 'Добавить';
    }
}

// ─── РЕДАКТИРОВАНИЕ ──────────────────────────────────────────
function openEditModal(lim) {
    document.getElementById('edit-limit-id').value          = lim.id;
    document.getElementById('edit-limit-category-id').value = lim.categoryId;
    document.getElementById('edit-category-display').textContent = getCategoryName(lim.categoryId);
    document.getElementById('edit-limit-amount').value      = lim.limit;
    document.getElementById('edit-limit-modal').style.display = 'flex';
    setTimeout(() => document.getElementById('edit-limit-amount').focus(), 200);
}

async function saveEditLimit() {
    const limitId = document.getElementById('edit-limit-id').value;
    const catId   = document.getElementById('edit-limit-category-id').value;
    const limit   = parseFloat(document.getElementById('edit-limit-amount').value);
    const btn     = document.getElementById('save-limit-edit');

    if (!limit || limit <= 0) { showToast('Укажите корректный лимит', 'warning'); return; }

    btn.disabled    = true;
    btn.textContent = '...';

    try {
        const res = await fetchWithAuth(`${API_BASE_URL}/limit/${limitId}`, {
            method: 'PUT',
            body: JSON.stringify({ categoryId: catId, limit }),
        });
        if (!res) return;

        if (res.ok || res.status === 204) {
            closeModal('edit-limit-modal');
            await loadAndRenderLimits();
        } else {
            const err = await res.json().catch(() => ({}));
            showToast(err.message || `Ошибка ${res.status}`, 'error');
        }
    } catch (e) {
        showToast('Ошибка соединения', 'error');
    } finally {
        btn.disabled    = false;
        btn.textContent = 'Сохранить';
    }
}

// ─── УДАЛЕНИЕ ────────────────────────────────────────────────
function openDeleteModal(lim, catName) {
    currentDeleteId = lim.id;
    document.getElementById('delete-limit-message').textContent =
        `Удалить лимит для категории "${catName}"?`;
    document.getElementById('delete-limit-modal').style.display = 'flex';
}

async function confirmDeleteLimit() {
    const btn = document.getElementById('confirm-limit-delete');
    btn.disabled    = true;
    btn.textContent = '...';

    try {
        const res = await fetchWithAuth(`${API_BASE_URL}/limit/${currentDeleteId}`, {
            method: 'DELETE',
        });
        if (!res) return;

        if (res.ok || res.status === 204) {
            closeModal('delete-limit-modal');
            await loadAndRenderLimits();
        } else {
            showToast('Ошибка при удалении', 'error');
        }
    } catch (e) {
        showToast('Ошибка соединения', 'error');
    } finally {
        btn.disabled    = false;
        btn.textContent = 'Удалить';
        currentDeleteId = null;
    }
}

// ─── МОДАЛЬНЫЕ ОКНА ──────────────────────────────────────────
function closeModal(id) {
    const el = document.getElementById(id);
    if (el) el.style.display = 'none';
}

// ─── ОБРАБОТЧИКИ СОБЫТИЙ ─────────────────────────────────────
function initEventListeners() {
    document.getElementById('add-limit-btn').addEventListener('click', addLimit);

    document.getElementById('new-limit-amount').addEventListener('keypress', e => {
        if (e.key === 'Enter') addLimit();
    });

    document.getElementById('apply-period').addEventListener('click', async () => {
        currentPeriod.month = parseInt(document.getElementById('period-month').value);
        currentPeriod.year  = parseInt(document.getElementById('period-year').value);
        await loadAndRenderLimits();
    });

    document.getElementById('save-limit-edit').addEventListener('click', saveEditLimit);
    document.getElementById('cancel-limit-edit').addEventListener('click', () => closeModal('edit-limit-modal'));

    document.getElementById('confirm-limit-delete').addEventListener('click', confirmDeleteLimit);
    document.getElementById('cancel-limit-delete').addEventListener('click', () => closeModal('delete-limit-modal'));

    // Закрытие по клику вне модального окна
    window.addEventListener('click', e => {
        ['edit-limit-modal', 'delete-limit-modal'].forEach(id => {
            const modal = document.getElementById(id);
            if (e.target === modal) closeModal(id);
        });
    });

    // Enter в поле редактирования
    document.getElementById('edit-limit-amount').addEventListener('keypress', e => {
        if (e.key === 'Enter') saveEditLimit();
    });
}