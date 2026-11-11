using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceBay.MVC.Models.Dto;
using RaceBay.MVC.Models.ViewModels;
using RaceBay.MVC.Services;
using RaceBay.MVC.Services.Interfaces;

namespace RaceBay.MVC.Controllers;

[Authorize(Roles = "Participant")]
public class ParticipantController : Controller
{
    private readonly IApiService _api;

    public ParticipantController(IApiService api)
    {
        _api = api;
    }

    private void SetToken()
    {
        var token = User.FindFirstValue(ClaimsHelper.Token);
        _api.SetAuthToken(token);
    }

    // ─────────── 1. HOME PAGE (FILTERING) ───────────
    public async Task<IActionResult> Index(int? categoryId, string? searchString)
    {
        SetToken();

        // Build query string for filtering
        var query = "api/events?";
        if (categoryId.HasValue) query += $"categoryId={categoryId}&";
        if (!string.IsNullOrEmpty(searchString)) query += $"search={searchString}&";

        var eventsResponse = await _api.GetAsync(query.TrimEnd('&', '?'));
        var categoriesResponse = await _api.GetAsync("api/categories");

        var events = await eventsResponse.Content.ReadFromJsonAsync<List<EventDto>>() ?? new List<EventDto>();
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? new List<CategoryDto>();

        ViewBag.Categories = categories;
        ViewBag.CurrentCategory = categoryId;
        ViewBag.CurrentSearch = searchString;

        return View(events);
    }

    // ─────────── 2. EVENT DETAILS ───────────
    public async Task<IActionResult> EventDetails(int id)
    {
        SetToken();
        var response = await _api.GetAsync($"api/events/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var ev = await response.Content.ReadFromJsonAsync<EventDto>();
        return View(ev);
    }

    // ─────────── 3. ENROLL IN EVENT ───────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int eventId)
    {
        SetToken();
        // Assuming your API endpoint looks like this
        var response = await _api.PostAsync($"api/events/{eventId}/enroll", new { });

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Successfully enrolled! Your status is now Pending.";
        }
        else
        {
            TempData["Error"] = "Failed to enroll. You may already be enrolled.";
        }

        return RedirectToAction(nameof(EventDetails), new { id = eventId });
    }

    // ─────────── 4. MY ENROLLMENTS ───────────
    public async Task<IActionResult> MyEnrollments()
    {
        SetToken();
        var response = await _api.GetAsync("api/participants/my-enrollments");
        if (!response.IsSuccessStatusCode) return View(new List<EnrollmentDto>());

        var enrollments = await response.Content.ReadFromJsonAsync<List<EnrollmentDto>>();
        return View(enrollments ?? new List<EnrollmentDto>());
    }

    // ─────────── 5. MY RESULTS ───────────
    public async Task<IActionResult> MyResults()
    {
        SetToken();
        var response = await _api.GetAsync("api/participants/my-results");
        if (!response.IsSuccessStatusCode) return View(new List<ResultDto>());

        var results = await response.Content.ReadFromJsonAsync<List<ResultDto>>();
        return View(results ?? new List<ResultDto>());
    }

    // ─────────── 6. PROFILE (VIEW & UPDATE) ───────────
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        SetToken();
        var response = await _api.GetAsync("api/participants/profile");
        if (!response.IsSuccessStatusCode) return NotFound();

        var profile = await response.Content.ReadFromJsonAsync<ParticipantProfileDto>();

        var model = new ProfileViewModel
        {
            FirstName = profile!.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            ExistingProfilePictureUrl = profile.ProfilePictureUrl
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        SetToken();
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.FirstName), "FirstName");
        content.Add(new StringContent(model.LastName), "LastName");
        content.Add(new StringContent(model.Email), "Email");

        if (model.ProfilePicture != null)
        {
            var fileContent = new StreamContent(model.ProfilePicture.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ProfilePicture.ContentType);
            content.Add(fileContent, "ProfilePicture", model.ProfilePicture.FileName);
        }

        var result = await _api.PostMultipartAsync("api/participants/profile", content);

        if (result.IsSuccessStatusCode)
        {
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        ModelState.AddModelError("", "Failed to update profile.");
        return View(model);
    }
}