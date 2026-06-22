using Microsoft.AspNetCore.Mvc;
using Infrastructure.Models;
using Infrastructure.Repository;

namespace APAEApplication.Controllers
{
    public class EventController : Controller
    {
        private readonly EventRepository _eventRepository = new EventRepository();

        [HttpGet]
        public IActionResult Index()
        {
            var events = _eventRepository.GetAll();
            return View(events);
        }

        // Tela de cadastro restrita ao Admin
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Event());
        }

        [HttpPost]
        public IActionResult Create(Event newEvent)
        {
            if (newEvent is null) return View(newEvent);

            _eventRepository.Create(newEvent);

            // Retorna ao painel de controle após cadastrar
            return RedirectToAction("AdminIndex");
        }

        // --- ÁREA ADMINISTRATIVA ---

        // 1. Listagem de Gestão
        [HttpGet]
        public IActionResult AdminIndex()
        {
            var events = _eventRepository.GetAll();
            return View(events);
        }

        // 2. Edição (Carrega os dados)
        [HttpGet]
        public IActionResult Update(int id)
        {
            var ev = _eventRepository.GetById(id);
            if (ev == null) return NotFound();

            return View(ev);
        }

        // 3. Edição (Salva os dados)
        [HttpPost]
        public IActionResult Update(Event updatedEvent)
        {
            _eventRepository.Update(updatedEvent);
            return RedirectToAction("AdminIndex");
        }

        // 4. Exclusão (Tela de Confirmação)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var ev = _eventRepository.GetById(id);
            if (ev == null) return NotFound();

            return View(ev);
        }

        // 5. Exclusão (Executa a ação)
        [HttpPost]
        public IActionResult ConfirmDelete(int id)
        {
            _eventRepository.Delete(id);
            return RedirectToAction("AdminIndex");
        }

    }
}