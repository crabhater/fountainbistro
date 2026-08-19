using Microsoft.AspNetCore.Mvc;
using FountainBistro.Web.Services.Abstractions;
using FountainBistro.Web.Models.Enums;

namespace FountainBistro.Web.Controllers;

public class OrderController : Controller
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IProductService productService,
        ICartService cartService,
        IOrderService orderService,
        ILogger<OrderController> logger)
    {
        _productService = productService;
        _cartService = cartService;
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Menu()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMenu()
    {
        var menu = await _productService.GetMenuByCategoriesAsync();
        return Json(menu);
    }

    [HttpGet]
    public IActionResult Cart()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetCartState()
    {
        if (!HttpContext.Items.ContainsKey("UserId") || HttpContext.Items["UserId"] == null)
        {
            return Json(Array.Empty<object>());
        }

        var userId = (Guid)HttpContext.Items["UserId"]!;
        var cart = _cartService.GetCart(userId);
        
        if (cart == null)
        {
            return Json(Array.Empty<object>());
        }

        var items = cart.Items.Select(i => new { i.ProductId, i.Quantity });
        return Json(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (!HttpContext.Items.ContainsKey("UserId") || HttpContext.Items["UserId"] == null)
        {
            return Unauthorized();
        }

        var userId = (Guid)HttpContext.Items["UserId"]!;
        
        if (!await _productService.IsProductAvailableAsync(request.ProductId))
        {
            return BadRequest("Товар недоступен");
        }

        _cartService.AddItem(userId, request.ProductId, request.Quantity);
        return Ok();
    }

    [HttpPost]
    public IActionResult UpdateCartItem([FromBody] UpdateCartRequest request)
    {
        if (!HttpContext.Items.ContainsKey("UserId") || HttpContext.Items["UserId"] == null)
        {
            return Unauthorized();
        }

        var userId = (Guid)HttpContext.Items["UserId"]!;
        _cartService.UpdateQuantity(userId, request.ProductId, request.Quantity);
        return Ok();
    }

    [HttpPost]
    public IActionResult ClearCart()
    {
        if (!HttpContext.Items.ContainsKey("UserId") || HttpContext.Items["UserId"] == null)
        {
            return Unauthorized();
        }

        var userId = (Guid)HttpContext.Items["UserId"]!;
        _cartService.ClearCart(userId);
        return Ok();
    }

    [HttpGet]
    public IActionResult CartSummary()
    {
        if (!HttpContext.Items.ContainsKey("UserId") || HttpContext.Items["UserId"] == null)
        {
            return Json(new { totalItems = 0, totalSum = 0 });
        }

        var userId = (Guid)HttpContext.Items["UserId"]!;
        var cart = _cartService.GetCart(userId);
        
        if (cart == null)
        {
            return Json(new { totalItems = 0, totalSum = 0 });
        }

        return Json(new { totalItems = cart.TotalItems, totalSum = cart.TotalSum });
    }

    [HttpGet]
    public async Task<IActionResult> CurrentOrder()
    {
        if (!HttpContext.Items.ContainsKey("UserId") || HttpContext.Items["UserId"] == null)
        {
            return Json(null);
        }

        var userId = (Guid)HttpContext.Items["UserId"]!;
        var order = await _orderService.GetCurrentOrderAsync(userId);
        
        if (order == null)
        {
            return Json(null);
        }

        return Json(new
        {
            id = order.Id,
            status = order.Status.ToString(),
            totalAmount = order.TotalAmount,
            createdAt = order.CreatedAt,
            items = order.Items.Select(i => new
            {
                name = i.Product?.Name ?? "Товар",
                quantity = i.Quantity,
                price = i.UnitPrice,
                total = i.UnitPrice * i.Quantity
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        if (!HttpContext.Items.ContainsKey("UserId") || HttpContext.Items["UserId"] == null)
        {
            return Unauthorized();
        }

        var userId = (Guid)HttpContext.Items["UserId"]!;
        var cart = _cartService.GetCart(userId);
        
        if (cart == null || cart.Items.Count == 0)
        {
            return BadRequest("Корзина пуста");
        }

        try
        {
            // Создаем заказ
            var order = await _orderService.CreateOrderAsync(userId, cart);
            
            // Очищаем корзину
            _cartService.ClearCart(userId);
            
            // Фоновый сервис сам найдет заказ в БД
            _logger.LogInformation("Order {OrderId} created, background service will process it", order.Id);
            
            // Возвращаем JSON с redirect URL
            return Json(new { 
                success = true, 
                orderId = order.Id,
                redirectUrl = Url.Action(nameof(PaymentStatus), new { orderId = order.Id })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Ошибка при создании заказа");
        }
    }

    [HttpGet]
    public IActionResult Payment()
    {
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> PaymentStatus(Guid orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderStatus(Guid orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        return Json(new
        {
            id = order.Id,
            status = order.Status.ToString(),
            statusDisplay = GetStatusDisplay(order.Status),
            totalAmount = order.TotalAmount,
            createdAt = order.CreatedAt,
            items = order.Items.Select(i => new
            {
                name = i.Product?.Name ?? "Товар",
                quantity = i.Quantity,
                price = i.UnitPrice,
                total = i.UnitPrice * i.Quantity
            })
        });
    }

    [HttpPost]
    public IActionResult Webhook()
    {
        return Ok();
    }

    private string GetStatusDisplay(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.New => "Новый заказ",
            OrderStatus.Paid => "Оплачен",
            OrderStatus.InProgress => "Готовится",
            OrderStatus.Ready => "Готов к выдаче",
            OrderStatus.Completed => "Выполнен",
            OrderStatus.Cancelled => "Отменен",
            _ => status.ToString()
        };
    }
}

public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
