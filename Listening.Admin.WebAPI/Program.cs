using CommonInitializer;
using Listening.Admin.WebAPI.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureDbConfiguration();
//ÉèÖÃ¼àÌý¶Ë¿Ú
builder.WebHost.UseUrls("http://*:50403");
builder.ConfigureExtraServices(new InitializerOptions
{
    LogFilePath = "e:/temp/Listening.Admin.log",
    EventBusQueueName = "Listening.Admin"
});
builder.Services.AddScoped<EncodingEpisodeHelper>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Listening.Admin.WebAPI", Version = "v1" });
});
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Listening.Admin.WebAPI v1"));
}
app.MapHub<EpisodeEncodingStatusHub>("/Hubs/EpisodeEncodingStatusHub");
app.UseZackDefault();
app.MapControllers();
app.Run();
