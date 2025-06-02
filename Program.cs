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

// --- BURADAN �T�BAREN IDENTITY AYARLARI EKLEND� VE G�NCELLEND� ---
// Identity servislerini ve �ifre/kullan�c� se�eneklerini yap�land�rma
builder.Services.AddIdentity<AppUser, AppRole>(options => // AppRole kulland���n�z i�in AppRole olarak b�rak�ld�
{
    // �ifre Politikalar� (g�venli�i art�rmak i�in)
    options.Password.RequireDigit = true; // �ifrede en az bir rakam olmal�
    options.Password.RequireLowercase = true; // �ifrede en az bir k���k harf olmal�
    options.Password.RequireUppercase = true; // �ifrede en az bir b�y�k harf olmal�
    options.Password.RequireNonAlphanumeric = true; // �ifrede en az bir �zel karakter olmal�
    options.Password.RequiredLength = 6; // �ifre en az 8 karakter uzunlu�unda olmal�
    // options.Password.RequiredUniqueChars = 1; // Opsiyonel: �ifrede tekrarlayan karakterlerin minimum say�s�

    // Kullan�c� Ayarlar�
    options.User.RequireUniqueEmail = true; // Her kullan�c�n�n benzersiz bir e-posta adresi olmas� zorunludur

    // Giri� Ayarlar� (API projelerinde genellikle e-posta onay� gerekmez, ancak kontrol edilebilir)
    options.SignIn.RequireConfirmedEmail = false; // E-posta onay�n�n giri� i�in zorunlu olup olmad���n� belirler
    options.SignIn.RequireConfirmedAccount = false; // Hesap onay�n�n giri� i�in zorunlu olup olmad���n� belirler
})
.AddEntityFrameworkStores<BookContext>() // Identity'nin veritaban� deposunu belirtir
.AddDefaultTokenProviders(); // �ifre s�f�rlama, e-posta onay� vb. i�in token sa�lay�c�lar�n� ekler
// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddAutoMapper(typeof(GeneralMapping)); // GeneralMapping s�n�f�n�z�n bulundu�u assembly'yi belirtir


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
    string adminPassword = "123456aA*"; // �ifre politikalar�n�za uygun olmal�!

    // Admin rol�n�n varl���n� kontrol et ve yoksa olu�tur
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new AppRole
        {
            Name = adminRole,
            Description = "Sistem Y�neticisi"
        });
    }

    // Admin kullan�c�s�n�n varl���n� kontrol et ve yoksa olu�tur
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = "Apo2550", // Admin kullan�c�s� i�in kullan�c� ad�
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Abdullah",
            LastName = "KO�DEM�R",
            CreatedAt = DateTime.UtcNow // DateTimeKind hatas�n� �nlemek i�in UTC
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole); // Admin rol�n� kullan�c�ya ata
        }
        else
        {
            // Admin kullan�c� olu�turulamazsa hata mesajlar�n� loglamak iyi bir pratiktir
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Admin User Creation Error: {error.Description}");
            }
        }
    }
}

app.Run();
