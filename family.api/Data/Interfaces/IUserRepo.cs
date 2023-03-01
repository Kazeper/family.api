using family.api.Models;

namespace family.api.Data.Interfaces;

public interface IUserRepo
{
    Task<User> Get(string email);

    Task Register(User user);

    Task<User> Update(User user);

    Task SaveChanges();
}