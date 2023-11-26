using CommonInitializer;
using Identity.Repository;
using IdentityService.Domain;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Options;
using IdentityService.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using reCAPTCHA.AspNetCore;
using Zack.JWT;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.ConfigureDbConfiguration();
//���ü����˿�
builder.WebHost.UseUrls("http://*:50402");

builder.ConfigureExtraServices(new InitializerOptions
{
    EventBusQueueName = "IdentityService.WebAPI",
    LogFilePath = "d:/temp/IdentityService.log"
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IdentityService.WebAPI", Version = "v1" });
    //c.AddAuthenticationHeader();
});
//�ȸ���֤
var tesetst = builder.Configuration.GetSection("RecaptchaSettings");
builder.Services.AddRecaptcha(builder.Configuration.GetSection("RecaptchaSettings"));
builder.Services.AddDataProtection();
//��¼��ע�����Ŀ����Ҫ����WebApplicationBuilderExtensions�еĳ�ʼ��֮�⣬��Ҫ���µĳ�ʼ��
//��Ҫ��AddIdentity��������AddIdentityCore
//��Ϊ��AddIdentity�ᵼ��JWT���Ʋ������ã�AddJwtBearer�лص����ᱻִ�У��������AuthenticationУ��ʧ��
//https://github.com/aspnet/Identity/issues/1376
IdentityBuilder idBuilder = builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        //�����趨RequireUniqueEmail��������������Ϊ��
        //options.User.RequireUniqueEmail = true;
        //�������У���GenerateEmailConfirmationTokenAsync��֤������
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    }
);
idBuilder = new IdentityBuilder(idBuilder.UserType, typeof(Role), builder.Services);
idBuilder.AddEntityFrameworkStores<IdDbContext>().AddDefaultTokenProviders()
    //.AddRoleValidator<RoleValidator<Role>>()
    .AddRoleManager<RoleManager<Role>>()
    .AddUserManager<IdUserManager>();
//��������ע��
builder.Services.AddHttpClient();
//builder.Services//.AddOptions() //asp.net core��Ŀ��AddOptions()��дҲ�У���Ϊ���һ���Զ�ִ����
//    .Configure<SendCloudSmsSettings>(builder.Configuration.GetSection("QQEmail"));
    
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailSender, SendCloudEmailSender>();
    builder.Services.AddScoped<ISmsSender, MockSmsSender>();
}
else
{
    builder.Services.AddScoped<IEmailSender, SendCloudEmailSender>();
    builder.Services.AddScoped<ISmsSender, SendCloudSmsSender>();
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
}
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityService.WebAPI v1"));

app.UseZackDefault();
app.MapControllers();
app.Run();