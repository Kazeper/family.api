using family.api.Data.Interfaces;
using family.api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace family.api.Data.Repos;

public class UserRepo : IUserRepo
{
    private readonly AppDataContext _dataContext;
    private readonly PasswordHasher _hasher;

    public UserRepo(AppDataContext dataContext)
    {
        _dataContext = dataContext;
        _hasher = new PasswordHasher();
    }

    public async Task<bool> ChangePassword(string email, string oldPassword, string newPassword)
    {
        var user = await Get(email);

        if (user == null)
            return false;

        if (!_hasher.Check(user.PasswordHash, oldPassword).Verified)
            return false;

        user.PasswordHash = _hasher.Hash(newPassword);

        return true;
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

    protected sealed class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int iterations = 1000;

        public string Hash(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
              password,
              SaltSize,
              iterations,
              HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{iterations}.{salt}.{key}";
            }
        }

        public (bool Verified, bool NeedsUpgrade) Check(string hash, string password)
        {
            var parts = hash.Split('.', 3);

            if (parts.Length != 3)
            {
                throw new FormatException("Unexpected hash format. " +
                  "Should be formatted as `{iterations}.{salt}.{hash}`");
            }

            var retrievedIterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var needsUpgrade = iterations != retrievedIterations;

            using (var algorithm = new Rfc2898DeriveBytes(
              password,
              salt,
              iterations,
              HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(KeySize);

                var verified = keyToCheck.SequenceEqual(key);

                return (verified, needsUpgrade);
            }
        }
    }
}