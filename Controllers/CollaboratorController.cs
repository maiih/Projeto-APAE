using Microsoft.AspNetCore.Mvc;
using Infrastructure.Models;
using Infrastructure.Repository;
using APAEApplication.Services;

namespace APAEApplication.Controllers
{
    public class CollaboratorController : Controller
    {
        private readonly CollaboratorRepository _collaboratorRepository = new CollaboratorRepository();
        private readonly EventRepository _eventRepository = new EventRepository();
        private readonly EmailService _emailService;

        public CollaboratorController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Create(int eventId)
        {
            if (eventId <= 0) return RedirectToAction("Index", "Event");

            var collaborator = new Collaborator { EventId = eventId };
            return View(collaborator);
        }

        [HttpPost]
        public IActionResult Create(Collaborator collaborator)
        {
            if (collaborator is null) return View(collaborator);

            _collaboratorRepository.Create(collaborator);
            return RedirectToAction("Index", "Event");
        }

        [HttpGet]
        public IActionResult Index(int? eventId, string helpType)
        {
            var collaborators = GetFilteredCollaborators(eventId, helpType);

            ViewBag.Events = _eventRepository.GetAll();
            ViewBag.CurrentEventFilter = eventId;
            ViewBag.CurrentHelpFilter = helpType;

            return View(collaborators);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(int id, int? eventId, string? helpType)
        {
            var collaborator = _collaboratorRepository.GetAll().FirstOrDefault(c => c.Id == id);
            if (collaborator == null)
            {
                return RedirectToAction(nameof(Index), new { eventId, helpType });
            }

            await SendCollaboratorEmailAsync(collaborator);

            return RedirectToAction(nameof(Index), new { eventId, helpType });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSelectedEmails(List<int>? selectedIds, int? eventId, string? helpType)
        {
            selectedIds ??= new List<int>();

            var collaborators = _collaboratorRepository.GetAll()
                .Where(c => selectedIds.Contains(c.Id))
                .ToList();

            foreach (var collaborator in collaborators)
            {
                await SendCollaboratorEmailAsync(collaborator);
            }

            return RedirectToAction(nameof(Index), new { eventId, helpType });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAllEmails(int? eventId, string? helpType)
        {
            var collaborators = GetFilteredCollaborators(eventId, helpType);

            foreach (var collaborator in collaborators)
            {
                await SendCollaboratorEmailAsync(collaborator);
            }

            return RedirectToAction(nameof(Index), new { eventId, helpType });
        }

        private List<Collaborator> GetFilteredCollaborators(int? eventId, string? helpType)
        {
            var collaborators = _collaboratorRepository.GetAll();

            if (eventId.HasValue && eventId > 0)
            {
                collaborators = collaborators.Where(c => c.EventId == eventId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(helpType) &&
                Enum.TryParse<Core.Enums.HelpType>(helpType, out var helpEnum))
            {
                collaborators = collaborators.Where(c => c.HelpTypes.Contains(helpEnum)).ToList();
            }

            return collaborators;
        }

        private async Task SendCollaboratorEmailAsync(Collaborator collaborator)
        {
            if (string.IsNullOrWhiteSpace(collaborator.Email))
                return;

            var eventInfo = _eventRepository.GetById(collaborator.EventId);

            var helpTypesText = collaborator.HelpTypes != null && collaborator.HelpTypes.Any()
                ? string.Join(", ", collaborator.HelpTypes.Select(h =>
                    h == Core.Enums.HelpType.MaoDeObra ? "Mão de obra" :
                    h == Core.Enums.HelpType.DoacaoFinanceira ? "Doação financeira" :
                    h == Core.Enums.HelpType.Armazenamento ? "Armazenamento" :
                    "Outros"))
                : "Não informado";

            var subject = eventInfo != null
                ? $"APAE - Confirmação do evento {eventInfo.Title}"
                : "APAE - Confirmação de cadastro";

            var body = eventInfo != null
                ? $@"Olá, {collaborator.Name}!

Seu cadastro como voluntário foi confirmado para o evento:
{eventInfo.Title}
Data: {eventInfo.Date}

Forma de ajuda informada:
{helpTypesText}

Obrigado por colaborar com a APAE!"
                : $@"Olá, {collaborator.Name}!

Seu cadastro como voluntário foi confirmado.

Forma de ajuda informada:
{helpTypesText}

Obrigado por colaborar com a APAE!";

            await _emailService.SendEmailAsync(collaborator.Email, subject, body);

            collaborator.EmailSent = true;
        }
    }
}
