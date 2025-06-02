using FluentValidation;
using KitapProject.Context;
using KitapProject.Entities;
using KitapProject.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection");

// DbContext ve Identity servislerini ekle
builder.Services.AddDbContext<BookContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// --- BURADAN ÝTÝBAREN IDENTITY AYARLARI EKLENDÝ VE GÜNCELLENDÝ ---
// Identity servislerini ve þifre/kullanýcý seçeneklerini yapýlandýrma
builder.Services.AddIdentity<AppUser, AppRole>(options => // AppRole kullandýðýnýz için AppRole olarak býrakýldý
{
    // Þifre Politikalarý (güvenliði artýrmak için)
    options.Password.RequireDigit = true; // Þifrede en az bir rakam olmalý
    options.Password.RequireLowercase = true; // Þifrede en az bir küçük harf olmalý
    options.Password.RequireUppercase = true; // Þifrede en az bir büyük harf olmalý
    options.Password.RequireNonAlphanumeric = true; // Þifrede en az bir özel karakter olmalý
    options.Password.RequiredLength = 6; // Þifre en az 8 karakter uzunluðunda olmalý
    // options.Password.RequiredUniqueChars = 1; // Opsiyonel: Þifrede tekrarlayan karakterlerin minimum sayýsý

    // Kullanýcý Ayarlarý
    options.User.RequireUniqueEmail = true; // Her kullanýcýnýn benzersiz bir e-posta adresi olmasý zorunludur

    // Giriþ Ayarlarý (API projelerinde genellikle e-posta onayý gerekmez, ancak kontrol edilebilir)
    options.SignIn.RequireConfirmedEmail = false; // E-posta onayýnýn giriþ için zorunlu olup olmadýðýný belirler
    options.SignIn.RequireConfirmedAccount = false; // Hesap onayýnýn giriþ için zorunlu olup olmadýðýný belirler
})
.AddEntityFrameworkStores<BookContext>() // Identity'nin veritabaný deposunu belirtir
.AddDefaultTokenProviders(); // Þifre sýfýrlama, e-posta onayý vb. için token saðlayýcýlarýný ekler
// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddAutoMapper(typeof(GeneralMapping)); // GeneralMapping sýnýfýnýzýn bulunduðu assembly'yi belirtir


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    string adminRole = "Admin";
    string adminEmail = "kcdmirapo96@gmail.com";
    string adminPassword = "123456aA*"; // Þifre politikalarýnýza uygun olmalý!

    // Admin rolünün varlýðýný kontrol et ve yoksa oluþtur
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new AppRole
        {
            Name = adminRole,
            Description = "Sistem Yöneticisi"
        });
    }

    // Admin kullanýcýsýnýn varlýðýný kontrol et ve yoksa oluþtur
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = "Apo2550", // Admin kullanýcýsý için kullanýcý adý
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Abdullah",
            LastName = "KOÇDEMÝR",
            CreatedAt = DateTime.UtcNow // DateTimeKind hatasýný önlemek için UTC
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole); // Admin rolünü kullanýcýya ata
        }
        else
        {
            // Admin kullanýcý oluþturulamazsa hata mesajlarýný loglamak iyi bir pratiktir
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Admin User Creation Error: {error.Description}");
            }
        }
    }
}

app.Run();
