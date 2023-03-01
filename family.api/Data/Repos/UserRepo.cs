using family.api.Data.Interfaces;
using family.api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace family.api.Data.Repos;

public class UserRepo : IUserRepo
{
    private readonly AppDataContext _dataContext;

    public UserRepo(AppDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<User> Get(string email)
    {
        return await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
        ////return await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower() && _hasher.Check(u.PasswordHash, password).Verified);
    }

    public async Task<User> Update(User user)
    {
        var existingUser = await _dataContext.Users.FirstOrDefaultAsync(x => x.Email == user.Email);

        if (existingUser != null)
        {
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.PasswordHash = user.PasswordHash;
        }

        return existingUser;
    }

    public async Task Register(User user)
    {
        if (user == null)
            throw new NullReferenceException(nameof(user));

        await _dataContext.Users.AddAsync(user);
    }

    public async Task SaveChanges()
    {
        await _dataContext.SaveChangesAsync();
    }
}