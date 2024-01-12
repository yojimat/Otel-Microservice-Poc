using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenTelemetry;

namespace Microservice_1.Pages;
public class IndexModel(ILogger<IndexModel> logger, ActivitySource activitySource, HttpClient httpClient) : PageModel
{
    public string Message { get; private set; } = "Content didn't come.";
    public string NumberReceived { get; private set; } = "No number received.";
    public string NumberGenerated { get; private set; } = "No number generated.";

    public async Task OnGet()
    {
        logger.LogInformation("Microservice-1: Index page visited");

        var randomNumberReceived = Baggage.GetBaggage("number-generated");
        Activity.Current?.SetTag("random-number-received", randomNumberReceived);

        NumberReceived = randomNumberReceived ?? "No number received.";

        HttpResponseMessage result;

        var randomNumberGenerator = new Random();
        var randomNumber = randomNumberGenerator.Next(7, 13);

        NumberGenerated = randomNumber.ToString();

        try
        {
            const string activityName = "microservice-1-sending-number";

            using var activity = activitySource.StartActivity(activityName, ActivityKind.Producer);
            activity?.AddTag("microservice-who-created-number", "1");

            Baggage.SetBaggage("number-generated", randomNumber.ToString());

            result = await httpClient.GetAsync("http://host2:8080");

            if (!result.IsSuccessStatusCode) return;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
            return;
        }

        var content = await result.Content.ReadAsByteArrayAsync();
        if (content.Length > 0)
        {
            var contentString = Encoding.UTF8.GetString(content);
            Message = contentString;
        }
    }
}
