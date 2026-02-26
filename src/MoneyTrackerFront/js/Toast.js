/**
 * toast.js — система всплывающих уведомлений
 * Использование:
 *   showToast('Трата удалена', 'success')
 *   showToast('Ошибка соединения', 'error')
 *   showToast('Заполните все поля', 'warning')
 *   showToast('Сессия истекла', 'info')
 */

(function () {
    // Создаём контейнер один раз
    function getContainer() {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            document.body.appendChild(container);
            injectStyles();
        }
        return container;
    }

    function injectStyles() {
        if (document.getElementById('toast-styles')) return;
        const style = document.createElement('style');
        style.id = 'toast-styles';
        style.textContent = `
            #toast-container {
                position: fixed;
                bottom: 24px;
                left: 50%;
                transform: translateX(-50%);
                z-index: 99999;
                display: flex;
                flex-direction: column-reverse;
                align-items: center;
                gap: 10px;
                pointer-events: none;
                width: calc(100% - 32px);
                max-width: 420px;
            }

            .toast {
                display: flex;
                align-items: center;
                gap: 10px;
                padding: 13px 16px;
                border-radius: 12px;
                font-size: 14px;
                font-weight: 500;
                line-height: 1.4;
                box-shadow: 0 4px 20px rgba(0,0,0,0.25);
                pointer-events: auto;
                width: 100%;
                backdrop-filter: blur(8px);
                -webkit-backdrop-filter: blur(8px);
                animation: toastIn 0.3s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
                cursor: pointer;
                user-select: none;
            }

            .toast.hiding {
                animation: toastOut 0.25s ease forwards;
            }

            .toast-icon {
                font-size: 18px;
                flex-shrink: 0;
                line-height: 1;
            }

            .toast-text {
                flex: 1;
            }

            /* Типы */
            .toast-success {
                background: rgba(30, 50, 35, 0.97);
                color: #6ee87a;
                border: 1px solid rgba(76, 175, 80, 0.4);
            }

            .toast-error {
                background: rgba(50, 20, 20, 0.97);
                color: #f87171;
                border: 1px solid rgba(244, 67, 54, 0.4);
            }

            .toast-warning {
                background: rgba(50, 40, 10, 0.97);
                color: #fbbf24;
                border: 1px solid rgba(255, 183, 77, 0.4);
            }

            .toast-info {
                background: rgba(15, 30, 55, 0.97);
                color: #93c5fd;
                border: 1px solid rgba(100, 181, 246, 0.4);
            }

            /* Светлая тема */
            :root:not([data-theme="dark"]) .toast-success {
                background: rgba(240, 253, 244, 0.97);
                color: #166534;
                border: 1px solid rgba(76, 175, 80, 0.35);
            }
            :root:not([data-theme="dark"]) .toast-error {
                background: rgba(254, 242, 242, 0.97);
                color: #991b1b;
                border: 1px solid rgba(244, 67, 54, 0.3);
            }
            :root:not([data-theme="dark"]) .toast-warning {
                background: rgba(255, 251, 235, 0.97);
                color: #92400e;
                border: 1px solid rgba(255, 183, 77, 0.4);
            }
            :root:not([data-theme="dark"]) .toast-info {
                background: rgba(239, 246, 255, 0.97);
                color: #1e40af;
                border: 1px solid rgba(100, 181, 246, 0.35);
            }

            @keyframes toastIn {
                from {
                    opacity: 0;
                    transform: translateY(16px) scale(0.95);
                }
                to {
                    opacity: 1;
                    transform: translateY(0) scale(1);
                }
            }

            @keyframes toastOut {
                from {
                    opacity: 1;
                    transform: translateY(0) scale(1);
                    max-height: 80px;
                    margin-bottom: 0;
                }
                to {
                    opacity: 0;
                    transform: translateY(8px) scale(0.95);
                    max-height: 0;
                    margin-bottom: -10px;
                }
            }
        `;
        document.head.appendChild(style);
    }

    const ICONS = {
        success: '✓',
        error:   '✕',
        warning: '⚠',
        info:    'ℹ',
    };

    const DURATIONS = {
        success: 2800,
        error:   4500,
        warning: 3500,
        info:    3000,
    };

    /**
     * @param {string} message  — текст уведомления
     * @param {'success'|'error'|'warning'|'info'} type
     * @param {number} [duration] — ms, 0 = не исчезает автоматически
     */
    window.showToast = function (message, type = 'info', duration) {
        const container = getContainer();

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <span class="toast-icon">${ICONS[type] || 'ℹ'}</span>
            <span class="toast-text">${message}</span>
        `;

        function dismiss() {
            toast.classList.add('hiding');
            toast.addEventListener('animationend', () => toast.remove(), { once: true });
        }

        toast.addEventListener('click', dismiss);

        container.appendChild(toast);

        const ms = duration !== undefined ? duration : DURATIONS[type] || 3000;
        if (ms > 0) {
            setTimeout(dismiss, ms);
        }
    };
})();