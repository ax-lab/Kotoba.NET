var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(
	"https://0.0.0.0:29802",
	"http://0.0.0.0:29801"
);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
