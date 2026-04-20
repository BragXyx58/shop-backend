using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IConfiguration _configuration;

        public OrdersController(OrderDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            order.Status = "Pending";
            order.CreatedAt = DateTime.UtcNow;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await PublishOrderCreatedEventAsync(order);

            return Ok(new { message = "Замовлення створено", orderId = order.Id });
        }


        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.CreatedAt).ToListAsync();
            return Ok(orders);
        }

        [HttpGet("my/{email}")]
        public async Task<IActionResult> GetMyOrders(string email)
        {
            var orders = await _context.Orders
                .Where(o => o.UserEmail == email)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return Ok(orders);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Статус оновлено", status = order.Status });
        }

        private async Task PublishOrderCreatedEventAsync(Order order)
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var factory = new ConnectionFactory { HostName = hostName };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("order_notifications", false, false, false, null);

            var orderEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserEmail = order.UserEmail,
                TotalAmount = order.TotalAmount
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderEvent));
            await channel.BasicPublishAsync(string.Empty, "order_notifications", body);
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}