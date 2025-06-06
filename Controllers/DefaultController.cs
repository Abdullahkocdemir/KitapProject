using Microsoft.AspNetCore.Mvc;
using System.Linq;
using KitapProject.Entities;
using Microsoft.EntityFrameworkCore;
using KitapProject.Context;

namespace KitapProject.Controllers
{
    public class DefaultController : Controller
    {
        private readonly BookContext _context;

        public DefaultController(BookContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}