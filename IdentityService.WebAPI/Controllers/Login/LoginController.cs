using IdentityService.Domain;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using reCAPTCHA.AspNetCore;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using Zack.ASPNETCore;
using Zack.Commons;

namespace IdentityService.WebAPI.Controllers.Login;

[Route("[controller]/[action]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IIdRepository repository;
    private readonly IdDomainService idService;
    private readonly IRecaptchaService recaptcha;
    public LoginController(IdDomainService idService, IIdRepository repository, IRecaptchaService recaptcha)
    {
        this.idService = idService;
        this.recaptcha = recaptcha;
        this.repository = repository;
        
    }
    /// <summary>
    /// 初始化系统账号
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CreateWorld(CreateAccountRequest req)
    {
        if (await repository.FindByNameAsync("admin") != null)
        {
            return StatusCode((int)HttpStatusCode.Conflict, "已经初始化过了");
        }
        User user = new User(req.userName);
        var r = await repository.CreateAsync(user, req.password);
        Debug.Assert(r.Succeeded);
        var token = await repository.GenerateChangePhoneNumberTokenAsync(user, req.phoneNum);
        var cr = await repository.ChangePhoneNumAsync(user.Id, req.phoneNum, token);
        Debug.Assert(cr.Succeeded);
        var tlock = await repository.SetLockOut(user, null, false);
        Debug.Assert(tlock.Succeeded);
        r = await repository.AddToRoleAsync(user, "User");
        Debug.Assert(r.Succeeded);
        r = await repository.AddToRoleAsync(user, "Admin");
        Debug.Assert(r.Succeeded);
        return Ok();
    }
    /// <summary>
    /// 创建账户
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CreateAccount(CreateAccountRequest req)
    {
        var isNameExist = await repository.FindByNameAsync(req.userName);
        if (isNameExist != null)
        {
            return StatusCode((int)HttpStatusCode.Conflict, "当前用户名已存在");
        }
        var isPhoneNumExist = await repository.FindByPhoneNumberAsync(req.phoneNum);
        if (isPhoneNumExist != null)
        {
            return StatusCode((int)HttpStatusCode.Conflict, "当前手机号已存在");
        }

        User user = new User(req.userName);
        var r = await repository.CreateAsync(user, req.password);
        Debug.Assert(r.Succeeded);
        var emailToken = await repository.GenerateChangeEmailTokenAsync(user, req.email);
        var er = await repository.ChangeEmailAsync(user.Id, req.email, emailToken);
        var phoneToken = await repository.GenerateChangePhoneNumberTokenAsync(user, req.phoneNum);
        var cr = await repository.ChangePhoneNumAsync(user.Id, req.phoneNum, phoneToken);
        Debug.Assert(cr.Succeeded);
        var tlock = await repository.SetLockOut(user, null, false);
        Debug.Assert(tlock.Succeeded);
        r = await repository.AddToRoleAsync(user, "User");
        Debug.Assert(r.Succeeded);
        return Ok(new {message=$"已成功创建账号{req.userName}，请返回登录吧" });
    }

    [AllowAnonymous]
    [HttpPost]

    public async Task<ActionResult<string>> LoginByPhoneAndPwdReturnAdminToken()
    {
        (var checkResult, var token) = await idService.LoginByUserNameAndPwdAsync("admin", "123456");
        if (checkResult.Succeeded) return token!;
        else if (checkResult.IsLockedOut)//尝试登录次数太多
            return StatusCode((int)HttpStatusCode.Locked, "用户已经被锁定");
        else
        {
            string msg = checkResult.ToString();
            return BadRequest("登录失败" + msg);
        }
    }

    [AllowAnonymous]
    [HttpPost]

    public async Task<ActionResult<string>> LoginByPhoneAndPwdReturnUserToken()
    {
        (var checkResult, var token) = await idService.LoginByUserNameAndPwdAsync("string", "string");
        if (checkResult.Succeeded) return token!;
        else if (checkResult.IsLockedOut)//尝试登录次数太多
            return StatusCode((int)HttpStatusCode.Locked, "用户已经被锁定");
        else
        {
            string msg = checkResult.ToString();
            return BadRequest("登录失败" + msg);
        }
    }



    //[HttpGet]
    //[Authorize]
    //public async Task<ActionResult<UserResponse>> GetUserInfo()
    //{
    //    string userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    var user = await repository.FindByIdAsync(Guid.Parse(userId));
    //    if (user == null)//可能用户注销了
    //    {
    //        return NotFound();
    //    }
    //    //出于安全考虑，不要机密信息传递到客户端
    //    //除非确认没问题，否则尽量不要直接把实体类对象返回给前端
    //    return new UserResponse(user.Id, user.PhoneNumber, user.CreationTime);
    //}

    ////书中的项目只提供根据用户名登录的功能，以及管理员增删改查，像用户主动注册、手机验证码登录等功能都不弄。

    //[AllowAnonymous]
    //[HttpPost]
    //public async Task<ActionResult<string?>> LoginByPhoneAndPwd(LoginByPhoneAndPwdRequest req)
    //{
    //    //todo：要通过行为验证码、图形验证码等形式来防止暴力破解
    //    (var checkResult, string? token) = await idService.LoginByPhoneAndPwdAsync(req.PhoneNum, req.Password);
    //    if (checkResult.Succeeded)
    //    {
    //        return token;
    //    }
    //    else if (checkResult.IsLockedOut)
    //    {
    //        //尝试登录次数太多
    //        return StatusCode((int)HttpStatusCode.Locked, "此账号已经锁定");
    //    }
    //    else
    //    {
    //        string msg = "登录失败";
    //        return StatusCode((int)HttpStatusCode.BadRequest, msg);
    //    }
    //}

    [AllowAnonymous]
    [HttpPost]

    public async Task<ActionResult<LoginResponse>> LoginByUserNameAndPwd(
        LoginByUserNameAndPwdRequest req)
    {
        //这样子接口不好测试，开发环境就先注释掉了
        //var recaptchaReault = await recaptcha.Validate(req.GoogleToken);
        //if (!recaptchaReault.success || recaptchaReault.score < 0.5)
        //{
        //    var response = new LoginResponse { Msg = "登录失败，谷歌验证判定为机器人"  };
        //    return BadRequest(response); // 返回 400 Bad Request 状态码
        //}
        (var checkResult, var token) = await idService.LoginByUserNameAndPwdAsync(req.UserName, req.Password);
        if (checkResult.Succeeded)
        {
            // 登录成功，返回包含 Token 的 DTO
            var response = new LoginResponse { Token = token ,Msg = "登录成功，返回Token"};
            return Ok(response); // 返回 200 OK 状态码
        }
        else if (checkResult.IsLockedOut)//尝试登录次数太多
        {
            // 用户被锁定，返回包含错误消息的 DTO
            var response = new LoginResponse { Msg = "用户已经被锁定" };
            return StatusCode((int)HttpStatusCode.Locked, response); // 返回 423 Locked 状态码
        }
        else
        {
            // 登录失败，返回包含错误消息的 DTO
            string msg = checkResult.ToString();
            var response = new LoginResponse { Msg = "登录失败" + msg };
            return BadRequest(response); // 返回 400 Bad Request 状态码
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> ChangeMyPassword(ChangeMyPasswordRequest req)
    {
        Guid userId = Guid.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
        var resetPwdResult = await repository.ChangePasswordAsync(userId, req.Password);
        if (resetPwdResult.Succeeded)
        {
            return Ok();
            
        }
        else
        {
            return BadRequest(resetPwdResult.Errors.SumErrors());
        }
    }

    
}