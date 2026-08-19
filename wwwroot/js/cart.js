let cartItems = [];
let totalSum = 0;

// Загрузка корзины
async function loadCart() {
    try {
        const response = await fetch('/Order/GetCartState');
        if (!response.ok) throw new Error('Failed to load cart');
        
        const data = await response.json();
        if (data.length === 0) {
            showEmptyCart();
            return;
        }
        
        // Получаем детальную информацию о товарах
        const menuResponse = await fetch('/Order/GetMenu');
        const menu = await menuResponse.json();
        
        // Собираем корзину
        cartItems = [];
        data.forEach(cartItem => {
            let product = null;
            menu.forEach(category => {
                const found = category.products.find(p => p.id === cartItem.productId);
                if (found) product = found;
            });
            
            if (product) {
                cartItems.push({
                    ...cartItem,
                    name: product.name,
                    price: product.price,
                    total: product.price * cartItem.quantity
                });
            }
        });
        
        renderCart();
    } catch (error) {
        console.error('Error loading cart:', error);
    }
}

// Показать пустую корзину
function showEmptyCart() {
    document.getElementById('cart-items').innerHTML = `
        <div class="empty-cart">
            <div class="empty-icon">🛒</div>
            <p>Корзина пуста</p>
            <a href="/Order/Menu" class="btn btn-primary">В меню</a>
        </div>
    `;
    document.getElementById('cart-footer').style.display = 'none';
}

// Рендер корзины
function renderCart() {
    const container = document.getElementById('cart-items');
    let html = '';
    totalSum = 0;
    
    cartItems.forEach(item => {
        const itemTotal = item.price * item.quantity;
        totalSum += itemTotal;
        
        html += `
            <div class="cart-item" data-product-id="${item.productId}">
                <div class="cart-item-info">
                    <div class="cart-item-name">${item.name}</div>
                    <div class="cart-item-price">${item.price} ₽ × ${item.quantity}</div>
                </div>
                <div class="cart-item-controls">
                    <div class="cart-item-total">${itemTotal.toFixed(0)} ₽</div>
                    <div class="quantity-controls" style="display: flex; align-items: center; gap: 8px; background: var(--light-gray); border-radius: 20px; padding: 4px;">
                        <button class="quantity-btn minus" onclick="updateQuantity('${item.productId}', -1)">−</button>
                        <span class="quantity-number">${item.quantity}</span>
                        <button class="quantity-btn plus" onclick="updateQuantity('${item.productId}', 1)">+</button>
                    </div>
                    <button class="btn btn-danger btn-sm" onclick="removeItem('${item.productId}')" style="padding: 4px 8px; font-size: 1rem;">×</button>
                </div>
            </div>
        `;
    });
    
    container.innerHTML = html;
    
    // Показываем футер
    document.getElementById('cart-footer').style.display = 'block';
    document.getElementById('cart-total').textContent = totalSum.toFixed(0) + ' ₽';
}

// Обновление количества
async function updateQuantity(productId, delta) {
    const item = cartItems.find(i => i.productId === productId);
    if (!item) return;
    
    const newQuantity = item.quantity + delta;
    if (newQuantity < 0) return;
    
    try {
        const response = await fetch('/Order/UpdateCartItem', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: productId, quantity: newQuantity })
        });
        
        if (!response.ok) throw new Error('Failed to update cart');
        
        if (newQuantity === 0) {
            cartItems = cartItems.filter(i => i.productId !== productId);
        } else {
            item.quantity = newQuantity;
            item.total = item.price * newQuantity;
        }
        
        if (cartItems.length === 0) {
            showEmptyCart();
        } else {
            renderCart();
        }
    } catch (error) {
        console.error('Error updating quantity:', error);
        alert('Не удалось обновить корзину');
    }
}

// Удаление товара
async function removeItem(productId) {
    await updateQuantity(productId, -cartItems.find(i => i.productId === productId)?.quantity || 0);
}

// Очистка корзины
async function clearCart() {
    if (!confirm('Очистить корзину?')) return;
    
    try {
        const response = await fetch('/Order/ClearCart', {
            method: 'POST'
        });
        
        if (!response.ok) throw new Error('Failed to clear cart');
        
        cartItems = [];
        showEmptyCart();
    } catch (error) {
        console.error('Error clearing cart:', error);
        alert('Не удалось очистить корзину');
    }
}

// Оформление заказа
async function checkout() {
    if (cartItems.length === 0) return;
    
    const btn = event?.target || document.querySelector('.btn-success');
    const originalText = btn.textContent;
    btn.textContent = '⏳ Оформление...';
    btn.disabled = true;
    
    try {
        const response = await fetch('/Order/Checkout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        
        if (!response.ok) {
            const error = await response.text();
            throw new Error(error || 'Failed to checkout');
        }
        
        const data = await response.json();
        
        if (data.success && data.redirectUrl) {
            // Перенаправляем на страницу статуса
            window.location.href = data.redirectUrl;
        } else {
            throw new Error('Invalid response from server');
        }
    } catch (error) {
        console.error('Error during checkout:', error);
        alert('Не удалось оформить заказ: ' + error.message);
        btn.textContent = originalText;
        btn.disabled = false;
    }
}

// Загружаем корзину при загрузке страницы
document.addEventListener('DOMContentLoaded', loadCart);
