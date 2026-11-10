using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ApiService _api;

    public AccountController(ApiService api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // 1. Send the login request to the API
        var response = await _api.PostAsync("/api/auth/login", model);

        // 2. If the API says "OK", we log them in!
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();

                // Only try to parse if the API actually returned data
                if (!string.IsNullOrWhiteSpace(json) && json.Trim().StartsWith("{"))
                {
                    var data = JsonDocument.Parse(json).RootElement;

                    if (data.TryGetProperty("token", out var tokenProp))
                    {
                        HttpContext.Session.SetString("JwtToken", tokenProp.GetString()!);
                    }
                    if (data.TryGetProperty("roleName", out var roleProp))
                    {
                        HttpContext.Session.SetString("RoleName", roleProp.GetString()!);
                    }
                    if (data.TryGetProperty("fullName", out var nameProp))
                    {
                        HttpContext.Session.SetString("FullName", nameProp.GetString()!);
                    }
                }
            }
            catch
            {
                // Ignore JSON errors. The 200 OK is enough to let them in.
            }

            // Fallbacks in case the API didn't send JSON (e.g. session cookies)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("RoleName")))
                HttpContext.Session.SetString("RoleName", "Organiser");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("FullName")))
                HttpContext.Session.SetString("FullName", model.Email);

            // 3. Explicitly save the session before redirecting
            await HttpContext.Session.CommitAsync();

            // 4. Redirect to the Organiser Dashboard
            return RedirectToAction("Dashboard", "Organiser");
        }

        // 5. If the API says "Unauthorized", show the error
        ModelState.AddModelError("", "Invalid email or password.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Send the registration request to the API
        var response = await _api.PostAsync("/api/auth/register", model);

        if (!response.IsSuccessStatusCode)
        {
            // THIS IS THE FIX: Read the actual error from the API so we can see what's wrong
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"API Error: {response.StatusCode} - {errorContent}");
            return View(model);
        }

        return RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}