using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System;
using System.Collections.Generic; 
using System.Linq; 
using KitapProject.Entities; 

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
        public async Task<List<AppUser>> GetAllUsersFromRedisCache()
        {
            var users = new List<AppUser>();
            var server = _redisDb.Multiplexer.GetServer(_redisDb.IdentifyEndpoint()!);

            foreach (var key in server.Keys(database: 0, pattern: "user:*")) 
            {
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
        public async Task<IActionResult> UserList()
        {
            List<AppUser> users = await GetAllUsersFromRedisCache();
            if (users.Count == 0)
            {
                ViewBag.Message = "Redis önbelleðinde kayýtlý kullanýcý bulunamadý.";
            }
            else
            {
                ViewBag.Message = $"Redis önbelleðinden {users.Count} kullanýcý yüklendi.";
            }
            return View(users);
        }
    }
}