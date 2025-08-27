using DevPortfolio.Components;
using DevPortfolio.Models;
using DevPortfolio.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ResumeSettings>(builder.Configuration.GetSection("Resume"));
builder.Services.Configure<ResumeParserSettings>(builder.Configuration.GetSection("ResumeParser"));
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("GitHub"));

builder.Services.AddHttpClient("GoogleDocs", c =>
{
    c.Timeout = TimeSpan.FromMinutes(2);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("DevPortfolio/1.0 (+https://localhost)");
    c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
});

builder.Services.AddHttpClient("ResumeApi", (sp, c) =>
{
    var opts = sp.GetRequiredService<IOptions<ResumeParserSettings>>().Value;
    if (string.IsNullOrWhiteSpace(opts.BaseUrl))
        throw new InvalidOperationException("Missing configuration: ResumeParser:BaseUrl");
    c.BaseAddress = new Uri(opts.BaseUrl, UriKind.Absolute);
    c.Timeout = TimeSpan.FromMinutes(2);
});


builder.Services.AddMemoryCache();
builder.Services.AddScoped<IResumeService, ResumeService>();

builder.Services.AddHttpClient("GitHub", c =>
{
    c.BaseAddress = new Uri("https://api.github.com/");
    c.DefaultRequestHeaders.UserAgent.ParseAdd("DevPortfolio/1.0 (+https://localhost)");
    c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
});

builder.Services.AddScoped<IGitHubService, GitHubService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
