using IdentityService.Domain;
using IdentityService.Infrastructure;
using IdentityService.WebAPI.Controllers.Login;
using IdentityService.WebAPI.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Zack.Commons;
using Zack.EventBus;

namespace IdentityService.WebAPI.Controllers.UserAdmin;

[Route("[controller]/[action]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserAdminController : ControllerBase
{
    private readonly IdUserManager userManager;
    private readonly IIdRepository repository;
    private readonly IEventBus eventBus;

    public UserAdminController(IdUserManager userManager, IEventBus eventBus, IIdRepository repository)
    {
        this.userManager = userManager;
        this.eventBus = eventBus;
        this.repository = repository;
    }

    [HttpGet]
    public Task<UserDTO[]> FindAllUsers()
    {
        var allUsers= userManager.Users.Select(u => UserDTO.Create(u)).ToArrayAsync();
        return allUsers;
    }

    [HttpPut]
    public async Task<ActionResult<NoticeResponse>> LockOut(string id,bool isLock)
    {
        var user = await userManager.FindByIdAsync(id);
        var result = new IdentityResult();
        if (isLock == true)
        {
            result = await repository.SetLockOut(user, DateTimeOffset.Now.AddYears(100),isLock);

        }
        else
        {
            result = await repository.SetLockOut(user, null, isLock);
        }
        if(result.Succeeded)
        {
            var response = new NoticeResponse { Code=200, Msg = "设置成功" };
            return response;
        }
        else
        {
            var response = new NoticeResponse { Code = 400, Msg = "未设置成功" };
            return response;
        }
        
    }

    [HttpGet]
    //[Route("{id}")]
    public async Task<ActionResult<UserDTO>> FindById(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if(user == null)
        {
            return BadRequest("找不到用户"+id);
        }
        return UserDTO.Create(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDTO>> ResetInfo(ResetInfoRequest req)
    {
        var user = await userManager.FindByIdAsync(req.Id);
        if (user == null)
        {
            return BadRequest("找不到用户" + req.Id);
        }
        var emailToken = await userManager.GenerateChangeEmailTokenAsync(user, req.Email);
        var emailResult = await userManager.ChangeEmailAsync(user, req.Email, emailToken);
        var phoneToken = await userManager.GenerateChangePhoneNumberTokenAsync(user, req.PhoneNum);
        var phoneResult = await userManager.ChangePhoneNumberAsync(user,req.PhoneNum, phoneToken);
        if(emailResult.Succeeded && phoneResult.Succeeded)
        {
            return UserDTO.Create(user);
        }
        else
        {
            return BadRequest("设置失败");
        }
    }
    [HttpPost]
    public async Task<ActionResult> CreateAdminUser(CreateAccountRequest req)
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
        r = await repository.AddToRoleAsync(user, "Admin");
        Debug.Assert(r.Succeeded);
        return Ok(new { message = $"已成功创建账号{req.userName}，请返回登录吧" });
    }

    //[Authorize(Roles = "Admin")]
    //[HttpPost]
    //public async Task<ActionResult> AddAdminUser(AddAdminUserRequest req)
    //{
    //    (var result, var user, var password) = await repository
    //        .AddAdminUserAsync(req.UserName, req.PhoneNum,req.Email);
    //    if (!result.Succeeded)
    //    {
    //        return BadRequest(result.Errors.SumErrors());
    //    }
    //    //生成的密码短信发给对方
    //    //可以同时或者选择性的把新增用户的密码短信/邮件/打印给用户
    //    //体现了领域事件对于代码“高内聚、低耦合”的追求
    //    var userCreatedEvent = new UserCreatedEvent(user.Id, req.UserName, password, req.PhoneNum,req.Email);
    //    eventBus.Publish("IdentityService.User.Created", userCreatedEvent);
    //    return Ok();
    //}

    [HttpDelete]
    //[Route("{id}")]
    public async Task<ActionResult> DeleteAdminUser(Guid id)
    {
        await repository.RemoveUserAsync(id);
        return Ok();
    }

    //[HttpPut]
    //[Route("{id}")]
    //public async Task<ActionResult> UpdateAdminUser(Guid id, EditAdminUserRequest req)
    //{
    //    var user = await repository.FindByIdAsync(id);
    //    if (user == null)
    //    {
    //        return NotFound("用户没找到");
    //    }
    //    user.PhoneNumber = req.PhoneNum;
    //    await userManager.UpdateAsync(user);
    //    return Ok();
    //}

    [HttpPost]
    //[Route("{id}")]
    public async Task<ActionResult> ResetAdminUserPassword(Guid id)
    {
        (var result, var user, var password) = await repository.ResetPasswordAsync(id);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.SumErrors());
        }
        //生成的密码邮件发给对方
        var eventData = new ResetPasswordEvent(user.Id, user.UserName, password, user.Email);
        eventBus.Publish("IdentityService.User.PasswordReset", eventData);
        return Ok("已成功发送");
    }
    
    [HttpPost]
    public async Task<ActionResult> PostEmailTest()
    {
        var eventData = new ResetPasswordEvent(Guid.NewGuid(), "用户名", "123456", "");
        eventBus.Publish("IdentityService.User.PasswordReset", eventData);
        return Ok();
    }
}