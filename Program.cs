// Startup / Program.cs のビルド直後に追加してください（WASM の場合 Program.Main の builder.Services に）
builder.Services.AddScoped<BrothersEnglishApp.Services.SpeechService>();
builder.Services.AddScoped<BrothersEnglishApp.Services.KeyboardService>();