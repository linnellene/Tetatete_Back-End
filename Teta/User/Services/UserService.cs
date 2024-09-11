using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.User.Interfaces;
using TetaBackend.User.Utilities;

namespace TetaBackend.User.Services;

public class UserService : IUserService
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

    public async Task<bool> ValidateRegisterParameters(string username, string password)
    {
        return Regex.IsMatch(username, @"^[a-z0-9]+$") &&
               Regex.IsMatch(password, @"^[a-zA-Z0-9]+$") &&
               !await _dataContext.Users.AnyAsync(u => u.Username == username);
    }
}