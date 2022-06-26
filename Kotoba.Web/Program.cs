using Microsoft.Extensions.FileProviders;

const int PORT_SSL = 29801;
const int PORT_HTTP = 29802;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	WebRootPath = "App/public",
});

builder.WebHost.UseUrls(
	String.Format("https://0.0.0.0:{0}", PORT_SSL),
	String.Format("http://0.0.0.0:{0}", PORT_HTTP)
);

var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() =>
{
	app.Logger.LogInformation("Local listener at https://localhost:{0}", PORT_SSL);
	app.Logger.LogInformation("Local listener at http://localhost:{0}", PORT_HTTP);
});

app.MapGet("/hello", () => "hello world!!!");
app.UseFileServer();

app.MapFallbackToFile("/index.html");

app.Run();
