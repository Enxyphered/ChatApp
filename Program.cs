using ChatApp.Hubs;
using ChatApp.Endpoints;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddAntiforgery();
var app = builder.Build();

app.UseHttpsRedirection();
app.MapHomePage();
app.MapSendMessage();
app.MapHub<ChatHub>("/chathub");
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();