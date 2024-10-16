using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazorContextMenu();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
