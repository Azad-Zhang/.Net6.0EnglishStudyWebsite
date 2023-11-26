using CommonInitializer;
using FileService.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureDbConfiguration();
//���ü����˿�
builder.WebHost.UseUrls("http://*:50401");
builder.ConfigureExtraServices(new InitializerOptions
{
    LogFilePath = "e:/temp/FileService.log",
    EventBusQueueName = "FileService.WebAPI",//��������������趨����󶨵Ķ��е�����
});
// Add services to the container.
var testc = builder.Configuration.GetSection("FileService:SMB");
builder.Services//.AddOptions() //asp.net core��Ŀ��AddOptions()��дҲ�У���Ϊ���һ���Զ�ִ����
    .Configure<SMBStorageOptions>(builder.Configuration.GetSection("FileService:SMB"));
//.Configure<UpYunStorageOptions>(builder.Configuration.GetSection("FileService:UpYun"))

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FileService.WebAPI", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileService.WebAPI v1"));
}
app.UseStaticFiles();
app.UseZackDefault();

app.MapControllers();

app.Run();
