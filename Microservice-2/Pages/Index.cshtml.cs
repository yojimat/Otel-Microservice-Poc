using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenTelemetry;

namespace Microservice_2.Pages;
public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public string NumberReceived { get; private set; } = "No number received.";

    public void OnGet()
    {
        logger.LogInformation("Microservice-2: Index page visited");

        var randomNumberReceived = Baggage.GetBaggage("number-generated");
        Activity.Current?.SetTag("random-number-received", randomNumberReceived);

        NumberReceived = randomNumberReceived ?? "No number received.";
    }
}
