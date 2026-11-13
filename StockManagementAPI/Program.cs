using Microsoft.EntityFrameworkCore;
using StockManagementAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller'ları sisteme ekliyoruz (Garsonlar)
builder.Services.AddControllers();

// 2. Swagger'ı (Test arayüzünü) sisteme ekliyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Seninle yazdığımız PostgreSQL veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 4. Swagger'ı kullanıma açıyoruz (Geliştirici ortamındaysak)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers(); // Gelen istekleri bizim ProductsController'a yönlendirir

// 5. VE EN ÖNEMLİSİ: Sunucuyu başlat ve bekle! (Bu olmazsa uygulama kapanır)
app.Run();