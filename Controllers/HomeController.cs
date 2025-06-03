using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System;
using System.Collections.Generic; // List i�in
using System.Linq; // LINQ i�lemleri i�in (�rne�in ToList)
using KitapProject.Entities; // AppUser i�in

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

        // �nceki SaveUserToRedisCache ve GetUserFromRedisCache metotlar�n�z buraya eklendi�ini varsay�yorum.
        // ... (SaveUserToRedisCache ve GetUserFromRedisCache metotlar�) ...

        // --- REDIS'TEN T�M KULLANICILARI �EKME METODU ---
        public async Task<List<AppUser>> GetAllUsersFromRedisCache()
        {
            var users = new List<AppUser>();
            var server = _redisDb.Multiplexer.GetServer(_redisDb.IdentifyEndpoint());

            // HATA D�ZELT�LD�: 'await foreach' yerine SADECE 'foreach' kullan�ld�.
            // 'server.Keys()' metodu IAsyncEnumerable d�nd�rmedi�i i�in await foreach uygun de�il.
            foreach (var key in server.Keys(database: 0, pattern: "user:*")) // Buradaki 'database: 0' varsay�lan Redis DB'dir.
            {
                // Her anahtar i�in Redis'ten de�eri (JSON string'ini) �ek
                // Bu k�s�m zaten 'await' ile asenkron �al��maya devam eder.
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
            Console.WriteLine($"Redis'ten {users.Count} adet kullan�c� �ekildi.");
            return users;
        }

        // --- KULLANICI L�STES�N� G�R�NT�LEME EYLEM� ---
        // Bu eylem, Redis'ten t�m kullan�c�lar� �ekecek ve bir View'da listeleyecektir.
        public async Task<IActionResult> UserList()
        {
            // �nce Redis �nbelle�inden t�m kullan�c�lar� �ekmeyi dene
            List<AppUser> users = await GetAllUsersFromRedisCache();

            // E�er Redis'te hi� kullan�c� yoksa (veya azsa), veritaban�ndan �ekme mant��� da eklenebilir.
            // Ancak "t�m kullan�c�lar" senaryosu i�in �nbellek genellikle tek kaynakt�r veya
            // Redis'te olmayan kullan�c�lar� veritaban�ndan �ekip Redis'e ekleme stratejisi izlenir.
            // �imdilik sadece Redis'teki kullan�c�lar� listeliyoruz.
            if (users.Count == 0)
            {
                // E�er Redis'te kullan�c� yoksa ve veritaban�ndan �ekmek istiyorsan�z:
                // var allUsersFromDb = _userManager.Users.ToList(); // Bu senkron olabilir, async versiyonu yok.
                // E�er b�y�k bir liste ise _userManager.Users.ToListAsync() kullanmak i�in EF Core'a ihtiyac�n�z olur.
                // Veya daha iyi: T�m kullan�c�lar� veritaban�ndan �ekip Redis'e topluca kaydedebilirsiniz.
                // �rne�in:
                // var allUsersFromDb = await _userManager.Users.ToListAsync(); // EF Core'un ToListAsync'i
                // foreach(var user in allUsersFromDb)
                // {
                //     await SaveUserToRedisCache(user);
                // }
                // users = allUsersFromDb; // �imdi kullan�c� listemiz dolu
                ViewBag.Message = "Redis �nbelle�inde kay�tl� kullan�c� bulunamad�.";
            }
            else
            {
                ViewBag.Message = $"Redis �nbelle�inden {users.Count} kullan�c� y�klendi.";
            }

            // View'a g�ndermek i�in List<AppUser> nesnesini kullan�yoruz
            return View(users); // users listesini do�rudan View'a Model olarak g�nderiyoruz
        }

        // ... Di�er eylemleriniz (UserProfile, Index vb.) ...
    }
}