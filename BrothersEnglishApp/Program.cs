using BrothersEnglishApp;
using BrothersEnglishApp.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// LocalStorageService‚ðŽg‚¦‚é‚æ‚¤‚É“o˜^‚·‚é
builder.Services.AddScoped<BrothersEnglishApp.Services.LocalStorageService>();
builder.Services.AddScoped<UserContext>();

await builder.Build().RunAsync();
