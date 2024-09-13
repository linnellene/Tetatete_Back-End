using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Features.User.Interfaces;
using TetaBackend.Features.User.Utilities;

namespace TetaBackend.Features.User.Services;

public class UserService: IUserService
{
    private readonly DataContext _dataContext;
    private readonly ILogger _logger;

    public UserService(DataContext dataContext, ILogger<UserService> logger)
    {
        _dataContext = dataContext;
        _logger = logger;
    }

    public async Task<bool> CreateUser(string username, string password)
    {
        try
        {
            var hashedPassword = PasswordHasher.HashPassword(password);

            var user = new UserEntity
            {
                Username = username,
                Password = hashedPassword
            };

            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception e)
        {
            _logger.LogInformation($"User creation failed, error: {e.Message}");

            return false;
        }
    }

    public async Task ValidateRegisterParameters(string username, string password, bool logIn = false)
    {
        if (!Regex.IsMatch(username, @"^[a-z0-9]+$"))
        {
            throw new ArgumentException("Username must be in latin, in lower case, without spaces and special signs.");
        }

        if (!Regex.IsMatch(password, @"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+$"))
        {
            throw new ArgumentException("Password must be in latin, without spaces.");
        }

        if (!logIn && await _dataContext.Users.AnyAsync(u => u.Username == username))
        {
            throw new ArgumentException("Username already exists");
        }
    }

    public async Task<UserEntity?> Authenticate(string username, string password)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
        {
            return null;
        }

        return PasswordHasher.VerifyPassword(password, user.Password) ? user : null;
    }
}