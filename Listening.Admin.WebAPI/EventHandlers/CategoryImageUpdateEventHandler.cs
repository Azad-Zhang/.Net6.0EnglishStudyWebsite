using CommonInitializer;
using Dynamic.Json;
using FileService.SDK.NETCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using Zack.EventBus;
using Zack.JWT;

namespace Listening.Admin.WebAPI.EventHandlers
{
    [EventName("CategoryImage.Created")]
    public class CategoryImageUpdateEventHandler : DynamicIntegrationEventHandler
    {
        private readonly IServiceScope serviceScope;
        private readonly IOptionsSnapshot<FileServiceOptions> optionFileService;
        private readonly IOptionsSnapshot<JWTOptions> optionJWT;
        private readonly ITokenService tokenService;
        private readonly IHttpClientFactory httpClientFactory;

        public CategoryImageUpdateEventHandler(IServiceScopeFactory spf)
        {
            //MEDbContext等是Scoped，而BackgroundService是Singleton，所以不能直接注入，需要手动开启一个新的Scope
            this.serviceScope = spf.CreateScope();
            var sp = serviceScope.ServiceProvider;
            
            IConnectionMultiplexer connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
           
            this.httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
           
            this.optionFileService = sp.GetRequiredService<IOptionsSnapshot<FileServiceOptions>>();
            
            this.optionJWT = sp.GetRequiredService<IOptionsSnapshot<JWTOptions>>();
            this.tokenService = sp.GetRequiredService<ITokenService>();
            
        }
        string BuildToken()
        {
            //因为JWT的key等机密信息只有服务器端知道，因此可以这样非常简单的读到配置
            Claim claim = new Claim(ClaimTypes.Role, "Admin");
            return tokenService.BuildToken(new Claim[] { claim }, optionJWT.Value);
        }

        public override async Task<dynamic> HandleDynamic(string eventName, dynamic eventData)
        {

            Uri urlRoot = optionFileService.Value.UrlRoot;
            // 检查是否包含名为 myFile 的属性，并且类型是 IFormFile

            
            var fileUploadDto = JsonConvert.DeserializeObject<FileUploadDto>(eventData.myFile);



            FileServiceClient fileService = new FileServiceClient(httpClientFactory,
                    urlRoot, optionJWT.Value, tokenService);
            var myResult = await fileService.UploadAsync(fileUploadDto);
            string token = BuildToken();
            using MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent(eventData.chinese), "chinese");
            content.Add(new StringContent(eventData.english), "english");
            var httpClient = httpClientFactory.CreateClient();
            //Uri requestUri = new Uri("http://42.194.206.48:50403/" + "Category/AddItem");
            Uri requestUri = new Uri("http://127.0.0.1:50403/" + "Category/AddItem");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var respMsg = await httpClient.PostAsync(requestUri, content);
            if (!respMsg.IsSuccessStatusCode)
            {
                string respString = await respMsg.Content.ReadAsStringAsync();
                throw new HttpRequestException($"上传失败，状态码：{respMsg.StatusCode}，响应报文：{respString}");
            }


            //这里获取到了远程访问路径，通过post请求，发回给控制器，添加分类即可。
            return true;


        }



    }
}
