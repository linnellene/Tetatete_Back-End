using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Features.User.Dto;
using TetaBackend.Features.User.Interfaces;
using TetaBackend.Features.User.Utilities;

namespace TetaBackend.Features.User.Services;

public class UserService : IUserService
{
    private readonly DataContext _dataContext;
    private readonly ILogger _logger;
    private readonly IImageService _imageService;

    public UserService(DataContext dataContext, ILogger<UserService> logger, IImageService imageService)
    {
        _dataContext = dataContext;
        _logger = logger;
        _imageService = imageService;
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

    public Task<UserInfoEntity?> GetUserInfo(Guid userId)
    {
        return _dataContext.UserInfos
            .Include(u => u.UserInfoLanguages)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<UserInfoEntity> FillInformation(Guid userId, FillUserInfoDto dto)
    {
        var imageUrl = await _imageService.UploadImage(dto.Image);

        await ValidateUserInfo(dto.PlaceOfBirthId, dto.LocationId, dto.GenderId, dto.Languages, dto.FullName,
            dto.About, dto.Age);

        var userInfo = new UserInfoEntity
        {
            UserId = userId,
            PlaceOfBirthId = dto.PlaceOfBirthId,
            LocationId = dto.LocationId,
            About = dto.About.Trim(),
            FullName = dto.FullName.Trim(),
            Age = dto.Age,
            GenderId = dto.GenderId,
            ImageUrl = imageUrl,
        };

        var userInfoEntity = await _dataContext.UserInfos.AddAsync(userInfo);

        foreach (var languageId in dto.Languages)
        {
            await _dataContext.UserInfoLanguages.AddAsync(new UserInfoLanguageEntity
            {
                LanguageId = languageId,
                UserInfoId = userInfoEntity.Entity.Id,
            });
        }

        await _dataContext.SaveChangesAsync();

        return userInfoEntity.Entity;
    }

    public async Task<UserInfoEntity> UpdateInformation(Guid userId, UpdateUserInfoDto dto)
    {
        await ValidateUserInfo(dto.PlaceOfBirthId, dto.LocationId, dto.GenderId, dto.Languages, dto.FullName,
            dto.About, dto.Age);

        var userInfo = await _dataContext.UserInfos.Include(ui => ui.UserInfoLanguages).ThenInclude(u => u.Language)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (userInfo is null)
        {
            throw new ArgumentException("Invalid user id");
        }

        if (dto.Image is not null)
        {
            var imageUrl = await _imageService.UploadImage(dto.Image);

            userInfo.ImageUrl = imageUrl;
        }

        userInfo.PlaceOfBirthId = dto.PlaceOfBirthId ?? userInfo.PlaceOfBirthId;
        userInfo.GenderId = dto.GenderId ?? userInfo.GenderId;
        userInfo.LocationId = dto.LocationId ?? userInfo.LocationId;
        userInfo.About = dto.About ?? userInfo.About;
        userInfo.Age = dto.Age ?? userInfo.Age;
        userInfo.FullName = dto.FullName ?? userInfo.FullName;

        if (dto.Languages is not null)
        {
            _dataContext.UserInfoLanguages.RemoveRange(userInfo.UserInfoLanguages);

            userInfo.UserInfoLanguages = dto.Languages.Select(languageId => new UserInfoLanguageEntity
            {
                UserInfoId = userInfo.Id,
                LanguageId = languageId
            }).ToList();
        }

        await _dataContext.SaveChangesAsync();

        return userInfo;
    }

    private async Task ValidateUserInfo(Guid? placeOfBirthId, Guid? locationId, Guid? genderId,
        IEnumerable<Guid>? languages,
        string? fullName, string? about, int? age)
    {
        if (age is < 18 or > 100)
        {
            throw new ArgumentException("Age should be more than 18 and less than 100.");
        }

        if (placeOfBirthId is not null && !await _dataContext.Locations.AnyAsync(l => l.Id == placeOfBirthId))
        {
            throw new ArgumentException("Invalid place of birth location.");
        }

        if (locationId is not null && !await _dataContext.Locations.AnyAsync(l => l.Id == locationId))
        {
            throw new ArgumentException("Invalid current location.");
        }

        if (genderId is not null && !await _dataContext.Genders.AnyAsync(l => l.Id == genderId))
        {
            throw new ArgumentException("Invalid gender.");
        }

        if (languages is not null && _dataContext.Languages.Count(l => languages.Contains(l.Id)) != languages.Count())
        {
            throw new ArgumentException("Invalid languages.");
        }

        if (fullName is not null &&
            (fullName.Trim().Count(c => !char.IsWhiteSpace(c)) < 3 || !Regex.IsMatch(fullName, @"^[a-zA-Z ]+$")))
        {
            throw new ArgumentException(
                "Full name should contain more than 3 symbols and should not contain numbers and special symbols.");
        }

        if (about is not null && (about.Trim().Count(c => !char.IsWhiteSpace(c)) < 10 ||
                                  !Regex.IsMatch(about, @"^[a-z0-9., ]+$")))
        {
            throw new ArgumentException(
                "About should contain more than 10 symbols and should not contain special symbols.");
        }
    }
}