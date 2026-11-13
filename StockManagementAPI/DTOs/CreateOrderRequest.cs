
namespace StockManagementAPI.DTOs;

public class CreateOrderRequest
{
    public List<CreateOrderItemRequest> OrderItems { get; set; }
}