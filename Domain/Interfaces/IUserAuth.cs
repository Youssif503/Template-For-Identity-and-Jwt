using JWT_Train.Domain.Models;
using JWT_Train.Application.DTOs;

namespace JWT_Train.Domain.Interfaces
{
    public interface IUserAuth
    {
        Task<AuthModel> RegisterAsync(RegisterUser model);
        Task<AuthModel> GetTokenAsync(LoginUser model);
        Task<string> AddToRole(AddRoleModel model);
    }
}
