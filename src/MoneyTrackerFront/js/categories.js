// Конфигурация API
const API_BASE_URL = 'http://localhost:5183/api';

// Состояние приложения
let categories = [];
let currentEditId = null;

// Проверка авторизации при загрузке
document.addEventListener('DOMContentLoaded', function () {
    console.log('Страница категорий загружена');

    // Проверяем наличие токена
    const token = localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    if (!token) {
        window.location.href = 'login.html';
        return;
    }

    // Отображаем имя пользователя
    const userNameElement = document.getElementById('user-name');
    if (userNameElement && user.name) {
        userNameElement.textContent = user.name;
    }

    // Загружаем категории
    loadCategories();

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
        const categoriesList = document.getElementById('categories-list');
        if (!categoriesList) return;

        categoriesList.innerHTML = '<div class="loading-spinner">Загрузка...</div>';

        const response = await fetchWithAuth(`${API_BASE_URL}/category`);

        if (!response) return;

        if (response.ok) {
            categories = await response.json();
            console.log('Загруженные категории:', categories);
            displayCategories();
        } else {
            categoriesList.innerHTML = '<div class="empty-categories">Ошибка загрузки</div>';
        }
    } catch (error) {
        console.error('Ошибка загрузки категорий:', error);
        const categoriesList = document.getElementById('categories-list');
        if (categoriesList) {
            categoriesList.innerHTML = '<div class="empty-categories">Ошибка загрузки</div>';
        }
    }
}

// Отображение категорий
// Отображение категорий
function displayCategories() {
    const categoriesList = document.getElementById('categories-list');
    if (!categoriesList) return;

    if (!categories || categories.length === 0) {
        categoriesList.innerHTML = '<div class="empty-categories">Нет категорий. Создайте первую!</div>';
        return;
    }

    categoriesList.innerHTML = categories.map(category => `
        <div class="category-item" data-id="${category.id}">
            <div class="category-info">
                <div class="category-name">${category.name}</div>
            </div>
            <div class="category-actions">
                <button class="category-link" onclick="window.editCategory('${category.id}')">Редактировать</button>
                <button class="category-link delete" onclick="window.deleteCategory('${category.id}')">Удалить</button>
            </div>
        </div>
    `).join('');
}

// Добавление категории
async function addCategory() {
    const nameInput = document.getElementById('new-category-name');
    if (!nameInput) return;

    const name = nameInput.value.trim();

    if (!name) {
        showToast('Введите название категории', 'warning');
        return;
    }

    const addBtn = document.getElementById('add-category');
    if (!addBtn) return;

    const originalText = addBtn.textContent;
    addBtn.textContent = '...';
    addBtn.disabled = true;

    try {
        const categoryData = {
            name: name
        };

        const response = await fetchWithAuth(`${API_BASE_URL}/category`, {
            method: 'POST',
            body: JSON.stringify(categoryData)
        });

        if (!response) return;

        if (response.ok) {
            showToast('Категория добавлена', 'success');
            nameInput.value = '';
            await loadCategories();
        } else {
            try {
                const errorData = await response.json();
                showToast(errorData.message || 'Ошибка при добавлении категории', 'error');
            } catch (e) {
                showToast(`Ошибка ${response.status}: ${response.statusText}`, 'error');
            }
        }
    } catch (error) {
        console.error('Ошибка:', error);
        showToast('Ошибка соединения с сервером', 'error');
    } finally {
        addBtn.textContent = originalText;
        addBtn.disabled = false;
    }
}

// Редактирование категории
function editCategory(id) {
    const category = categories.find(c => c.id === id);
    if (!category) return;

    currentEditId = id;

    const editIdInput = document.getElementById('edit-category-id');
    const editNameInput = document.getElementById('edit-category-name');
    const editModal = document.getElementById('edit-category-modal');

    if (editIdInput && editNameInput && editModal) {
        editIdInput.value = id;
        editNameInput.value = category.name;
        editModal.style.display = 'flex';

        // Автоматический фокус на поле ввода
        setTimeout(() => {
            editNameInput.focus();
        }, 300);
    }
}

// Сохранение редактирования
async function saveCategoryEdit() {
    const id = document.getElementById('edit-category-id')?.value;
    const name = document.getElementById('edit-category-name')?.value.trim();

    if (!id || !name) {
        showToast('Введите название категории', 'warning');
        return;
    }

    const saveBtn = document.getElementById('save-category-edit');
    if (!saveBtn) return;

    const originalText = saveBtn.textContent;
    saveBtn.textContent = '...';
    saveBtn.disabled = true;

    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/category/${id}`, {
            method: 'PUT',
            body: JSON.stringify({ name: name })
        });

        if (!response) return;

        if (response.ok) {
            showToast('Категория обновлена', 'success');
            const modal = document.getElementById('edit-category-modal');
            if (modal) modal.style.display = 'none';
            await loadCategories();
        } else if (response.status === 409) {
            const error = await response.json();
            showToast(error.message || 'Такая категория уже существует', 'warning');
        } else {
            showToast('Ошибка при обновлении категории', 'error');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        showToast('Ошибка соединения', 'error');
    } finally {
        saveBtn.textContent = originalText;
        saveBtn.disabled = false;
    }
}

// Удаление категории
function deleteCategory(id) {
    const category = categories.find(c => c.id === id);
    if (!category) return;

    currentEditId = id;

    const messageElement = document.getElementById('delete-category-message');
    const deleteModal = document.getElementById('delete-category-modal');

    if (messageElement && deleteModal) {
        messageElement.textContent = `Удалить категорию "${category.name}"?`;
        deleteModal.style.display = 'flex';
    }
}

// Подтверждение удаления
async function confirmCategoryDelete() {
    const deleteBtn = document.getElementById('confirm-category-delete');
    if (!deleteBtn) return;

    const originalText = deleteBtn.textContent;
    deleteBtn.textContent = '...';
    deleteBtn.disabled = true;

    try {
        const response = await fetchWithAuth(`${API_BASE_URL}/category/${currentEditId}`, {
            method: 'DELETE'
        });

        if (!response) return;

        if (response.ok) {
            showToast('Категория удалена', 'success');
            const modal = document.getElementById('delete-category-modal');
            if (modal) modal.style.display = 'none';
            await loadCategories();
        } else if (response.status === 400) {
            const error = await response.json();
            showToast(error.message || 'Нельзя удалить категорию, в которой есть траты', 'error');
        } else {
            showToast('Ошибка при удалении категории', 'error');
        }
    } catch (error) {
        console.error('Ошибка:', error);
        showToast('Ошибка соединения', 'error');
    } finally {
        deleteBtn.textContent = originalText;
        deleteBtn.disabled = false;
    }
}

// Инициализация обработчиков
function initializeEventListeners() {
    const addBtn = document.getElementById('add-category');
    if (addBtn) {
        addBtn.addEventListener('click', addCategory);
    }

    const nameInput = document.getElementById('new-category-name');
    if (nameInput) {
        nameInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                addCategory();
            }
        });
    }

    const saveBtn = document.getElementById('save-category-edit');
    if (saveBtn) {
        saveBtn.addEventListener('click', saveCategoryEdit);
    }

    const cancelEditBtn = document.getElementById('cancel-category-edit');
    if (cancelEditBtn) {
        cancelEditBtn.addEventListener('click', function () {
            const modal = document.getElementById('edit-category-modal');
            if (modal) modal.style.display = 'none';
        });
    }

    const confirmDeleteBtn = document.getElementById('confirm-category-delete');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', confirmCategoryDelete);
    }

    const cancelDeleteBtn = document.getElementById('cancel-category-delete');
    if (cancelDeleteBtn) {
        cancelDeleteBtn.addEventListener('click', function () {
            const modal = document.getElementById('delete-category-modal');
            if (modal) modal.style.display = 'none';
        });
    }

    // Закрытие модальных окон при клике вне
    window.addEventListener('click', function (event) {
        const editModal = document.getElementById('edit-category-modal');
        const deleteModal = document.getElementById('delete-category-modal');

        if (event.target === editModal) {
            editModal.style.display = 'none';
        }
        if (event.target === deleteModal) {
            deleteModal.style.display = 'none';
        }
    });
}

// Делаем функции глобальными
window.editCategory = editCategory;
window.deleteCategory = deleteCategory;