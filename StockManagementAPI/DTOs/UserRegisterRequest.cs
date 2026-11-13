namespace StockManagementAPI.DTOs;

public class UserRegisterRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}