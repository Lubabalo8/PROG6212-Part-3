namespace RaceBay.MVC.Services;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public Endpoints Endpoints { get; set; } = new();
}

public class Endpoints
{
    public string Login { get; set; } = "api/auth/login";
    public string Register { get; set; } = "api/auth/register";
    public string Events { get; set; } = "api/events";
    public string Categories { get; set; } = "api/categories";
    public string Enrollments { get; set; } = "api/enrollments";
    public string Results { get; set; } = "api/results";
}
