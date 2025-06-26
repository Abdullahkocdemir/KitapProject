using KitapProject.Context;
using KitapProject.DTO.TestimonialDTO;
using KitapProject.Entities;
using Microsoft.AspNetCore.Mvc;
using AutoMapper; 

namespace KitapProject.Controllers
{
    public class AdminTestimonialController : Controller
    {
        private readonly BookContext _context;
        private readonly IMapper _mapper;

        public AdminTestimonialController(BookContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var values = _context.Testimonials.ToList();
            var result = _mapper.Map<List<ResultTestimonialDTO>>(values);
            return View(result);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateTestimonialDTO createTestimonialDTO)
        {
            if (ModelState.IsValid)
            {
                var testimonial = _mapper.Map<Testimonial>(createTestimonialDTO);
                _context.Testimonials.Add(testimonial);
                _context.SaveChanges();
                TempData["Başarılı Bir Şekilde Kayıt Edildi"] = "Referans başarıyla eklendi!";
                return RedirectToAction("Index");
            }
            return View(createTestimonialDTO);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var testimonial = _context.Testimonials.Find(id);
            if (testimonial != null)
            {
                _context.Testimonials.Remove(testimonial);
                _context.SaveChanges();
                TempData["Başarılı Bir Şekilde Silindi"] = "Referans başarıyla silindi!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var testimonial = _context.Testimonials.Find(id);
            if (testimonial == null)
            {
                return NotFound();
            }
            var updateDto = _mapper.Map<UpdateTestimonialDTO>(testimonial);
            return View(updateDto);
        }

        [HttpPost]
        public IActionResult Edit(UpdateTestimonialDTO updateTestimonialDTO)
        {
            if (ModelState.IsValid)
            {
                var testimonial = _context.Testimonials.Find(updateTestimonialDTO.TestimonialId);
                if (testimonial == null)
                {
                    return NotFound();
                }
                _mapper.Map(updateTestimonialDTO, testimonial);
                _context.Testimonials.Update(testimonial);
                _context.SaveChanges();
                TempData["Başarılı Bir Şekilde Güncellendi"] = "Referans başarıyla güncellendi!";
                return RedirectToAction("Index");
            }
            return View(updateTestimonialDTO);
        }


        [HttpGet]
        public IActionResult Detail(int id)
        {
            var testimonial = _context.Testimonials.Find(id);
            if (testimonial == null)
            {
                return NotFound();
            }
            var detailDto = _mapper.Map<GetByIdTestimonialDTO>(testimonial);
            return View(detailDto);
        }
    }
}