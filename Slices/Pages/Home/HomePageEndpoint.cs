using ChatApp.Slices.Pages.Home;
using Microsoft.AspNetCore.Antiforgery;

namespace ChatApp.Endpoints;

public static class HomePageEndpoint
{
    public static void MapHomePage(this WebApplication app)
    {
        app.MapGet("/", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Extensions.RazorSlice<HomePage, HomePageModel>(new HomePageModel(tokens));
        });
    }
}
