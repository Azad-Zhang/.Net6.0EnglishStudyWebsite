using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain
{
    public interface IIdRepository
    {
        Task<User?> FindByIdAsync(Guid userId);//根据Id获取用户
        Task<User?> FindByNameAsync(string userName);//根据用户名获取用户
        Task<User?> FindByPhoneNumberAsync(string phoneNum);//根据手机号获取用户
        Task<IdentityResult> CreateAsync(User user, string password);//创建用户
        Task<IdentityResult> AccessFailedAsync(User user);//记录一次登陆失败

        /// <summary>
        /// 生成重置手机号的令牌
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        Task<string> GenerateChangePhoneNumberTokenAsync(User user, string phoneNumber);
        /// <summary>
        /// 生成重置邮箱的令牌
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        Task<string> GenerateChangeEmailTokenAsync(User user, string email);
        /// <summary>
        /// 检查VCode，然后设置用户手机号为phoneNum
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="phoneNum"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<SignInResult> ChangePhoneNumAsync(Guid userId, string phoneNum, string token);
        /// <summary>
        /// 检查VCode，然后设置用户邮箱为email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="phoneNum"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<SignInResult> ChangeEmailAsync(Guid userId, string email, string token);
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<IdentityResult> ChangePasswordAsync(Guid userId, string password);
        

        /// <summary>
        /// 获取用户的角色
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IList<string>> GetRolesAsync(User user);

        /// <summary>
        /// 把用户user加入角色role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<IdentityResult> AddToRoleAsync(User user, string role);
        /// <summary>
        /// 为了登录而检查用户名、密码是否正确
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="lockoutOnFailure">如果登录失败，则记录一次登陆失败</param>
        /// <returns></returns>
        public Task<SignInResult> CheckForSignInAsync(User user, string password, bool lockoutOnFailure);
        /// <summary>
        /// 确认手机号
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task ConfirmPhoneNumberAsync(Guid id, string phoneNum);

        /// <summary>
        /// 修改手机号
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task UpdatePhoneNumberAsync(Guid id, string phoneNum);
        /// <summary>
        /// 确认邮箱
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task ConfirmEmailAsync(Guid id, string email);

        /// <summary>
        /// 修改邮箱
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task UpdateEmailAsync(Guid id, string email);
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<IdentityResult> RemoveUserAsync(Guid id);

        /// <summary>
        /// 添加管理员
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="phoneNum"></param>
        /// <returns>返回值第三个是生成的密码</returns>
        public Task<(IdentityResult, User?, string? password)> AddAdminUserAsync(string userName, string phoneNum,string email);

        /// <summary>
        /// 重置密码。
        /// </summary>
        /// <param name="id"></param>
        /// <returns>返回值第三个是生成的密码</returns>
        public Task<(IdentityResult, User?, string? password)> ResetPasswordAsync(Guid id);
        /// <summary>
        /// 手动设置是否锁定
        /// </summary>
        /// <param name="user"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public Task<IdentityResult> SetLockOut(User user, DateTimeOffset? date,bool IsLock);


    }
}
