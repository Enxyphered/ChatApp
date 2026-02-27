using Microsoft.AspNetCore.Antiforgery;

namespace ChatApp.Slices.Pages.Home;

public record HomePageModel(AntiforgeryTokenSet Tokens);
