using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data.SqlClient;
using Zack.ASPNETCore;
using Zack.Commons;
using Zack.Commons.JsonConverters;
using Zack.EventBus;
using Zack.JWT;

namespace CommonInitializer
{
    public static class WebApplicationBuilderExtensions
    {
        public static void ConfigureDbConfiguration(this WebApplicationBuilder builder)
        {
            builder.Host.ConfigureAppConfiguration((hostCtx, configBuilder) =>
            {
                //不能使用ConfigureAppConfiguration中的configBuilder去读取配置，否则就循环调用了，因此这里直接自己去读取配置文件
                //var configRoot = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                //string connStr = configRoot.GetValue<string>("DefaultDB:ConnStr");
                string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
                configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
                //string connStr = "Server=42.194.206.48;Database=YouzackVNextDB;User Id=sa;Password=z110112zZ;";

                // ASP.NET Core 的配置系统支持多种配置源，包括：
                //appsettings.json：应用程序的配置文件，通常位于项目根目录。
                //环境变量：可以从环境变量中读取配置信息。
                ///命令行参数：可以从命令行参数中读取配置信息。
                /////用户机密存储：可以使用用户机密存储来存储敏感信息。
                /////默认情况下，ASP.NET Core 会自动加载这些配置源，并合并它们以构建应用程序的配置。这意味着如果某个配置在多个源中都有定义，它将按照一定的优先级顺序进行合并，从而得到最终的配置值。
            });
        }

        public static void ConfigureExtraServices(this WebApplicationBuilder builder, InitializerOptions initOptions)
        {
            IServiceCollection services = builder.Services;
            IConfiguration configuration = builder.Configuration;
            var assemblies = ReflectionHelper.GetAllReferencedAssemblies();
            services.RunModuleInitializers(assemblies);
            services.AddAllDbContexts(ctx =>
            {
                //连接字符串如果放到appsettings.json中，会有泄密的风险
                //如果放到UserSecrets中，每个项目都要配置，很麻烦
                //因此这里推荐放到环境变量中。
                string connStr = configuration.GetValue<string>("DefaultDB:ConnStr");
                ctx.UseSqlServer(connStr);
            }, assemblies);
            //邮箱
            

            //开始:Authentication,Authorization
            //只要需要校验Authentication报文头的地方（非IdentityService.WebAPI项目）也需要启用这些
            //IdentityService项目还需要启用AddIdentityCore
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication();

            //var test = configuration.GetSection("RecaptchaSettings").Get<RecaptchaOptions>();
            JWTOptions jwtOpt = configuration.GetSection("JWT").Get<JWTOptions>();
            builder.Services.AddJWTAuthentication(jwtOpt);

            //启用Swagger中的【Authorize】按钮。这样就不用每个项目的AddSwaggerGen中单独配置了
            builder.Services.Configure<SwaggerGenOptions>(c =>
            {
                c.AddAuthenticationHeader();
            });

            //结束:Authentication,Authorization

            var teststs = builder.Configuration.GetSection("FileService:Endpoint");
            //注入文件上传终端
            services.Configure<FileServiceOptions>(builder.Configuration.GetSection("FileService:Endpoint"));

            services.AddMediatR(assemblies);
            //现在不用手动AddMVC了，因此把文档中的services.AddMvc(options =>{})改写成Configure<MvcOptions>(options=> {})这个问题很多都类似
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<UnitOfWorkFilter>();
            });
            services.Configure<JsonOptions>(options =>
            {
                //设置时间格式。而非“2008-08-08T08:08:08”这样的格式
                options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
            });

            services.AddCors(options =>
                {

                    ////更好的在Program.cs中用绑定方式读取配置的方法：https://github.com/dotnet/aspnetcore/issues/21491
                    ////不过比较麻烦。
                    var corsOpt = configuration.GetSection("Cors").Get<CorsSettings>();

                    string[] urls = corsOpt.Origins;
                    options.AddDefaultPolicy(builder => builder.WithOrigins(urls)
                            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
                }
            );
            services.AddLogging(builder =>
            {
                Log.Logger = new LoggerConfiguration()
                   // .MinimumLevel.Information().Enrich.FromLogContext()
                   .WriteTo.Console()
                   .WriteTo.File(initOptions.LogFilePath)
                   .CreateLogger();
                builder.AddSerilog();
            });
            services.AddFluentValidation(fv =>
            {                
                fv.RegisterValidatorsFromAssemblies(assemblies);
            });
            var mytests = configuration.GetSection("JWT");
            var ccas = configuration.GetSection("RabbitMQ");
            var cca2s = configuration.GetSection("Recaptcha");
            //var sdfsd = configuration.GetSection("Redis:ConnStr");
            //var sdfsd44 = configuration.GetSection("Redis");
            //var sdfsd445 = configuration.GetValue<string>("Redis:ConnStr");

            services.Configure<JWTOptions>(configuration.GetSection("JWT"));
            services.Configure<IntegrationEventRabbitMQOptions>(configuration.GetSection("RabbitMQ"));
            services.AddEventBus(initOptions.EventBusQueueName, assemblies);

            //Redis的配置
            string redisConnStr = configuration.GetValue<string>("Redis:ConnStr");
            IConnectionMultiplexer redisConnMultiplexer = ConnectionMultiplexer.Connect(redisConnStr);
            services.AddSingleton(typeof(IConnectionMultiplexer), redisConnMultiplexer);
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
            });
        }
    }
}
