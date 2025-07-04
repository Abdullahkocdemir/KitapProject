﻿using KitapProject.Context;
using Microsoft.AspNetCore.Mvc;

namespace KitapProject.ViewComponents
{
    public class _DefaultLayoutTestimonialPartials : ViewComponent
    {

        private readonly BookContext _context;

        public _DefaultLayoutTestimonialPartials(BookContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var values = _context.Testimonials.ToList();
            return View(values);
        }
    }
}
