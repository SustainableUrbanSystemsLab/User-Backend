using Urbano_API.Models;

namespace Urbano_API.DTOs;

public class UserDTO
{
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public User GetUser()
    {
        User user = new User();

        user.UserName = this.UserName;
        user.Password = this.Password;
        user.FirstName = this.FirstName;
        user.LastName = this.LastName;

        return user;
    }
}
