using System.Threading.RateLimiting;
using ThoughtTrashCan;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AzureOptions>(builder.Configuration.GetSection("Azure"));
builder.Services.AddSingleton<ReleaseAgent>();
builder.Services.AddHttpClient<SpeechTokenService>();

// Each visitor gets a small budget of AI replies so one tab can't run up the bill.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("per-ip", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 12, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

app.UseDefaultFiles();   // serves wwwroot/index.html (the Three.js room)
app.UseStaticFiles();
app.UseRateLimiter();

// The thought is used for one reply and then dropped: never logged, never stored.
app.MapPost("/api/release", async (ReleaseRequest req, ReleaseAgent agent, CancellationToken ct) =>
{
    var text = req.Text?.Trim() ?? "";
    if (text.Length == 0) return Results.BadRequest(new { error = "Write something before tossing it." });
    if (text.Length > 1200) text = text[..1200];
    if (!agent.IsConfigured) return Results.NotFound();

    var result = await agent.ReleaseAsync(text, ct);
    return Results.Ok(result);
}).RequireRateLimiting("per-ip");

// Short-lived (10 min) Azure AI Speech token so the browser never sees the key.
app.MapGet("/api/speech/token", async (SpeechTokenService speech, CancellationToken ct) =>
{
    var token = await speech.IssueAsync(ct);
    return token is null ? Results.NotFound() : Results.Ok(token);
}).RequireRateLimiting("per-ip");

app.MapGet("/healthz", () => Results.Ok("ok"));

app.Run();
