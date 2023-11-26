
using FluentValidation;
using Microsoft.AspNetCore.Http;


namespace Listening.Admin.WebAPI.Categories;

//启用了<Nullable>enable</Nullable>，所以string ChineseName就是非可空，会自动校验
public record CategoryAddRequest(string Chinese, string English, IFormFile ImageFile);
public class CategoryAddRequestValidator : AbstractValidator<CategoryAddRequest>
{
    public CategoryAddRequestValidator()
    {
        RuleFor(x => x.Chinese).NotNull().Length(1, 200);
        RuleFor(x => x.English).NotNull().Length(1, 200);
        RuleFor(x => x.ImageFile).NotNull();//CoverUrl允许为空
    }
}