let cart = {};
let totalItems = 0;
let totalSum = 0;

// Загрузка меню
async function loadMenu() {
    try {
        const response = await fetch('/Order/GetMenu');
        if (!response.ok) throw new Error('Failed to load menu');
        
        const data = await response.json();
        renderMenu(data);
        loadCartState();
    } catch (error) {
        console.error('Error loading menu:', error);
        document.getElementById('menu-content').innerHTML = `
            <div class="loading-spinner">
                <p style="color: var(--danger);">❌ Ошибка загрузки меню</p>
                <button onclick="loadMenu()" class="btn btn-primary" style="margin-top: 16px;">Попробовать снова</button>
            </div>
        `;
    }
}

// Рендер меню
function renderMenu(data) {
    const container = document.getElementById('menu-content');
    let html = '';
    
    data.forEach(category => {
        html += `
            <div class="category-section">
                <div class="category-title">${category.name}</div>
                ${category.products.map(product => `
                    <div class="product-item" data-product-id="${product.id}">
                        <div class="product-info">
                            <div class="product-name">${product.name}</div>
                            ${product.description ? `<div class="product-description">${product.description}</div>` : ''}
                            <div class="product-price">${product.price} ₽</div>
                        </div>
                        <div class="product-controls">
                            <div id="controls-${product.id}">
                                <button class="btn-add" onclick="addToCart('${product.id}')">+</button>
                            </div>
                        </div>
                    </div>
                `).join('')}
            </div>
        `;
    });
    
    container.innerHTML = html;
}

// Добавление в корзину
async function addToCart(productId) {
    try {
        const response = await fetch('/Order/AddToCart', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: 1 })
        });
        
        if (!response.ok) throw new Error('Failed to add to cart');
        
        // Обновляем локальное состояние
        if (!cart[productId]) {
            cart[productId] = 0;
        }
        cart[productId]++;
        updateCartUI(productId);
        updateCartSummary();
    } catch (error) {
        console.error('Error adding to cart:', error);
        alert('Не удалось добавить в корзину');
    }
}

// Обновление UI для конкретного товара
function updateCartUI(productId) {
    const quantity = cart[productId] || 0;
    const controls = document.getElementById(`controls-${productId}`);
    if (!controls) return;
    
    if (quantity > 0) {
        controls.innerHTML = `
            <div class="quantity-controls">
                <button class="quantity-btn minus" onclick="updateQuantity('${productId}', -1)">−</button>
                <span class="quantity-number">${quantity}</span>
                <button class="quantity-btn plus" onclick="updateQuantity('${productId}', 1)">+</button>
            </div>
        `;
    } else {
        controls.innerHTML = `
            <button class="btn-add" onclick="addToCart('${productId}')">+</button>
        `;
    }
}

// Обновление количества
async function updateQuantity(productId, delta) {
    const newQuantity = (cart[productId] || 0) + delta;
    
    if (newQuantity < 0) return;
    
    try {
        const response = await fetch('/Order/UpdateCartItem', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: newQuantity })
        });
        
        if (!response.ok) throw new Error('Failed to update cart');
        
        if (newQuantity === 0) {
            delete cart[productId];
        } else {
            cart[productId] = newQuantity;
        }
        updateCartUI(productId);
        updateCartSummary();
    } catch (error) {
        console.error('Error updating cart:', error);
        alert('Не удалось обновить корзину');
    }
}

// Обновление сводки по корзине
function updateCartSummary() {
    const items = document.getElementById('cart-total-items');
    const sum = document.getElementById('cart-total-sum');
    const checkoutBtn = document.getElementById('checkout-btn');
    
    let total = 0;
    let count = 0;
    
    // Получаем цены из DOM
    document.querySelectorAll('.product-item').forEach(item => {
        const id = item.dataset.productId;
        const priceText = item.querySelector('.product-price').textContent;
        const price = parseFloat(priceText.replace(' ₽', ''));
        const qty = cart[id] || 0;
        total += price * qty;
        count += qty;
    });
    
    totalItems = count;
    totalSum = total;
    
    items.textContent = count;
    sum.textContent = total.toFixed(0) + ' ₽';
    checkoutBtn.disabled = count === 0;
}

// Загрузка состояния корзины с сервера
async function loadCartState() {
    try {
        const response = await fetch('/Order/GetCartState');
        if (!response.ok) throw new Error('Failed to load cart state');
        
        const data = await response.json();
        data.forEach(item => {
            cart[item.productId] = item.quantity;
            updateCartUI(item.productId);
        });
        updateCartSummary();
    } catch (error) {
        console.error('Error loading cart state:', error);
    }
}

// Переход к оформлению
function goToCheckout() {
    if (totalItems > 0) {
        window.location.href = '/Order/Cart';
    }
}

// Загружаем меню при загрузке страницы
document.addEventListener('DOMContentLoaded', loadMenu);
