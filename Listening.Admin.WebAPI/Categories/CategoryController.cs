using FileService.SDK.NETCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using Zack.EventBus;

namespace Listening.Admin.WebAPI.Categories;
[Route("[controller]/[action]")]
[Authorize(Roles = "Admin")]
[ApiController]
[UnitOfWork(typeof(ListeningDbContext))]
//供后台用的增删改查接口不用缓存
public class CategoryController : ControllerBase
{
    private IListeningRepository repository;
    private readonly ListeningDbContext dbContext;
    private readonly ListeningDomainService domainService;
    private readonly IEventBus eventBus;
    public CategoryController(ListeningDbContext dbContext, ListeningDomainService domainService, IListeningRepository repository, IEventBus eventBus)
    {
        this.dbContext = dbContext;
        this.domainService = domainService;
        this.repository = repository;
        this.eventBus = eventBus;
    }

    [HttpGet]
    public Task<Category[]> FindAll()
    {
        return repository.GetCategoriesAsync();
    }

    [HttpGet]
    //[Route("{id}")]
    public async Task<ActionResult<Category?>> FindById(Guid id)
    {
        //返回ValueTask的需要await的一下
        var cat = await repository.GetCategoryByIdAsync(id);
        if (cat == null)
        {
            return NotFound($"没有Id={id}的Category");
        }
        else
        {
            return cat;
        }
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Add([FromForm] string chinese, [FromForm] string english, [FromForm] IFormFile myFile)
    {
        using (var memoryStream = new MemoryStream())
        {
            myFile.CopyTo(memoryStream);
            var fileData = memoryStream.ToArray();

            var fileUploadDto = new FileUploadDto
            {
                FileName = myFile.FileName,
                FileData = fileData
            };

            // 将FileUploadDto对象转换为JSON字符串
            var json = JsonConvert.SerializeObject(fileUploadDto);

            // 将字节数组发送到服务器
            // 发送请求，将 fileBytes 作为请求体的一部分发送到服务器
            // ...
            //通知分类封面图片上传
            eventBus.Publish("CategoryImage.Created", new { chinese = chinese, english = english, myFile = json });
            
        }
        
        return Ok();
    }
    [HttpPost]
    public async Task<ActionResult<Guid>> AddItem([FromForm] string chinese, [FromForm] string english, [FromForm] Uri fileSrc)
    {
        var Name = new MultilingualString(chinese, english);
        var category = await domainService.AddCategoryAsync(Name, fileSrc);
        dbContext.Add(category);
        return category.Id;
    }


    [HttpPut]
    //[Route("{id}")]
    public async Task<ActionResult> Update( Guid id, CategoryUpdateRequest request)
    {
        var cat = await repository.GetCategoryByIdAsync(id);
        if (cat == null)
        {
            return NotFound("id不存在");
        }
        cat.ChangeName(request.Name);
        cat.ChangeCoverUrl(request.CoverUrl);
        return Ok();
    }

    [HttpDelete]
    //[Route("{id}")]
    public async Task<ActionResult> DeleteById(Guid id)
    {
        var cat = await repository.GetCategoryByIdAsync(id);
        if (cat == null)
        {
            //这样做仍然是幂等的，因为“调用N次，确保服务器处于与第一次调用相同的状态。”与响应无关
            return NotFound($"没有Id={id}的Category");
        }
        cat.SoftDelete();//软删除
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> Sort(CategoriesSortRequest req)
    {
        await domainService.SortCategoriesAsync(req.SortedCategoryIds);
        return Ok();
    }
}
