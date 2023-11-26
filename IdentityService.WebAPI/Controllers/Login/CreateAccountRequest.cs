namespace IdentityService.WebAPI.Controllers.Login
{
    public record CreateAccountRequest(string userName = "userName", string password = "123", string phoneNum = "17338357782",string email = "1120148291@qq.com");
}
