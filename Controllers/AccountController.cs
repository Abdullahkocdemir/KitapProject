using KitapProject.Entities; // AppUser ve AppRole için
using KitapProject.Models; // ViewModel'lar için
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis; // Redis için
using System.Security.Claims;
using System.Text.Json; // JSON serileştirme için

namespace KitapProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IDatabase _redisDb; // Redis için

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConnectionMultiplexer redis)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _redisDb = redis.GetDatabase();
        }

        // --- KULLANICIYI REDIS'E KAYDETME (ÖNBELLEKLEME) METODU ---
        // Bu metot, bir kullanıcı başarıyla kaydedildiğinde veya bilgileri güncellendiğinde çağrılabilir.
        private async Task SaveUserToRedisCache(AppUser user)
        {
            if (user == null) return;
            string redisKey = $"user:{user.Id}";
            string userJson = JsonSerializer.Serialize(user);
            await _redisDb.StringSetAsync(redisKey, userJson, TimeSpan.FromHours(1)); // 1 saatlik önbellek süresi
            Console.WriteLine($"Redis'e kaydedildi: Anahtar='{redisKey}', Kullanıcı='{user.UserName}'");
        }

        // --- KULLANICIYI REDIS'TEN ÇEKME (ÖNBELLEKTEN OKUMA) METODU ---
        // Bu metot, bir kullanıcı bilgisini almanız gerektiğinde, önce Redis'e bakmak için kullanılabilir.
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

                    // --- START OF FIX FOR REGISTER METHOD ---
                    // Manually build claims for the newly registered user
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName), // Usually username/email
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

                    // Add any default roles or other claims needed on registration
                    // Example: Add a default role if new users always get one (e.g., "Customer")
                    // await _userManager.AddToRoleAsync(user, "Customer"); // Make sure "Customer" role exists
                    // claims.Add(new Claim(ClaimTypes.Role, "Customer"));

                    var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);
                    // --- END OF FIX FOR REGISTER METHOD ---

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: /Account/Login (Giriş formunu gösterir)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login (Giriş formunu işler)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) // Assuming you have a LoginViewModel
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email); // Or FindByEmailAsync if using email as username
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.Email); // Try finding by email too
                }

                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        // --- IMPORTANT: ADDING CUSTOM CLAIMS HERE ---
                        var claims = new List<Claim>();

                        // Add standard claims (UserName is usually default for User.Identity.Name)
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName)); // This will typically be your username/email

                        // Add your custom claims from AppUser properties
                        if (!string.IsNullOrEmpty(user.FirstName))
                        {
                            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName)); // Standard claim for first name
                        }
                        if (!string.IsNullOrEmpty(user.LastName))
                        {
                            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));   // Standard claim for last name
                        }
                        if (!string.IsNullOrEmpty(user.FullName))
                        {
                            claims.Add(new Claim("FullName", user.FullName)); // Your custom "FullName" claim
                        }

                        // Get existing user claims (e.g., roles)
                        var userClaims = await _userManager.GetClaimsAsync(user);
                        claims.AddRange(userClaims);

                        // Get user roles and add them as claims
                        var userRoles = await _userManager.GetRolesAsync(user);
                        foreach (var role in userRoles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        // Create a new ClaimsIdentity with all the desired claims
                        var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        // Sign in the user with the new ClaimsPrincipal
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

                        // If you are using _signInManager.SignInAsync directly,
                        // you might need to ensure it includes the custom claims.
                        // For example, you might override SignInAsync in a custom SignInManager,
                        // or use _signInManager.PasswordSignInAsync and then _userManager.AddClaimsAsync.
                        // The above HttpContext.SignInAsync gives you full control.

                        // Redirect to home or return URL
                        return RedirectToAction("Index", "Home");
                    }
                    if (result.IsLockedOut)
                    {
                        // Handle locked out user
                        ModelState.AddModelError(string.Empty, "Hesabınız kilitlendi.");
                        return View(model);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        // Handle two-factor authentication
                        return RedirectToAction("LoginWith2fa", new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });    
                    }
                }
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // --- ÇIKIŞ (LOGOUT) EYLEMİ ---

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Çıkış sonrası ana sayfaya yönlendir
        }
    }
}