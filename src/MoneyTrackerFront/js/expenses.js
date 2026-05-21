// ─── Утилита: защита от XSS ─────────────────────────────────────────────────
function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

// Состояние приложения
let allExpenses = [];
let filteredExpenses = [];
let categories = [];
let currentPage = 1;
const itemsPerPage = 20;
let currentEditId = null;

// Инициализация
document.addEventListener('DOMContentLoaded', async function () {
    console.log('Страница всех трат загружена');

    // Отображаем имя пользователя
    const userNameElement = document.getElementById('user-name');
    if (userNameElement && user.name) {
        userNameElement.textContent = user.name;
    }

    // Загружаем категории
    await loadCategories();

    // Загружаем траты
    await loadExpenses();

    // Заполняем годы
    populateYearFilter();

    // Инициализируем обработчики
    initializeEventListeners();
});

// Функция для добавления токена в запросы
async function fetchWithAuth(url, options = {}) {
    const token = localStorage.getItem('token');

    if (!token) {
        showToast('Сессия истекла. Войдите снова.', 'error');
        window.location.href = 'login.html';
        return null;
    }

    try {
        const response = await fetch(url, {
            ...options,
            headers: {
                ...options.headers,
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (response.status === 401) {
            showToast('Сессия истекла. Войдите снова.', 'error');
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = 'login.html';
            return null;
        }

        return response;
    } catch (error) {
        console.error('Ошибка сети:', error);
        throw error;
    }
}

// Загрузка категорий
async function loadCategories() {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/category`);
        if (response && response.ok) {
            categories = await response.json();
            console.log('Загруженные категории:', categories);

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

// Загрузка трат
async function loadExpenses(startDate, endDate) {
    try {
        const expensesList = document.getElementById('expenses-list');
        if (!expensesList) return;

        expensesList.innerHTML = '<div class="loading-spinner">Загрузка...</div>';

        // Если диапазон не передан — грузим текущий год целиком по умолчанию
        if (!startDate || !endDate) {
            const now = new Date();
            startDate = new Date(now.getFullYear(), 0, 1).getTime();
            endDate = new Date(now.getFullYear(), 11, 31, 23, 59, 59, 999).getTime();
        }

        console.log(`Загрузка трат с ${startDate} по ${endDate}`);

        const response = await fetchWithAuth(
            `${API_BASE_URL}/expense?startDate=${startDate}&endDate=${endDate}`
        );

        if (!response) return;

        if (response.status === 204) {
            console.log('Нет трат за выбранный период');
            allExpenses = [];
            filteredExpenses = [];
            displayExpenses();
            updateStats();
        } else if (response.ok) {
            const data = await response.json();
            console.log('Загруженные траты:', data);
            allExpenses = Array.isArray(data) ? data : [];
            filteredExpenses = [...allExpenses];
            currentPage = 1;
            displayExpenses();
            updateStats();
        } else {
            console.error('Ошибка загрузки трат:', response.status);
            expensesList.innerHTML = '<div class="empty-state">Ошибка загрузки данных</div>';
        }
    } catch (error) {
        console.error('Ошибка загрузки трат:', error);
        const expensesList = document.getElementById('expenses-list');
        if (expensesList) {
            expensesList.innerHTML = '<div class="empty-state">Ошибка загрузки данных</div>';
        }
    }
}

// Заполнение годов
function populateYearFilter() {
    const yearSelect = document.getElementById('year-filter');
    if (!yearSelect) return;

    const currentYear = new Date().getFullYear();

    yearSelect.innerHTML = '<option value="">Все годы</option>';
    for (let year = currentYear; year >= currentYear - 5; year--) {
        const option = document.createElement('option');
        option.value = year;
        option.textContent = year;
        yearSelect.appendChild(option);
    }
}

// Применение фильтров
async function applyFilters() {
    const month = parseInt(document.getElementById('month-filter')?.value) || null;
    const year = parseInt(document.getElementById('year-filter')?.value) || null;
    const categoryId = document.getElementById('category-filter')?.value || null;

    // Вычисляем диапазон дат для запроса к API
    let startDate, endDate;
    const now = new Date();

    if (year && month) {
        // Конкретный месяц конкретного года
        startDate = new Date(year, month - 1, 1).getTime();
        endDate = new Date(year, month, 0, 23, 59, 59, 999).getTime();
    } else if (year) {
        // Весь год
        startDate = new Date(year, 0, 1).getTime();
        endDate = new Date(year, 11, 31, 23, 59, 59, 999).getTime();
    } else if (month) {
        // Указан только месяц — ищем за последние 5 лет
        startDate = new Date(now.getFullYear() - 5, 0, 1).getTime();
        endDate = new Date(now.getFullYear(), 11, 31, 23, 59, 59, 999).getTime();
    } else {
        // Ничего не выбрано — текущий год
        startDate = new Date(now.getFullYear(), 0, 1).getTime();
        endDate = new Date(now.getFullYear(), 11, 31, 23, 59, 59, 999).getTime();
    }

    // Загружаем с API нужный диапазон
    await loadExpenses(startDate, endDate);

    // Теперь фильтруем по категории и месяцу (если год не указан) на клиенте
    filteredExpenses = allExpenses.filter(expense => {
        const date = new Date(expense.time);
        const expenseMonth = date.getMonth() + 1;

        if (month && expenseMonth !== month) return false;
        if (categoryId && String(expense.categoryId) !== String(categoryId)) return false;

        return true;
    });

    // Сортируем по дате (сначала новые)
    filteredExpenses.sort((a, b) => b.time - a.time);

    currentPage = 1;
    displayExpenses();
    updateStats();
}

// Отображение трат
function displayExpenses() {
    const expensesList = document.getElementById('expenses-list');
    if (!expensesList) return;

    const start = (currentPage - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const pageExpenses = filteredExpenses.slice(start, end);

    if (filteredExpenses.length === 0) {
        expensesList.innerHTML = '<div class="empty-state">Нет трат за выбранный период</div>';
    } else {
        expensesList.innerHTML = pageExpenses.map(expense => {
            // expense.time — дата покупки (с чека)
            // expense.entryTime — дата занесения в систему (если есть)
            const purchaseDate = new Date(expense.time);
            const entryDate = expense.entryTime ? new Date(expense.entryTime) : null;

            const formatDate = (d) => d.toLocaleString('ru-RU', {
                day: '2-digit', month: '2-digit', year: 'numeric',
                hour: '2-digit', minute: '2-digit'
            }).replace(',', '');

            const purchaseDateStr = formatDate(purchaseDate);
            const entryDateStr = entryDate ? formatDate(entryDate) : null;

            // Показываем дату занесения только если она отличается от покупки (>1 мин разница)
            const showEntryDate = entryDate && Math.abs(entryDate - purchaseDate) > 60000;

            const category = categories.find(c => String(c.id) === String(expense.categoryId));
            const categoryName = escapeHtml(category ? category.name : 'Другое');
            const description = escapeHtml(expense.description || '');

            return `
        <div class="expense-item" data-id="${expense.id}">
            <div class="expense-header">
                <div class="expense-date-info">
                    <div class="purchase-date">
                        <span class="date-label">Покупка:</span> ${purchaseDateStr}
                    </div>
                    ${showEntryDate ? `
                    <div class="entry-date">
                        <span class="date-label">Занесено:</span> ${entryDateStr}
                    </div>` : ''}
                </div>
                <div class="expense-actions-right">
                    <button class="expense-action-btn edit-action" onclick="editExpense('${expense.id}')">✎ Ред.</button>
                    <button class="expense-action-btn delete-action" onclick="deleteExpense('${expense.id}')">🗑 Удал.</button>
                </div>
            </div>
            ${description ? `
            <div class="scan-description">
                <div class="description-text">${description}</div>
            </div>` : ''}
            <div class="expense-footer">
                <span class="expense-category">${categoryName}</span>
                <span class="amount">${expense.sum.toFixed(2)} ₽</span>
            </div>
        </div>
    `;
        }).join('');
    }

    updatePagination();
}

// Обновление статистики
function updateStats() {
    const totalCount = document.getElementById('total-count');
    const totalSum = document.getElementById('total-sum');

    if (totalCount) {
        totalCount.textContent = filteredExpenses.length;
    }

    if (totalSum) {
        const sum = filteredExpenses.reduce((sum, exp) => sum + exp.sum, 0);
        totalSum.textContent = sum.toFixed(2) + ' ₽';
    }
}

// Обновление пагинации
function updatePagination() {
    const totalPages = Math.ceil(filteredExpenses.length / itemsPerPage);
    const pageInfo = document.getElementById('page-info');
    const prevBtn = document.getElementById('prev-page');
    const nextBtn = document.getElementById('next-page');

    if (pageInfo) {
        pageInfo.textContent = `Страница ${currentPage} из ${totalPages || 1}`;
    }

    if (prevBtn) {
        prevBtn.disabled = currentPage === 1;
    }

    if (nextBtn) {
        nextBtn.disabled = currentPage === totalPages || totalPages === 0;
    }
}

// Редактирование траты
// Редактирование траты
function editExpense(id) {
    const expense = allExpenses.find(e => e.id === id);
    if (!expense) return;

    currentEditId = id;

    const date = new Date(expense.time);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    const editIdInput = document.getElementById('edit-expense-id');
    const editDateInput = document.getElementById('edit-expense-date');
    const editSumInput = document.getElementById('edit-expense-sum');
    const editDescriptionInput = document.getElementById('edit-expense-description');
    const editCategorySelect = document.getElementById('edit-expense-category');
    const editModal = document.getElementById('edit-expense-modal');

    if (editIdInput) editIdInput.value = id;
    if (editDateInput) editDateInput.value = `${year}-${month}-${day}T${hours}:${minutes}`;
    if (editSumInput) editSumInput.value = expense.sum;
    if (editDescriptionInput) editDescriptionInput.value = expense.description || '';
    if (editCategorySelect) editCategorySelect.value = expense.categoryId;

    if (editModal) {
        editModal.style.display = 'flex';

        // Автоматический фокус на поле суммы (самое важное)
        setTimeout(() => {
            if (editSumInput) editSumInput.focus();
        }, 300);
    }
}

// Сохранение редактирования
async function saveExpenseEdit() {
    const id = document.getElementById('edit-expense-id')?.value;
    const dateStr = document.getElementById('edit-expense-date')?.value;
    const sum = parseFloat(document.getElementById('edit-expense-sum')?.value);
    const description = document.getElementById('edit-expense-description')?.value;
    const categoryId = document.getElementById('edit-expense-category')?.value;

    if (!id || !dateStr || isNaN(sum) || sum <= 0 || !categoryId) {
        showToast('Пожалуйста, заполните все поля', 'warning');
        return;
    }

    const expenseData = {
        time: new Date(dateStr).getTime(),
        sum: sum,
        description: description,
        categoryId: categoryId
    };

    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/expense/${id}`, {
            method: 'PUT',
            body: JSON.stringify(expenseData)
        });

        if (response && response.ok) {
            showToast('Трата обновлена', 'success');
            const modal = document.getElementById('edit-expense-modal');
            if (modal) modal.style.display = 'none';
            await loadExpenses(); // Перезагружаем список
        } else {
            showToast('Ошибка при обновлении', 'error');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        showToast('Ошибка соединения', 'error');
    }
}

// Удаление траты
function deleteExpense(id) {
    currentEditId = id;
    const deleteModal = document.getElementById('delete-modal');
    if (deleteModal) deleteModal.style.display = 'flex';
}

// Подтверждение удаления
async function confirmDelete() {
    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/expense/${currentEditId}`, {
            method: 'DELETE'
        });

        if (response && response.ok) {
            showToast('Трата удалена', 'success');
            const deleteModal = document.getElementById('delete-modal');
            if (deleteModal) deleteModal.style.display = 'none';
            await loadExpenses(); // Перезагружаем список
        } else {
            showToast('Ошибка при удалении', 'error');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        showToast('Ошибка соединения', 'error');
    }
}

// Экспорт отфильтрованных трат в CSV
function exportToCsv() {
    if (filteredExpenses.length === 0) {
        showToast('Нет трат для экспорта', 'warning');
        return;
    }

    // Заголовок таблицы
    const header = ['Дата', 'Сумма (₽)', 'Категория', 'Описание'];

    // Строки с данными
    const rows = filteredExpenses.map(expense => {
        const date = new Date(expense.time).toLocaleString('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        }).replace(',', '');

        const sum = expense.sum.toFixed(2);

        const category = categories.find(c => String(c.id) === String(expense.categoryId));
        const categoryName = category ? category.name : 'Другое';

        // Оборачиваем в кавычки — защита от запятых внутри текста
        const description = `"${(expense.description || '').replace(/"/g, '""')}"`;

        return [date, sum, categoryName, description].join(',');
    });

    // BOM-маркер \uFEFF нужен чтобы Excel правильно открыл кириллицу
    const csvContent = '\uFEFF' + [header.join(','), ...rows].join('\n');

    // Создаём файл в памяти и скачиваем
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = `траты_${new Date().toLocaleDateString('ru-RU').replace(/\./g, '-')}.csv`;
    link.click();

    URL.revokeObjectURL(url); // освобождаем память

    showToast(`Экспортировано ${filteredExpenses.length} трат`, 'success');
}



// ─── Экспорт в Excel (xlsx) ───────────────────────────────────────────────────
function exportToExcel() {
    if (filteredExpenses.length === 0) {
        showToast('Нет трат для экспорта', 'warning');
        return;
    }

    const rows = filteredExpenses.map(expense => {
        const date = new Date(expense.time).toLocaleString('ru-RU', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        }).replace(',', '');

        const category = categories.find(c => String(c.id) === String(expense.categoryId));
        const categoryName = category ? category.name : 'Другое';

        return {
            'Дата': date,
            'Сумма (₽)': expense.sum,
            'Категория': categoryName,
            'Описание': expense.description || ''
        };
    });

    const ws = XLSX.utils.json_to_sheet(rows);

    // Ширина столбцов
    ws['!cols'] = [
        { wch: 18 }, // Дата
        { wch: 12 }, // Сумма
        { wch: 20 }, // Категория
        { wch: 40 }, // Описание
    ];

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Траты');

    const fileName = `траты_${new Date().toLocaleDateString('ru-RU').replace(/\./g, '-')}.xlsx`;
    XLSX.writeFile(wb, fileName);

    showToast(`Экспортировано ${filteredExpenses.length} трат`, 'success');
}

window.exportToExcel = exportToExcel;

// Инициализация обработчиков
function initializeEventListeners() {
    // Экспорт в CSV
    const exportBtn = document.getElementById('export-csv');
    if (exportBtn) {
        exportBtn.addEventListener('click', exportToCsv);
    }

    const exportExcelBtn = document.getElementById('export-excel');
    if (exportExcelBtn) {
        exportExcelBtn.addEventListener('click', exportToExcel);
    }

    // Применить фильтры
    const applyBtn = document.getElementById('apply-filters');
    if (applyBtn) {
        applyBtn.addEventListener('click', applyFilters);
    }

    // Сбросить фильтры
    const resetBtn = document.getElementById('reset-filters');
    if (resetBtn) {
        resetBtn.addEventListener('click', async function () {
            const monthFilter = document.getElementById('month-filter');
            const yearFilter = document.getElementById('year-filter');
            const categoryFilter = document.getElementById('category-filter');

            if (monthFilter) monthFilter.value = '';
            if (yearFilter) yearFilter.value = '';
            if (categoryFilter) categoryFilter.value = '';

            filteredExpenses = [...allExpenses];
            currentPage = 1;
            displayExpenses();
            updateStats();
        });
    }

    // Пагинация
    const prevBtn = document.getElementById('prev-page');
    const nextBtn = document.getElementById('next-page');

    if (prevBtn) {
        prevBtn.addEventListener('click', function () {
            if (currentPage > 1) {
                currentPage--;
                displayExpenses();
            }
        });
    }

    if (nextBtn) {
        nextBtn.addEventListener('click', function () {
            const totalPages = Math.ceil(filteredExpenses.length / itemsPerPage);
            if (currentPage < totalPages) {
                currentPage++;
                displayExpenses();
            }
        });
    }

    // Сохранить редактирование
    const saveBtn = document.getElementById('save-expense-edit');
    if (saveBtn) {
        saveBtn.addEventListener('click', saveExpenseEdit);
    }

    // Отмена редактирования
    const cancelEditBtn = document.getElementById('cancel-expense-edit');
    if (cancelEditBtn) {
        cancelEditBtn.addEventListener('click', function () {
            const modal = document.getElementById('edit-expense-modal');
            if (modal) modal.style.display = 'none';
        });
    }

    // Подтвердить удаление
    const confirmDeleteBtn = document.getElementById('confirm-delete');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', confirmDelete);
    }

    // Отмена удаления
    const cancelDeleteBtn = document.getElementById('cancel-delete');
    if (cancelDeleteBtn) {
        cancelDeleteBtn.addEventListener('click', function () {
            const modal = document.getElementById('delete-modal');
            if (modal) modal.style.display = 'none';
        });
    }

    // Закрытие модальных окон при клике вне
    window.addEventListener('click', function (event) {
        const editModal = document.getElementById('edit-expense-modal');
        const deleteModal = document.getElementById('delete-modal');

        if (event.target === editModal) {
            editModal.style.display = 'none';
        }
        if (event.target === deleteModal) {
            deleteModal.style.display = 'none';
        }
    });
}

// Делаем функции глобальными
window.editExpense = editExpense;
window.deleteExpense = deleteExpense;