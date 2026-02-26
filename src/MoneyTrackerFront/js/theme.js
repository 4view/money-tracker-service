// Управление темой
(function () {
    // Проверяем сохранённую тему
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);

    // Функция для переключения темы
    window.toggleTheme = function () {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';

        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);

        // Обновляем текст кнопки
        updateThemeButtonText();

        console.log('Тема переключена на:', newTheme); // Для отладки
    };

    // Функция обновления текста кнопки
    function updateThemeButtonText() {
        const themeBtn = document.getElementById('theme-toggle');
        if (themeBtn) {
            const currentTheme = document.documentElement.getAttribute('data-theme');
            themeBtn.innerHTML = currentTheme === 'light'
                ? '🌙'
                : '☀️';
        }
    }

    // Добавляем обработчик после загрузки DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            updateThemeButtonText();
            const themeBtn = document.getElementById('theme-toggle');
            if (themeBtn) {
                themeBtn.addEventListener('click', toggleTheme);
            }
        });
    } else {
        // DOM уже загружен
        updateThemeButtonText();
        const themeBtn = document.getElementById('theme-toggle');
        if (themeBtn) {
            themeBtn.addEventListener('click', toggleTheme);
        }
    }
})();