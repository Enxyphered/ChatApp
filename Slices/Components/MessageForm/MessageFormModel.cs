using Microsoft.AspNetCore.Antiforgery;

namespace ChatApp.Slices.Components.MessageForm;

public record MessageFormModel(AntiforgeryTokenSet Tokens);
