
namespace StockManagementAPI.DTOs;

public class CreateOrderRequest
{
    public int UserId { get; set; }
    public List<CreateOrderItemRequest> OrderItems { get; set; }
}