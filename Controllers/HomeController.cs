using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System;
using System.Collections.Generic; // List için
using System.Linq; // LINQ iþlemleri için (örneðin ToList)
using KitapProject.Entities; // AppUser için

namespace KitapProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDatabase _redisDb;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(IConnectionMultiplexer redis, UserManager<AppUser> userManager)
        {
            _redisDb = redis.GetDatabase();
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        // Önceki SaveUserToRedisCache ve GetUserFromRedisCache metotlarýnýz buraya eklendiðini varsayýyorum.
        // ... (SaveUserToRedisCache ve GetUserFromRedisCache metotlarý) ...

        // --- REDIS'TEN TÜM KULLANICILARI ÇEKME METODU ---
        public async Task<List<AppUser>> GetAllUsersFromRedisCache()
        {
            var users = new List<AppUser>();
            var server = _redisDb.Multiplexer.GetServer(_redisDb.IdentifyEndpoint());

            // HATA DÜZELTÝLDÝ: 'await foreach' yerine SADECE 'foreach' kullanýldý.
            // 'server.Keys()' metodu IAsyncEnumerable döndürmediði için await foreach uygun deðil.
            foreach (var key in server.Keys(database: 0, pattern: "user:*")) // Buradaki 'database: 0' varsayýlan Redis DB'dir.
            {
                // Her anahtar için Redis'ten deðeri (JSON string'ini) çek
                // Bu kýsým zaten 'await' ile asenkron çalýþmaya devam eder.
                string? userJson = await _redisDb.StringGetAsync(key);

                if (!string.IsNullOrEmpty(userJson))
                {
                    AppUser? user = JsonSerializer.Deserialize<AppUser>(userJson);
                    if (user != null)
                    {
                        users.Add(user);
                    }
                }
            }
            Console.WriteLine($"Redis'ten {users.Count} adet kullanýcý çekildi.");
            return users;
        }

        // --- KULLANICI LÝSTESÝNÝ GÖRÜNTÜLEME EYLEMÝ ---
        // Bu eylem, Redis'ten tüm kullanýcýlarý çekecek ve bir View'da listeleyecektir.
        public async Task<IActionResult> UserList()
        {
            // Önce Redis önbelleðinden tüm kullanýcýlarý çekmeyi dene
            List<AppUser> users = await GetAllUsersFromRedisCache();

            // Eðer Redis'te hiç kullanýcý yoksa (veya azsa), veritabanýndan çekme mantýðý da eklenebilir.
            // Ancak "tüm kullanýcýlar" senaryosu için önbellek genellikle tek kaynaktýr veya
            // Redis'te olmayan kullanýcýlarý veritabanýndan çekip Redis'e ekleme stratejisi izlenir.
            // Þimdilik sadece Redis'teki kullanýcýlarý listeliyoruz.
            if (users.Count == 0)
            {
                // Eðer Redis'te kullanýcý yoksa ve veritabanýndan çekmek istiyorsanýz:
                // var allUsersFromDb = _userManager.Users.ToList(); // Bu senkron olabilir, async versiyonu yok.
                // Eðer büyük bir liste ise _userManager.Users.ToListAsync() kullanmak için EF Core'a ihtiyacýnýz olur.
                // Veya daha iyi: Tüm kullanýcýlarý veritabanýndan çekip Redis'e topluca kaydedebilirsiniz.
                // Örneðin:
                // var allUsersFromDb = await _userManager.Users.ToListAsync(); // EF Core'un ToListAsync'i
                // foreach(var user in allUsersFromDb)
                // {
                //     await SaveUserToRedisCache(user);
                // }
                // users = allUsersFromDb; // Þimdi kullanýcý listemiz dolu
                ViewBag.Message = "Redis önbelleðinde kayýtlý kullanýcý bulunamadý.";
            }
            else
            {
                ViewBag.Message = $"Redis önbelleðinden {users.Count} kullanýcý yüklendi.";
            }

            // View'a göndermek için List<AppUser> nesnesini kullanýyoruz
            return View(users); // users listesini doðrudan View'a Model olarak gönderiyoruz
        }

        // ... Diðer eylemleriniz (UserProfile, Index vb.) ...
    }
}