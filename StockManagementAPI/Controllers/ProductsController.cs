using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManagementAPI.Data;
using StockManagementAPI.Models;


namespace StockManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAllProducts()
    {
        var products = _context.Products.ToList();
        return Ok(products);
    }

    [HttpPost]
    public IActionResult AddProduct(Product newProduct)
    {
        _context.Products.Add(newProduct);
        _context.SaveChanges();
        return Ok(newProduct);
    }
}
