// Конфигурация API - измените порт если нужно
const API_BASE_URL = 'http://localhost:5183/api';

// Состояние приложения
let html5QrCode = null;
let lastScannedData = [];
let currentQrData = null;
let isScanning = false;
let categories = []; // Для хранения списка категорий

// Проверка загрузки библиотеки
console.log('Проверка загрузки Html5Qrcode:', typeof Html5Qrcode);
console.log('Текущий пользователь:', user.name || 'Не авторизован');

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function () {
    console.log('Страница загружена');

    // Отображаем информацию о пользователе
    displayUserInfo();

    loadLastScans();
    checkCameraAvailability();
    loadCategories(); // Загружаем категории

    if (typeof displayStatistics === 'function') {
        displayStatistics();
    }

    // Проверяем загрузку библиотеки
    if (typeof Html5Qrcode === 'undefined') {
        const statusDiv = document.getElementById('camera-status');
        if (statusDiv) {
            statusDiv.innerHTML = '<div class="error-message">❌ Ошибка загрузки библиотеки сканирования</div>';
        }
    }

    // Инициализируем обработчики событий
    initializeEventListeners();
});

// Функция для отображения информации о пользователе
function displayUserInfo() {
    const userNameElement = document.getElementById('user-name');

    if (userNameElement && user.name) {
        userNameElement.textContent = user.name;
    }

    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function () {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = 'login.html';
        });
    }
}

// Функция для добавления токена в запросы
async function fetchWithAuth(url, options = {}) {
    const token = localStorage.getItem('token');

    if (!token) {
        showToast('Сессия истекла. Войдите снова.', 'error');
        window.location.href = 'login.html';
        return;
    }

    return fetch(url, {
        ...options,
        headers: {
            ...options.headers,
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
}

function closeScanner() {
    stopScanner().then(() => {
        document.getElementById('scanner-wrapper').classList.add('hidden');
    });
}

// Обработка touch-событий для мобильных
document.addEventListener('touchstart', (e) => {
    if (e.target.closest('.button')) {
        e.preventDefault();
    }
}, { passive: false });

// Загрузка категорий
async function loadCategories() {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/category`);
        if (response && response.ok) {
            categories = await response.json();
            console.log('Загруженные категории:', categories);

            // Убедимся, что у каждой категории есть id и name
            categories = categories.map(cat => ({
                id: cat.id,
                name: cat.name
            }));

            // Заполняем фильтр категорий
            const categoryFilter = document.getElementById('category-filter');
            const editCategorySelect = document.getElementById('edit-expense-category');

            if (categoryFilter) {
                categoryFilter.innerHTML = '<option value="">Все категории</option>';
                if (categories && categories.length > 0) {
                    categories.forEach(cat => {
                        const option = document.createElement('option');
                        option.value = cat.id;
                        option.textContent = cat.name;
                        categoryFilter.appendChild(option);
                    });
                }
            }

            if (editCategorySelect) {
                editCategorySelect.innerHTML = '';
                if (categories && categories.length > 0) {
                    categories.forEach(cat => {
                        const option = document.createElement('option');
                        option.value = cat.id;
                        option.textContent = cat.name;
                        editCategorySelect.appendChild(option);
                    });
                }
            }
        } else {
            console.error('Ошибка загрузки категорий:', response?.status);
        }
    } catch (error) {
        console.error('Ошибка загрузки категорий:', error);
    }
}

// Функция для обновления выпадающего списка категорий
function updateCategorySelect() {
    const categorySelect = document.getElementById('category-select');
    if (!categorySelect) return;

    categorySelect.innerHTML = '<option value="">Выберите категорию</option>';

    categories.forEach(category => {
        const option = document.createElement('option');
        option.value = category.id;
        option.textContent = category.name;
        categorySelect.appendChild(option);
    });
}

// Функция для инициализации обработчиков событий
function initializeEventListeners() {
    const startScanBtn = document.getElementById('start-scan');
    const stopScanBtn = document.getElementById('stop-scan');
    const closeScannerBtn = document.getElementById('close-scanner');
    const saveExpenseBtn = document.getElementById('save-expense');
    const scanNewBtn = document.getElementById('scan-new');

    // Кнопки модального окна
    const saveEditsBtn = document.getElementById('save-edits');
    const cancelEditBtn = document.getElementById('cancel-edit');

    if (startScanBtn) {
        startScanBtn.addEventListener('click', function () {
            document.getElementById('scanner-wrapper').classList.remove('hidden');
            startScanner();
        });
    }

    if (stopScanBtn) {
        stopScanBtn.addEventListener('click', function () {
            closeScanner();
        });
    }

    if (closeScannerBtn) {
        closeScannerBtn.addEventListener('click', function () {
            closeScanner();
        });
    }

    if (saveExpenseBtn) {
        saveExpenseBtn.addEventListener('click', function () {
            if (currentQrData) {
                saveExpense(currentQrData);
            }
        });
    }

    if (scanNewBtn) {
        scanNewBtn.addEventListener('click', function () {
            const resultContainer = document.getElementById('result-container');
            if (resultContainer) {
                resultContainer.classList.add('hidden');
            }
            currentQrData = null;
            document.getElementById('scanner-wrapper').classList.remove('hidden');
            startScanner();
        });
    }

    if (saveEditsBtn) {
        saveEditsBtn.addEventListener('click', function () {
            saveEdits();
        });
    }

    if (cancelEditBtn) {
        cancelEditBtn.addEventListener('click', function () {
            hideEditModal();
        });
    }

    window.addEventListener('click', function (event) {
        const modal = document.getElementById('edit-modal');
        if (event.target === modal) {
            hideEditModal();
        }
    });
}

function formatDateForInput(timestamp) {
    const date = new Date(timestamp);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function parseDateFromInput(dateString) {
    const date = new Date(dateString);
    return date.getTime();
}

// Функция для показа модального окна редактирования
function showEditModal(field) {
    const modal = document.getElementById('edit-modal');
    const modalTitle = document.getElementById('modal-title');
    const editField = document.getElementById('edit-field');
    const editInput = document.getElementById('edit-input');
    const editSelect = document.getElementById('edit-select');
    const editDate = document.getElementById('edit-date');

    if (!modal || !currentQrData) return;

    console.log('Открытие модального окна для поля:', field);

    editField.value = field;

    editInput.style.display = 'none';
    editSelect.style.display = 'none';
    editDate.style.display = 'none';

    if (field === 'description') {
        modalTitle.textContent = 'Редактирование описания';
        editInput.style.display = 'block';
        editInput.value = currentQrData.Description || '';
        editInput.placeholder = 'Введите описание траты';
        editInput.focus();
    }
    else if (field === 'category') {
        modalTitle.textContent = 'Выбор категории';
        editSelect.style.display = 'block';

        editSelect.innerHTML = '<option value="">Выберите категорию</option>';
        categories.forEach(category => {
            const option = document.createElement('option');
            option.value = category.id;
            option.textContent = category.name;
            if (currentQrData.CategoryId === category.id) {
                option.selected = true;
            }
            editSelect.appendChild(option);
        });
    }
    else if (field === 'date') {
        modalTitle.textContent = 'Изменение даты занесения';
        editDate.style.display = 'block';
        // Редактируем только дату занесения
        editDate.value = formatDateForInput(currentQrData.entryTime);
    }

    modal.style.display = 'flex';

    setTimeout(() => {
        modal.style.opacity = '1';
    }, 10);
}

function hideEditModal() {
    const modal = document.getElementById('edit-modal');
    modal.style.display = 'none';

    document.getElementById('edit-input').value = '';
    document.getElementById('edit-select').innerHTML = '';
    document.getElementById('edit-date').value = '';
}

function saveEdits() {
    const editField = document.getElementById('edit-field');
    const editInput = document.getElementById('edit-input');
    const editSelect = document.getElementById('edit-select');
    const editDate = document.getElementById('edit-date');

    if (!currentQrData) return;

    console.log('Сохранение поля:', editField.value);

    if (editField.value === 'description') {
        currentQrData.Description = editInput.value;
    }
    else if (editField.value === 'category') {
        const selectedOption = editSelect.options[editSelect.selectedIndex];
        currentQrData.CategoryId = editSelect.value;
        currentQrData.CategoryName = selectedOption ? selectedOption.text : '';

        // Запоминаем последнюю выбранную категорию
        if (editSelect.value) {
            localStorage.setItem('lastUsedCategory', JSON.stringify({
                id: editSelect.value,
                name: selectedOption ? selectedOption.text : ''
            }));
        }
    }
    else if (editField.value === 'date') {
        if (editDate && editDate.value) {
            // Меняем только дату занесения
            currentQrData.entryTime = parseDateFromInput(editDate.value);
            console.log('Новая дата занесения:', new Date(currentQrData.entryTime).toLocaleString());
        }
    }

    hideEditModal();
    displayScanResult(currentQrData);
}

function getElement(id) {
    const element = document.getElementById(id);
    if (!element) {
        console.warn(`Элемент с id "${id}" не найден`);
    }
    return element;
}

function setElementDisplay(id, displayValue) {
    const element = getElement(id);
    if (element) {
        element.style.display = displayValue;
    }
}

async function checkCameraAvailability() {
    const statusText = document.getElementById('camera-status-text');

    if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
        if (statusText) {
            statusText.innerHTML = '❌ Камера не поддерживается';
            statusText.style.color = '#dc3545';
        }
        return;
    }

    try {
        const devices = await navigator.mediaDevices.enumerateDevices();
        const hasCamera = devices.some(device => device.kind === 'videoinput');

        if (statusText) {
            if (hasCamera) {
                statusText.innerHTML = '✅ Камера доступна';
                statusText.style.color = '#28a745';
            } else {
                statusText.innerHTML = '❌ Камера не найдена';
                statusText.style.color = '#dc3545';
            }
        }
    } catch (error) {
        console.error('Ошибка проверки камеры:', error);
        if (statusText) {
            statusText.innerHTML = '❌ Ошибка проверки камеры';
            statusText.style.color = '#dc3545';
        }
    }
}

// ─── ЗАПОМИНАНИЕ ПОСЛЕДНЕЙ КАТЕГОРИИ ───────────────────────────────────────

/** Возвращает последнюю выбранную категорию из localStorage или null */
function getLastUsedCategory() {
    try {
        const saved = localStorage.getItem('lastUsedCategory');
        if (!saved) return null;
        const cat = JSON.parse(saved);
        // Проверяем что категория всё ещё существует в списке
        if (categories.length > 0 && !categories.find(c => c.id === cat.id)) {
            localStorage.removeItem('lastUsedCategory');
            return null;
        }
        return cat;
    } catch {
        return null;
    }
}

// Функция для парсинга фискальных чеков (формат ФНС)
function parseFiscalReceiptQR(qrText) {
    console.log('Парсинг фискального чека:', qrText);

    // Разбираем параметры
    const params = {};
    qrText.split('&').forEach(param => {
        const [key, value] = param.split('=');
        params[key] = value;
    });

    console.log('Разобранные параметры:', params);

    let purchaseTime = null;
    let sum = 0;

    // Парсим дату из параметра t (формат: 20260109T1652)
    if (params.t) {
        try {
            // Формат: YYYYMMDDTHHMM
            const dateStr = params.t;
            const year = parseInt(dateStr.substring(0, 4));
            const month = parseInt(dateStr.substring(4, 6)) - 1; // Месяцы с 0
            const day = parseInt(dateStr.substring(6, 8));
            const hours = parseInt(dateStr.substring(9, 11));
            const minutes = parseInt(dateStr.substring(11, 13));

            const date = new Date(year, month, day, hours, minutes);
            purchaseTime = date.getTime();

            console.log('Распознанная дата:', date.toLocaleString());
        } catch (e) {
            console.error('Ошибка парсинга даты:', e);
        }
    }

    // Парсим сумму из параметра s
    if (params.s) {
        sum = parseFloat(params.s);
    }

    const savedCategory = getLastUsedCategory();

    return {
        purchaseTime: purchaseTime,
        entryTime: Date.now(),
        Sum: sum,
        Description: '',
        CategoryId: savedCategory ? savedCategory.id : null,
        CategoryName: savedCategory ? savedCategory.name : null,
        categoryAutoFilled: !!savedCategory,
        AdditionalData: params
    };
}
function parseReceiptQR(qrText) {
    console.log('Парсинг QR-кода:', qrText);

    try {
        // Проверяем, является ли строка URL-параметрами (формат фискального чека)
        if (qrText.includes('&') && qrText.includes('=')) {
            return parseFiscalReceiptQR(qrText);
        }

        // Пробуем распарсить как JSON
        if (qrText.trim().startsWith('{')) {
            const jsonData = JSON.parse(qrText);

            // Пытаемся извлечь дату покупки из различных полей
            let purchaseTime = null;

            if (jsonData.timestamp) {
                purchaseTime = jsonData.timestamp.toString().length === 10
                    ? jsonData.timestamp * 1000
                    : jsonData.timestamp;
            } else if (jsonData.time) {
                purchaseTime = jsonData.time.toString().length === 10
                    ? jsonData.time * 1000
                    : jsonData.time;
            } else if (jsonData.date) {
                purchaseTime = jsonData.date.toString().length === 10
                    ? jsonData.date * 1000
                    : jsonData.date;
            }

            const savedCategory = getLastUsedCategory();
            const categoryId   = jsonData.categoryId || (savedCategory ? savedCategory.id : null);
            const categoryName = jsonData.category || jsonData.categoryName || (savedCategory ? savedCategory.name : null);

            return {
                purchaseTime: purchaseTime,
                entryTime: Date.now(),
                Sum: parseFloat(jsonData.amount || jsonData.sum || jsonData.total || jsonData.price || 0),
                Description: jsonData.description || jsonData.name || jsonData.item || jsonData.product || '',
                CategoryId: categoryId,
                CategoryName: categoryName,
                categoryAutoFilled: !jsonData.categoryId && !!savedCategory,
                AdditionalData: jsonData
            };
        }

        // Если не JSON и не URL-параметры, пробуем распарсить как обычный текст
        return parsePlainTextQR(qrText);

    } catch (error) {
        console.error('Ошибка парсинга QR-кода:', error);
        return null;
    }
}

// Функция для парсинга простого текста
function parsePlainTextQR(qrText) {
    let sum = 0;
    let purchaseTime = null;

    // Ищем сумму
    const sumMatch = qrText.match(/(\d+[.,]\d{2})/);
    if (sumMatch) {
        sum = parseFloat(sumMatch[1].replace(',', '.'));
    }

    // Ищем дату в тексте
    const dateMatch = qrText.match(/(\d{2})[.\/](\d{2})[.\/](\d{4})/);
    if (dateMatch) {
        const day = parseInt(dateMatch[1]);
        const month = parseInt(dateMatch[2]) - 1;
        const year = parseInt(dateMatch[3]);
        const date = new Date(year, month, day);
        purchaseTime = date.getTime();
    }

    // Ищем время
    const timeMatch = qrText.match(/(\d{2}):(\d{2})(?::(\d{2}))?/);
    if (timeMatch && purchaseTime) {
        const hours = parseInt(timeMatch[1]);
        const minutes = parseInt(timeMatch[2]);
        const seconds = parseInt(timeMatch[3]) || 0;

        const date = new Date(purchaseTime);
        date.setHours(hours, minutes, seconds);
        purchaseTime = date.getTime();
    }

    const savedCategory = getLastUsedCategory();

    return {
        purchaseTime: purchaseTime,
        entryTime: Date.now(),
        Sum: sum,
        Description: '',
        CategoryId: savedCategory ? savedCategory.id : null,
        CategoryName: savedCategory ? savedCategory.name : null,
        categoryAutoFilled: !!savedCategory,
        AdditionalData: { rawData: qrText }
    };
}

// Функция для отображения результата сканирования
function displayScanResult(qrData) {
    const container = getElement('result-container');
    const details = getElement('expense-details');
    const status = getElement('scan-status');

    if (!container || !details || !status) return;

    container.classList.remove('hidden');

    status.textContent = '✓ Требуется заполнение';
    status.className = 'status warning';

    // Форматируем дату покупки (из QR)
    let formattedPurchaseDate = 'Не указана';
    if (qrData.purchaseTime) {
        const purchaseDate = new Date(qrData.purchaseTime);
        formattedPurchaseDate = purchaseDate.toLocaleString('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Форматируем дату занесения (можно редактировать)
    const entryDate = new Date(qrData.entryTime);
    const formattedEntryDate = entryDate.toLocaleString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });

    // Находим название категории если есть ID
    let categoryName = qrData.CategoryName || 'Не выбрана';
    if (qrData.CategoryId && categories.length > 0) {
        const category = categories.find(c => c.id === qrData.CategoryId);
        if (category) {
            categoryName = category.name;
        }
    }

    // Определяем статус заполненности
    const hasCategory    = !!qrData.CategoryId;
    const hasDescription = !!(qrData.Description && qrData.Description.trim());
    const isAutoFilled   = qrData.categoryAutoFilled && hasCategory;

    if (hasCategory && hasDescription) {
        status.textContent = '✓ Готово к сохранению';
        status.className = 'status success';
    } else {
        status.textContent = '✓ Требуется заполнение';
        status.className = 'status warning';
    }

    // Подсказка об автоподстановке категории
    const autoHint = isAutoFilled
        ? `<div class="auto-category-hint">
               ↩ Категория из прошлого сканирования
           </div>`
        : '';

    details.innerHTML = `
        <div class="detail-row">
            <span class="detail-label">Дата покупки:</span>
            <span class="detail-value">${formattedPurchaseDate}</span>
        </div>
        <div class="detail-row editable" data-field="date" id="date-row">
            <span class="detail-label">Дата занесения:</span>
            <span class="detail-value">
                ${formattedEntryDate}
                <button class="edit-btn" id="edit-date-btn">Изменить</button>
            </span>
        </div>
        <div class="detail-row">
            <span class="detail-label">Сумма:</span>
            <span class="detail-value">${qrData.Sum.toFixed(2)} ₽</span>
        </div>
        <div class="detail-row editable" data-field="description" id="description-row">
            <span class="detail-label">Описание:</span>
            <span class="detail-value">
                ${hasDescription ? qrData.Description : '<span style="color: #999;">не указано</span>'}
                <button class="edit-btn" id="edit-description">Изменить</button>
            </span>
        </div>
        <div class="detail-row editable" data-field="category" id="category-row">
            <span class="detail-label">Категория:</span>
            <span class="detail-value">
                <span class="category-value-wrap">
                    ${hasCategory
                        ? `<span class="category-chip">${categoryName}</span>`
                        : '<span style="color: #999;">не выбрана</span>'}
                    ${autoHint}
                </span>
                <button class="edit-btn" id="edit-category">Изменить</button>
            </span>
        </div>
    `;

    document.getElementById('edit-description')?.addEventListener('click', () => showEditModal('description'));
    document.getElementById('edit-category')?.addEventListener('click', () => showEditModal('category'));
    document.getElementById('edit-date-btn')?.addEventListener('click', () => showEditModal('date'));
}

async function resetAfterSave() {
    if (html5QrCode) {
        try {
            await html5QrCode.stop();
            await html5QrCode.clear();
            html5QrCode = null;
            isScanning = false;
        } catch (error) {
            console.error('Ошибка при сбросе сканера:', error);
            html5QrCode = null;
            isScanning = false;
        }
    }

    const scannerWrapper = document.getElementById('scanner-wrapper');
    if (scannerWrapper) {
        scannerWrapper.classList.add('hidden');
    }

    console.log('Сканер полностью остановлен');
}

// Функция для сохранения расхода
async function saveExpense(qrData) {
    // Проверяем заполнены ли обязательные поля
    if (!qrData.Description || qrData.Description.trim() === '') {
        showToast('Укажите описание расхода', 'warning');
        showEditModal('description');
        return;
    }

    if (!qrData.CategoryId) {
        showToast('Выберите категорию', 'warning');
        showEditModal('category');
        return;
    }

    try {
        console.log('Сохранение расхода:', qrData);

        // Для отправки на сервер используем дату покупки
        const expenseToSave = {
            Time: qrData.purchaseTime || Date.now(),
            Sum: qrData.Sum,
            Description: qrData.Description,
            CategoryName: qrData.CategoryName
        };

        console.log('Отправляемые данные на сервер:', expenseToSave);

        const response = await fetchWithAuth(`${API_BASE_URL}/expense/scan-qr`, {
            method: 'POST',
            body: JSON.stringify(expenseToSave)
        });

        if (!response) return;

        const result = await response.json();

        if (response.ok) {
            // Находим название категории по ID
            const category = categories.find(c => c.id === qrData.CategoryId);
            const categoryName = category ? category.name : qrData.CategoryName || 'Другое';

            // Сохраняем категорию как последнюю использованную
            if (qrData.CategoryId) {
                localStorage.setItem('lastUsedCategory', JSON.stringify({
                    id: qrData.CategoryId,
                    name: categoryName
                }));
            }

            showToast('Расход успешно добавлен!', 'success');

            // ВАЖНО: сохраняем все данные, включая категорию
            const newScan = {
                purchaseTime: qrData.purchaseTime,
                entryTime: qrData.entryTime,
                Sum: qrData.Sum,
                Description: qrData.Description || 'Без описания',
                CategoryId: qrData.CategoryId,        // <-- ОБЯЗАТЕЛЬНО сохраняем ID
                CategoryName: categoryName,            // <-- ОБЯЗАТЕЛЬНО сохраняем название
                savedAt: new Date().toISOString()
            };

            console.log('Сохраняем в историю:', newScan);

            lastScannedData.unshift(newScan);

            if (lastScannedData.length > 10) {
                lastScannedData.pop();
            }

            localStorage.setItem('lastScans', JSON.stringify(lastScannedData));
            updateScansList();

            const resultContainer = getElement('result-container');
            if (resultContainer) {
                resultContainer.classList.add('hidden');
            }

            currentQrData = null;

            await resetAfterSave();

            console.log('Готов к новому сканированию');
        }
        else {
            showToast('Ошибка при сохранении: ' + (result.message || 'Неизвестная ошибка'), 'error');

            if (response.status === 401) {
                localStorage.removeItem('token');
                localStorage.removeItem('user');
                window.location.href = 'login.html';
            }
        }
    } catch (error) {
        console.error('Ошибка при сохранении:', error);
        showToast('Ошибка соединения с сервером', 'error');
    }
}

// Функция для обновления списка последних сканирований
function updateScansList() {
    const scansList = getElement('scans-list');
    if (!scansList) return;

    if (lastScannedData.length === 0) {
        scansList.innerHTML = '<div class="empty-state">Нет сохраненных сканирований</div>';
        return;
    }

    scansList.innerHTML = lastScannedData.map((scan, index) => {
        // Дата покупки (из QR)
        let formattedPurchaseDate = 'Не указана';
        if (scan.purchaseTime) {
            const purchaseDate = new Date(scan.purchaseTime);
            formattedPurchaseDate = purchaseDate.toLocaleString('ru-RU', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }

        // Дата занесения
        let formattedEntryDate = 'Не указана';
        if (scan.entryTime) {
            const entryDate = new Date(scan.entryTime);
            formattedEntryDate = entryDate.toLocaleString('ru-RU', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }

        const descId = `desc-${index}-${Date.now()}`;
        const description = scan.Description || 'Без описания';
        const needsCollapse = description.length > 50;

        // Получаем название категории
        const categoryName = scan.CategoryName || 'Другое';

        return `
            <div class="expense-item" data-scan-index="${index}">
                <div class="expense-header">
                    <div class="expense-date-info">
                        <div class="purchase-date">
                            <span class="date-label">Покупка:</span>
                            ${formattedPurchaseDate}
                        </div>
                        <div class="added-date">
                            <span class="date-label">Занесено:</span>
                            ${formattedEntryDate}
                        </div>
                    </div>
                    <span class="expense-category">${categoryName}</span>
                </div>
                <div class="scan-description">
                    <div id="${descId}" class="description-text ${needsCollapse ? 'collapsed' : ''}">
                        ${description}
                    </div>
                    ${needsCollapse ? `
                        <button class="toggle-description" data-desc-id="${descId}">
                            Показать полностью
                        </button>
                    ` : ''}
                </div>
                <div class="amount">${scan.Sum.toFixed(2)} ₽</div>
            </div>
        `;
    }).join('');

    document.querySelectorAll('.toggle-description').forEach(button => {
        button.addEventListener('click', function (e) {
            e.stopPropagation();
            const descId = this.dataset.descId;
            const descElement = document.getElementById(descId);

            if (descElement) {
                const isCollapsed = descElement.classList.contains('collapsed');

                if (isCollapsed) {
                    descElement.classList.remove('collapsed');
                    descElement.classList.add('expanded');
                    this.textContent = 'Свернуть';
                } else {
                    descElement.classList.add('collapsed');
                    descElement.classList.remove('expanded');
                    this.textContent = 'Показать полностью';
                }
            }
        });
    });
}

function loadLastScans() {
    const saved = localStorage.getItem('lastScans');
    if (saved) {
        try {
            lastScannedData = JSON.parse(saved);
            if (lastScannedData.length > 10) {
                lastScannedData = lastScannedData.slice(0, 10);
                localStorage.setItem('lastScans', JSON.stringify(lastScannedData));
            }
            updateScansList();
        } catch (error) {
            console.error('Ошибка загрузки истории:', error);
        }
    }
}

async function stopScanner() {
    if (html5QrCode && isScanning) {
        try {
            await html5QrCode.stop();
            await html5QrCode.clear();
            html5QrCode = null;
            isScanning = false;
        } catch (error) {
            console.error('Ошибка остановки сканера:', error);
            html5QrCode = null;
            isScanning = false;
        }
    }

    setElementDisplay('start-scan', 'block');
    setElementDisplay('stop-scan', 'none');

    const statusDiv = getElement('camera-status');
    if (statusDiv) {
        statusDiv.innerHTML = '<div style="color: #666;">Камера остановлена</div>';
    }

    const qrReader = getElement('qr-reader');
    if (qrReader) {
        qrReader.innerHTML =
            '<div style="text-align: center; color: #666; padding: 20px;">' +
            '<p>Нажмите "Начать сканирование" для доступа к камере</p>' +
            '</div>';
    }

    console.log('Сканирование остановлено');
}

function debugMobileMode() {
    console.log('=== ДИАГНОСТИКА МОБИЛЬНОГО РЕЖИМА ===');
    console.log('User Agent:', navigator.userAgent);
    console.log('Html5Qrcode загружен:', typeof Html5Qrcode !== 'undefined');
    console.log('MediaDevices поддерживается:', !!navigator.mediaDevices);
    console.log('EnumerateDevices поддерживается:', !!navigator.mediaDevices?.enumerateDevices);

    if (navigator.mediaDevices) {
        navigator.mediaDevices.enumerateDevices()
            .then(devices => {
                console.log('Доступные устройства:');
                devices.forEach(device => {
                    console.log(`- ${device.kind}: ${device.label || 'без названия'}`);
                });
            })
            .catch(err => console.error('Ошибка получения устройств:', err));
    }
}

async function startScanner() {
    console.log('Запуск сканера...');
    debugMobileMode();

    if (typeof Html5Qrcode === 'undefined') {
        showToast('Библиотека сканирования не загружена. Обновите страницу.', 'error', 5000);
        return;
    }

    try {
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            showToast('Ваш браузер не поддерживает доступ к камере', 'error', 5000);
            return;
        }

        if (html5QrCode) {
            try {
                await html5QrCode.stop();
                await html5QrCode.clear();
            } catch (e) {
                console.log('Ошибка при остановке предыдущего сканера:', e);
            }
            html5QrCode = null;
        }

        const qrReaderElement = getElement('qr-reader');
        if (!qrReaderElement) {
            console.error('Элемент qr-reader не найден');
            return;
        }

        qrReaderElement.innerHTML = '';

        html5QrCode = new Html5Qrcode("qr-reader");

        const config = {
            fps: 10,
            qrbox: { width: 250, height: 250 },
            rememberLastUsedCamera: true,
            showTorchButtonIfSupported: true,
            aspectRatio: 1.0
        };

        try {
            const cameras = await Html5Qrcode.getCameras();
            console.log('Доступные камеры:', cameras);

            if (cameras && cameras.length > 0) {
                const backCamera = cameras.find(camera =>
                    camera.label.toLowerCase().includes('back') ||
                    camera.label.toLowerCase().includes('environment') ||
                    camera.label.toLowerCase().includes('rear')
                );

                const cameraId = backCamera ? backCamera.id : cameras[0].id;

                await html5QrCode.start(
                    cameraId,
                    config,
                    (qrText) => {
                        console.log('QR Code найден:', qrText);
                        stopScanner();
                        const qrData = parseReceiptQR(qrText);
                        if (qrData) {
                            currentQrData = qrData;
                            displayScanResult(qrData);
                        } else {
                            showToast('Не удалось распознать данные чека', 'error');
                        }
                    },
                    (error) => { }
                ).then(() => {
                    console.log('Сканер успешно запущен');
                    isScanning = true;

                    setElementDisplay('start-scan', 'none');
                    setElementDisplay('stop-scan', 'block');

                    const statusDiv = getElement('camera-status');
                    if (statusDiv) {
                        statusDiv.innerHTML = '<div style="color: #28a745;">✅ Сканирование активно</div>';
                    }
                });
            } else {
                showToast('Камера не найдена. Разрешите доступ в настройках браузера.', 'error', 5000);
            }
        } catch (cameraError) {
            console.error('Ошибка получения камер:', cameraError);

            showToast('Не удалось получить список камер. Разрешите доступ к камере.', 'warning', 5000);

            try {
                await html5QrCode.start(
                    { facingMode: "environment" },
                    config,
                    (qrText) => {
                        console.log('QR Code найден:', qrText);
                        stopScanner();
                        const qrData = parseReceiptQR(qrText);
                        if (qrData) {
                            currentQrData = qrData;
                            displayScanResult(qrData);
                        } else {
                            showToast('Не удалось распознать данные чека', 'error');
                        }
                    },
                    (error) => { }
                );
            } catch (fallbackError) {
                console.error('Ошибка альтернативного запуска:', fallbackError);
                showToast('Не удалось запустить камеру. Проверьте разрешения.', 'error', 5000);
            }
        }
    } catch (error) {
        console.error('Ошибка запуска сканера:', error);

        let errorMessage = 'Ошибка доступа к камере';
        if (error.name === 'NotAllowedError') {
            errorMessage = 'Доступ к камере запрещён. Разрешите в настройках браузера.';
        } else if (error.name === 'NotFoundError') {
            errorMessage = 'Камера не найдена на вашем устройстве.';
        } else if (error.message) {
            errorMessage = 'Ошибка доступа к камере: ' + error.message;
        }

        showToast(errorMessage, 'error', 5000);

        const statusDiv = getElement('camera-status');
        if (statusDiv) {
            statusDiv.innerHTML = '<div class="error-message">❌ ' + errorMessage + '</div>';
        }

        html5QrCode = null;
        isScanning = false;
        setElementDisplay('start-scan', 'block');
        setElementDisplay('stop-scan', 'none');
    }
}

async function checkApiConnection() {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/category`, {
            method: 'GET'
        });

        if (response && response.ok) {
            console.log('✅ Подключение к API успешно');
        } else if (response && response.status === 401) {
            console.warn('⚠️ Требуется авторизация');
        } else {
            console.warn('⚠️ API вернул ошибку:', response?.status);
        }
    } catch (error) {
        console.error('❌ Не удалось подключиться к API. Проверьте что backend запущен на порту 5183');
    }
}

checkApiConnection();