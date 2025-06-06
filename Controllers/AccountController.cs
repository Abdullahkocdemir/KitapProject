using KitapProject.Entities; 
using KitapProject.Models; 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis; 
using System.Security.Claims;
using System.Text.Json; 

namespace KitapProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IDatabase _redisDb; 

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConnectionMultiplexer redis)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _redisDb = redis.GetDatabase();
        }
        private async Task SaveUserToRedisCache(AppUser user)
        {
            if (user == null) return;
            string redisKey = $"user:{user.Id}";
            string userJson = JsonSerializer.Serialize(user);
            await _redisDb.StringSetAsync(redisKey, userJson, TimeSpan.FromHours(1)); 
            Console.WriteLine($"Redis'e kaydedildi: Anahtar='{redisKey}', Kullanıcı='{user.UserName}'");
        }
        private async Task<AppUser?> GetUserFromRedisCache(string userId)
        {
            string redisKey = $"user:{userId}";
            string? userJson = await _redisDb.StringGetAsync(redisKey);

            if (string.IsNullOrEmpty(userJson))
            {
                Console.WriteLine($"Redis'te '{redisKey}' anahtarıyla kullanıcı bulunamadı.");
                return null;
            }
            else
            {
                AppUser? user = JsonSerializer.Deserialize<AppUser>(userJson);
                Console.WriteLine($"Redis'ten çekildi: Anahtar='{redisKey}', Kullanıcı='{user?.UserName}'");
                return user;
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await SaveUserToRedisCache(user);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName), 
            };

                    if (!string.IsNullOrEmpty(user.FirstName))
                    {
                        claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
                    }
                    if (!string.IsNullOrEmpty(user.LastName))
                    {
                        claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
                    }
                    if (!string.IsNullOrEmpty(user.FullName))
                    {
                        claims.Add(new Claim("FullName", user.FullName));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) 
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email); 
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.Email); 
                }

                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        var claims = new List<Claim>();

                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName!)); 

                        if (!string.IsNullOrEmpty(user.FirstName))
                        {
                            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName)); 
                        }
                        if (!string.IsNullOrEmpty(user.LastName))
                        {
                            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));   
                        }
                        if (!string.IsNullOrEmpty(user.FullName))
                        {
                            claims.Add(new Claim("FullName", user.FullName));
                        }

                        var userClaims = await _userManager.GetClaimsAsync(user);
                        claims.AddRange(userClaims);

                        var userRoles = await _userManager.GetRolesAsync(user);
                        foreach (var role in userRoles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);
                        return RedirectToAction("Index", "Default");
                    }
                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Hesabınız kilitlendi.");
                        return View(model);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction("LoginWith2fa", new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });    
                    }
                }
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Default");
        }
    }
}