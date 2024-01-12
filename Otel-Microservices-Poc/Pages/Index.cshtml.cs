using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenTelemetry;

namespace Otel_Microservices_Poc.Pages;
public class IndexModel(ILogger<IndexModel> logger, ActivitySource activitySource, HttpClient httpClient) : PageModel
{
    public string ResultFromRequest { get; private set; } = "Content didn't come.";
    public string NumberGenerated { get; private set; } = "No number generated.";

    public async Task OnGet()
    {
        logger.LogInformation("Microservice-0: Index page visited");

        HttpResponseMessage result;

        var randomNumberGenerator = new Random();
        var randomNumber = randomNumberGenerator.Next(1, 7);

        NumberGenerated = randomNumber.ToString();

        try
        {
            const string activityName = "microservice-0-sending-number";

            using var activity = activitySource.StartActivity(activityName, ActivityKind.Producer);
            activity?.AddTag("microservice-who-created-number", "0");

            Baggage.SetBaggage("number-generated", randomNumber.ToString());

            result = await httpClient.GetAsync("http://host1:8080"); 

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
            ResultFromRequest = contentString;
        }
    }
}
