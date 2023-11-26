using IdentityService.Domain;

namespace IdentityService.WebAPI.Controllers.UserAdmin;
public record UserDTO(Guid Id, string UserName, string PhoneNumber,string Email,bool Lockout, DateTime CreationTime, DateTime? DeletionTime, DateTimeOffset? LockOutEnd)
{
    public static UserDTO Create(User user)
    {
        
        return new UserDTO(user.Id, user.UserName, user.PhoneNumber, user.Email,user.LockoutEnabled, user.CreationTime,user.DeletionTime, user.LockoutEnd);
    }
}