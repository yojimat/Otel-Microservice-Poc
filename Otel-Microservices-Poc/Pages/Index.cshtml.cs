using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Otel_Microservices_Poc.Pages;
public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public string Message { get; private set; } = "Content didn't come.";

    public async Task OnGet()
    {
        logger.LogInformation("Microservice-0: Index page visited");

        var httpClient = new HttpClient();
        HttpResponseMessage result;

        try
        {
            result = await httpClient.GetAsync("http://host1:8080");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
            return;
        }

        if (!result.IsSuccessStatusCode) return;

        var content = await result.Content.ReadAsByteArrayAsync();
        if (content.Length > 0)
        {
            var contentString = Encoding.UTF8.GetString(content);
            Message = contentString;
        }
    }
}
