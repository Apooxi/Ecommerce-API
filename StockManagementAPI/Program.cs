using Microsoft.EntityFrameworkCore;
using StockManagementAPI.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var jwtSecretKey = builder.Configuration.GetSection("JwtSettings:SecretKey").Value;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });*/


builder.Services.AddAuthentication(options =>
    {
        // Sistemin her yerinde varsayılan olarak JWT kullanılacağını kesin olarak belirtiyoruz
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // "Saat farkı tolere etme, süresi dolduysa acıma" ayarı
        };

        // İŞTE MÜHENDİSLİK SİLAHI: Hata olursa Rider konsoluna bas!
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("\n=== GÜVENLİK DUVARI TOKEN'I REDDETTİ ===");
                Console.WriteLine("HATA SEBEBİ: " + context.Exception.Message);
                Console.WriteLine("==========================================\n");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("\n=== SİSTEM YETKİ (CHALLENGE) İSTEDİ ===");
                Console.WriteLine("Hata (varsa): " + context.Error);
                Console.WriteLine("Açıklama (varsa): " + context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddEndpointsApiExplorer();



builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Ticaret API", Version = "v1" });

    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Token'ınızı buraya girin.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });

    
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>() 
        }
    });
});



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 


app.Run();