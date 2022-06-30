using Microsoft.Extensions.FileProviders;

using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server;
using GraphQL.SystemTextJson;

const int PORT_SSL = 29801;
const int PORT_HTTP = 29802;

const string APP_ROOT = "App";

const bool USE_DEV_SERVER = true;

// Path for static files that are served directly.
const string PUBLIC_PATH = APP_ROOT + "/public";

// Path for built files. Those are merged with the public files.
const string BUILD_PATH = APP_ROOT + "/build";

var builder = WebApplication.CreateBuilder();

builder.Services.AddGraphQL(options =>
{
	options.AddSchema<AppSchema>();
	options.AddSystemTextJson();
	options.AddHttpMiddleware<AppSchema, GraphQLHttpMiddleware<AppSchema>>();
	options.AddGraphTypes(typeof(AppSchema).Assembly);
});

// Bind to `0.0.0.0` so that the application can be accessed on the network
// during development.
builder.WebHost.UseUrls(
	String.Format("https://0.0.0.0:{0}", PORT_SSL),
	String.Format("http://0.0.0.0:{0}", PORT_HTTP)
);

var app = builder.Build();

var isDev = app.Environment.IsDevelopment();

if (isDev)
{
	Node.AppRoot = Path.Combine(builder.Environment.ContentRootPath, APP_ROOT);
	if (Node.HasPackageJson)
	{
		Node.BeginNpmCommand(USE_DEV_SERVER ? "run serve" : "run watch");
	}
}

app.Lifetime.ApplicationStarted.Register(() =>
{
	app.Logger.LogInformation("Local listener at https://localhost:{0}", PORT_SSL);
	app.Logger.LogInformation("Local listener at http://localhost:{0}", PORT_HTTP);
});

app.Lifetime.ApplicationStopping.Register(() =>
{
	app.Logger.LogInformation("Shutting down");
});

// File routing:

var publicFileProvider = new PhysicalFileProvider(
	Path.Combine(builder.Environment.ContentRootPath, PUBLIC_PATH));

IFileProvider fileProvider = isDev
	? new CompositeFileProvider(
		publicFileProvider,
		new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, BUILD_PATH)))
	: publicFileProvider;

app.UseDefaultFiles(new DefaultFilesOptions
{
	FileProvider = fileProvider,
	DefaultFileNames = { "index.html" }
});

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = fileProvider,
	RequestPath = ""
});

// Any unknown routes fallback to the index.
app.MapFallbackToFile("index.html", new StaticFileOptions
{
	FileProvider = publicFileProvider,
});

// Additional routes:

app.UseGraphQL<AppSchema>();
app.UseGraphQLGraphiQL();

app.MapGet("/hello", () => "hello world!!!");

app.Run();
