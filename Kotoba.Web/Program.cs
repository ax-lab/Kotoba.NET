using Microsoft.Extensions.FileProviders;

using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server;
using GraphQL.SystemTextJson;

const int PORT_SSL = 29801;
const int PORT_HTTP = 29802;

const bool USE_DEV_SERVER = true;

// Root path for the web application.
const string APP_ROOT = "App";

// Relative path from `APP_ROOT` for static web files.
const string PUBLIC_PATH = "public";

// Relative path from `APP_ROOT` for web files that are built by the
// development tools. In development mode, these files are merged with
// the `PUBLIC_PATH`.
const string BUILD_PATH = "build";

var builder = WebApplication.CreateBuilder();
var isDev = builder.Environment.IsDevelopment();


var appRoot = Path.Combine(builder.Environment.ContentRootPath, APP_ROOT);
var publicPath = Path.Combine(appRoot, PUBLIC_PATH);
var buildPath = Path.Combine(appRoot, BUILD_PATH);

builder.Services.AddGraphQL(options =>
{
	options.AddHttpMiddleware<Kotoba.Data.Schema, GraphQLHttpMiddleware<Kotoba.Data.Schema>>();
	options.AddSchema(new Kotoba.Data.Schema());
	options.ConfigureExecutionOptions(exec =>
	{
		exec.EnableMetrics = isDev;
	});
	options.AddSystemTextJson();
	if (isDev)
	{
		options.AddErrorInfoProvider(new Kotoba.Data.DetailedErrorInfoProvider());
	}
});

// Bind to `0.0.0.0` so that the application can be accessed on the network
// during development.
builder.WebHost.UseUrls(
	String.Format("https://0.0.0.0:{0}", PORT_SSL),
	String.Format("http://0.0.0.0:{0}", PORT_HTTP)
);

var app = builder.Build();

if (isDev)
{
	// Make sure this exists, in case we haven't run the web build yet.
	Directory.CreateDirectory(buildPath);

	Node.AppRoot = appRoot;
	if (Node.HasPackageJson)
	{
		Node.BeginNpmCommand(USE_DEV_SERVER ? "run serve" : "run watch");
	}

	app.Lifetime.ApplicationStarted.Register(() =>
	{
		app.Logger.LogInformation("Local listener at https://localhost:{0}", PORT_SSL);
		app.Logger.LogInformation("Local listener at http://localhost:{0}", PORT_HTTP);
	});
}

app.Lifetime.ApplicationStopping.Register(() =>
{
	app.Logger.LogInformation("Shutting down");
});

// File routing:

var publicFileProvider = new PhysicalFileProvider(publicPath);

IFileProvider fileProvider = isDev
	? new CompositeFileProvider(
		publicFileProvider,
		new PhysicalFileProvider(buildPath))
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

//----------------------------------------------------------------------------//
// Routing and endpoints
//----------------------------------------------------------------------------//

app.UseGraphQL<Kotoba.Data.Schema>();
app.UseGraphQLGraphiQL();

app.MapGet("/hello", () => "hello world!!!");

app.Run();
