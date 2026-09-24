using LifestyleChecker.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddHttpClient<PatientApiClient>(client =>
{
    client.BaseAddress =
        new Uri("https://al-tech-test-apim.azure-api.net/");
    client.Timeout = TimeSpan.FromSeconds(5);

    var key = builder.Configuration["PatientApi:SubscriptionKey"];
    if (string.IsNullOrWhiteSpace(key))
        throw new InvalidOperationException(
            "Configure PatientApi:SubscriptionKey before starting the app.");

    client.DefaultRequestHeaders.Add(
        "Ocp-Apim-Subscription-Key", key);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
