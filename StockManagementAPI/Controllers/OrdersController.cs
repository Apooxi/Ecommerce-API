using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StockManagementAPI.Data;
using StockManagementAPI.Models;
using StockManagementAPI.DTOs;

namespace StockManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }
    
    [Authorize]
    [HttpPost("checkout")]
    public IActionResult Checkout([FromBody] CreateOrderRequest request)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdString == null)
            {
                return Unauthorized(new { Message = "Biletiniz gecersiz!"});
            }

            int currentUserId = int.Parse(userIdString);
            
            //Bos bir order olusturuyoruz
            var newOrder = new Order
            {
                UserId = currentUserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0, //Fiyati daha sonra hesaplayacagiz
                Items = new List<OrderItem>()
            };
            
            //Müşterinin gönderdiği sepetteki her bir ürünü dönecegiz
            foreach (var itemRequest in request.OrderItems)
            {
                //Urunu databaseden buluyoruz
                var product = _context.Products.Find(itemRequest.ProductId);
                
                if(product == null)
                {
                    throw new Exception($"{itemRequest.ProductId} ID'li ürün bulunamadı!");
                }

                if (product.Quantity < itemRequest.Quantity)
                {
                    throw new Exception(
                        $"{product.Name} isimli urun icin yeterli stok yok. Kalan stok {product.Quantity}");
                }
                
                //Stogu dusecegiz su an hala ramde yapiyoruz islemi veritabanina yansimadi henuz
                product.Quantity -= itemRequest.Quantity;
                
                //OrderItem olusturuyoruz

                var orderItem = new OrderItem
                {
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price
                };

                newOrder.Items.Add(orderItem);
                
                //Toplam fiyati hesapliyoruz
                newOrder.TotalAmount += (orderItem.Quantity * orderItem.UnitPrice);
            }

            _context.Orders.Add(newOrder);
            _context.SaveChanges(); //Ramdeki degisiklikleri sql'e cevirip kaydettik
            transaction.Commit(); //Eger hersey yolunda gittiyse transaction'i commit ediyoruz

            return Ok(new { Message = "Siparis basariyla olusturuldu", OrderId = newOrder.Id });
        }
        catch (Exception ex)
        {
            transaction.Rollback(); //Eger bir hata olursa transaction'i geri aliyoruz

            return BadRequest(new { Error = ex.Message });
        }
    }
}