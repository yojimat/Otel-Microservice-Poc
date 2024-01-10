using Microsoft.AspNetCore.Mvc.RazorPages;
using Utils.Messaging;

namespace Microservice_2.Pages;
public class IndexModel(ILogger<IndexModel> logger, MessageReceiver messageReceiver) : PageModel
{
    public void OnGet()
    {
        logger.LogInformation("Microservice-2: Index page visited");

        BasicDeliverEventArgs ea = new("received", new BasicProperties(), new List<byte>());

        messageReceiver.ReceiveMessage(ea);
    }
}
