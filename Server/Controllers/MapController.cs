using Microsoft.AspNetCore.Mvc;

namespace BluForTracker.Server.Controllers;

public class MapController : Controller
{
    [HttpGet]
    public ActionResult<string> GetGoogleMapsApiKey([FromServices] IConfiguration configuration) {
        return configuration["GoogleMapsApiKey"] ?? throw new KeyNotFoundException("Could not find the required configuration key GoogleMapsApiKey");
    }
}
