using ChatApp.Hubs;
using ChatApp.Slices.Components.MessageForm;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Endpoints;

public static class SendMessageEndpoint
{
    public static void MapSendMessage(this WebApplication app)
    {
        app.MapPost("/send-message", async (
            HttpContext context,
            IHubContext<ChatHub> hubContext,
            IAntiforgery antiforgery) =>
        {
            var form = await context.Request.ReadFormAsync();
            var user = System.Net.WebUtility.HtmlEncode(form["user"].ToString());
            var message = System.Net.WebUtility.HtmlEncode(form["message"].ToString());

            var htmlMessage = $"<li><strong>{user}</strong> says {message}</li>";
            await hubContext.Clients.All.SendAsync("ReceiveMessage", htmlMessage);

            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Extensions.RazorSlice<MessageForm, MessageFormModel>(new MessageFormModel(tokens));
        });
    }
}
