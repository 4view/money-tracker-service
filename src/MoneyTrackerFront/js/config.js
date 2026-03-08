// config.js — единая точка конфигурации для всего фронтенда
// Подключается первым скриптом на каждой странице.

const API_BASE_URL = 'http://localhost:8080/api';

/**
 * Экранирует HTML-спецсимволы для защиты от XSS.
 * Используй везде, где вставляешь пользовательский текст в innerHTML.
 */
function escapeHtml(str) {
    if (str == null) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}