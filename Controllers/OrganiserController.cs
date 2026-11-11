using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceBay.MVC.Models.Dto;
using RaceBay.MVC.Models.ViewModels;
using RaceBay.MVC.Services;
using RaceBay.MVC.Services.Interfaces;

namespace RaceBay.MVC.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserController : Controller
{
    private readonly IApiService _api;

    public OrganiserController(IApiService api)
    {
        _api = api;
    }

    // Helper to attach the JWT token to the API requests
    private void SetToken()
    {
        var token = User.FindFirstValue(ClaimsHelper.Token);
        _api.SetAuthToken(token);
    }

    // ─────────── 1. DASHBOARD ───────────
    public async Task<IActionResult> Index()
    {
        SetToken();
        var response = await _api.GetAsync("api/organiser/dashboard");
        if (!response.IsSuccessStatusCode) return View(new DashboardStatsDto());

        var stats = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
        return View(stats ?? new DashboardStatsDto());
    }

    // ─────────── 2. EVENTS ───────────
    public async Task<IActionResult> Events()
    {
        SetToken();
        var response = await _api.GetAsync("api/events"); // Assuming this gets organiser's events
        if (!response.IsSuccessStatusCode) return View(new List<EventDto>());

        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>();
        return View(events ?? new List<EventDto>());
    }

    // CREATE
    [HttpGet]
    public async Task<IActionResult> CreateEvent()
    {
        SetToken();
        var response = await _api.GetAsync("api/categories");
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? new List<CategoryDto>();

        ViewBag.Categories = categories;
        return View(new EventFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SetToken();
            var response = await _api.GetAsync("api/categories");
            ViewBag.Categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
            return View(model);
        }

        SetToken();
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.Name), "Name");
        content.Add(new StringContent(model.Description), "Description");
        content.Add(new StringContent(model.Date.ToString("yyyy-MM-dd")), "Date");
        content.Add(new StringContent(model.Location), "Location");
        content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

        if (model.BannerImage != null)
        {
            var fileContent = new StreamContent(model.BannerImage.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.BannerImage.ContentType);
            content.Add(fileContent, "BannerImage", model.BannerImage.FileName);
        }

        var result = await _api.PostMultipartAsync("api/events", content);
        if (result.IsSuccessStatusCode)
            return RedirectToAction(nameof(Events));

        ModelState.AddModelError("", "Failed to create event via API.");
        return View(model);
    }

    // EDIT / UPDATE
    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        SetToken();
        var eventRes = await _api.GetAsync($"api/events/{id}");
        var categoriesRes = await _api.GetAsync("api/categories");

        if (!eventRes.IsSuccessStatusCode) return NotFound();

        var ev = await eventRes.Content.ReadFromJsonAsync<EventDto>();
        ViewBag.Categories = await categoriesRes.Content.ReadFromJsonAsync<List<CategoryDto>>();

        var model = new EventFormViewModel
        {
            Id = ev!.Id,
            Name = ev.Name,
            Description = ev.Description,
            Date = ev.Date,
            Location = ev.Location,
            CategoryId = ev.CategoryId,
            ExistingBannerUrl = ev.BannerImageUrl
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(EventFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SetToken();
            var catRes = await _api.GetAsync("api/categories");
            ViewBag.Categories = await catRes.Content.ReadFromJsonAsync<List<CategoryDto>>();
            return View(model);
        }

        SetToken();
        // Note: For simplicity, this assumes PUT accepts multipart/form-data.
        // If your API expects JSON for update, you'll need to adjust this.
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(model.Id.ToString()), "Id");
        content.Add(new StringContent(model.Name), "Name");
        content.Add(new StringContent(model.Description), "Description");
        content.Add(new StringContent(model.Date.ToString("yyyy-MM-dd")), "Date");
        content.Add(new StringContent(model.Location), "Location");
        content.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");

        if (model.BannerImage != null)
        {
            var fileContent = new StreamContent(model.BannerImage.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.BannerImage.ContentType);
            content.Add(fileContent, "BannerImage", model.BannerImage.FileName);
        }

        var result = await _api.PostMultipartAsync($"api/events/{model.Id}", content);
        if (result.IsSuccessStatusCode)
            return RedirectToAction(nameof(Events));

        ModelState.AddModelError("", "Failed to update event.");
        return View(model);
    }

    // DELETE
    [HttpPost]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        SetToken();
        await _api.DeleteAsync($"api/events/{id}");
        return RedirectToAction(nameof(Events));
    }

    // ─────────── 3. CATEGORIES ───────────
    public async Task<IActionResult> Categories()
    {
        SetToken();
        var response = await _api.GetAsync("api/categories");
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? new List<CategoryDto>();
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Categories)); // Add proper error handling

        SetToken();
        await _api.PostAsync("api/categories", model);
        return RedirectToAction(nameof(Categories));
    }

    // ─────────── 4. ENROLLMENTS ───────────
    public async Task<IActionResult> Enrollments(int eventId)
    {
        SetToken();
        var response = await _api.GetAsync($"api/events/{eventId}/enrollments");
        if (!response.IsSuccessStatusCode) return View(new List<EnrollmentDto>());

        var enrollments = await response.Content.ReadFromJsonAsync<List<EnrollmentDto>>();
        ViewBag.EventId = eventId; // Keep for routing
        return View(enrollments ?? new List<EnrollmentDto>());
    }

    // ─────────── 5. RESULTS ───────────
    [HttpGet]
    public async Task<IActionResult> EnterResults(int eventId)
    {
        SetToken();
        // Fetch confirmed participants for the dropdown
        var response = await _api.GetAsync($"api/events/{eventId}/enrollments?status=Confirmed");
        var enrollments = await response.Content.ReadFromJsonAsync<List<EnrollmentDto>>() ?? new List<EnrollmentDto>();

        ViewBag.Enrollments = enrollments;
        return View(new ResultFormViewModel { EventId = eventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterResults(ResultFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        SetToken();
        var result = await _api.PostAsync($"api/events/{model.EventId}/results", model);

        if (result.IsSuccessStatusCode)
            return RedirectToAction(nameof(Events)); // Or back to results list

        ModelState.AddModelError("", "Failed to publish results.");
        return View(model);
    }
}