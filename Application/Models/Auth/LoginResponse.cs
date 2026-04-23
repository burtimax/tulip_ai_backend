using Infrastructure.Db.App.Entities;

namespace Application.Models.Auth
{
    public class LoginResponse
    {
        public UserEntity User { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
