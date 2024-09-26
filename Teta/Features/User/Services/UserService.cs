using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Domain.Entities.CategoryInfo;
using TetaBackend.Features.Interfaces;
using TetaBackend.Features.User.Dto;
using TetaBackend.Features.User.Dto.Category;
using TetaBackend.Features.User.Enums;
using TetaBackend.Features.User.Interfaces;
using TetaBackend.Features.User.Utilities;

namespace TetaBackend.Features.User.Services;

public class UserService : IUserService
{
    private readonly DataContext _dataContext;
    private readonly IImageService _imageService;
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly string _emaiLRedirectUrl;

    public UserService(DataContext dataContext, IImageService imageService, IEmailService emailService,
        IConfiguration configuration, IJwtService jwtService)
    {
        _dataContext = dataContext;
        _imageService = imageService;
        _emailService = emailService;
        _jwtService = jwtService;

        var redirectUrl = configuration.GetSection("EmailRestoreLink").Value;

        _emaiLRedirectUrl = redirectUrl ?? throw new ArgumentException("Invalid email restore link");
    }

    public async Task<IEnumerable<GenderEntity>> GetAllGenders()
    {
        return await _dataContext.Genders.ToListAsync();
    }

    public async Task<IEnumerable<LocationEntity>> GetAllLocations()
    {
        return await _dataContext.Locations.ToListAsync();
    }

    public async Task<IEnumerable<LanguageEntity>> GetAllLanguages()
    {
        return await _dataContext.Languages.ToListAsync();
    }

    public async Task CreateUser(string email, string phone, string password)
    {
        await ValidateAuthParameters(email, phone, password);

        var hashedPassword = PasswordHasher.HashPassword(password);

        var user = new UserEntity
        {
            Email = email,
            Phone = phone,
            Password = hashedPassword
        };

        await _dataContext.Users.AddAsync(user);
        await _dataContext.SaveChangesAsync();
    }

    public async Task<UserEntity?> Authenticate(string password, string? email, string? phone)
    {
        if (email is null && phone is null)
        {
            throw new ArgumentException("Email and phone cannot be null at the same time.");
        }

        if (email is not null && phone is not null)
        {
            throw new ArgumentException("Email and phone cannot be non-nullable at the same time.");
        }

        await ValidateAuthParameters(email, phone, password, true);

        var user = await _dataContext.Users.FirstOrDefaultAsync(
            u => phone == null ? u.Email == email : u.Phone == phone);

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
            .Include(u => u.Images)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<UserInfoEntity> FillInformation(Guid userId, FillUserInfoDto dto)
    {
        var imageUrl = await _imageService.UploadImage(dto.Images);

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
        };

        var userInfoEntity = await _dataContext.UserInfos.AddAsync(userInfo);

        var imagesToSave = imageUrl.Select(i => new ImageEntity
        {
            UserInfoId = userInfoEntity.Entity.Id,
            Url = i
        });

        await _dataContext.Images.AddRangeAsync(imagesToSave);

        foreach (var languageId in dto.Languages)
        {
            await _dataContext.UserInfoLanguages.AddAsync(new UserInfoLanguageEntity
            {
                LanguageId = languageId,
                UserInfoId = userInfoEntity.Entity.Id,
            });
        }

        var user = (await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId))!;

        user.UserInfoId = userInfoEntity.Entity.Id;

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

        if (dto.Images is not null)
        {
            userInfo.Images = new List<ImageEntity>();

            var imageUrl = await _imageService.UploadImage(dto.Images);

            var imagesToSave = imageUrl.Select(i => new ImageEntity
            {
                UserInfoId = userInfo.Id,
                Url = i
            });

            await _dataContext.Images.AddRangeAsync(imagesToSave);
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

    public async Task<CategoryType?> GetFulfilledInfoType(Guid userId)
    {
        var user = await _dataContext.Users.Include(u => u.FriendsCategoryInfo).Include(u => u.LoveCategoryInfo)
            .Include(u => u.WorkCategoryInfo).FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return null;
        }

        if (user.LoveCategoryInfo is not null)
        {
            return CategoryType.Love;
        }

        if (user.FriendsCategoryInfo is not null)
        {
            return CategoryType.Friends;
        }

        if (user.WorkCategoryInfo is not null)
        {
            return CategoryType.Work;
        }

        return null;
    }

    public async Task<TCategory?> GetCategoryInfo<TCategory>(Guid userId) where TCategory : class, ICategory
    {
        TCategory? info;

        if (typeof(TCategory) == typeof(WorkCategoryInfoEntity))
        {
            info = await _dataContext.WorkCategoryInfos.FirstOrDefaultAsync(w => w.UserId == userId) as TCategory;
        }
        else if (typeof(TCategory) == typeof(FriendsCategoryInfoEntity))
        {
            info = await _dataContext.FriendsCategoryInfos.FirstOrDefaultAsync(f => f.UserId == userId) as TCategory;
        }
        else
        {
            info = await _dataContext.LoveCategoryInfos.FirstOrDefaultAsync(l => l.UserId == userId) as TCategory;
        }

        return info;
    }

    public async Task FillCategoryInfo<TCategory>(Guid userId, TCategory info)
        where TCategory : class, ICategory
    {
        var existingCategoryType = await GetFulfilledInfoType(userId);

        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (existingCategoryType is not null)
        {
            throw new ArgumentException("Info already exists.");
        }

        if (user is null)
        {
            throw new ArgumentException("Invalid user id.");
        }

        if (typeof(TCategory) == typeof(WorkCategoryInfoEntity))
        {
            var parsedInfo = (info as WorkCategoryInfoEntity)!;

            ValidateWorkCategoryInfo(parsedInfo.Info, parsedInfo.Income, parsedInfo.Skills);

            var entity = await _dataContext.WorkCategoryInfos.AddAsync(parsedInfo);
            user.WorkCategoryInfoId = entity.Entity.Id;
        }
        else if (typeof(TCategory) == typeof(FriendsCategoryInfoEntity))
        {
            ValidateFriendsCategoryInfo(info.Info);

            var entity = await _dataContext.FriendsCategoryInfos.AddAsync((info as FriendsCategoryInfoEntity)!);
            user.FriendsCategoryInfoId = entity.Entity.Id;
        }
        else if (typeof(TCategory) == typeof(LoveCategoryInfoEntity))
        {
            var parsedInfo = (info as LoveCategoryInfoEntity)!;

            await ValidateLoveCategoryInfo(parsedInfo.GenderId, parsedInfo.Info, parsedInfo.MinAge, parsedInfo.MaxAge);

            var entity = await _dataContext.LoveCategoryInfos.AddAsync(parsedInfo);
            user.LoveCategoryInfoId = entity.Entity.Id;
        }

        await _dataContext.SaveChangesAsync();
    }

    public async Task UpdateCategoryInfo<TCategory>(Guid infoId, TCategory? info)
        where TCategory : class, ICategory
    {
        if (typeof(TCategory) == typeof(UpdateWorkCategoryInfoDto))
        {
            var parsedInfo = (info as UpdateWorkCategoryInfoDto)!;

            var existingInfo = await _dataContext.WorkCategoryInfos.FirstOrDefaultAsync(w => w.Id == infoId);

            if (existingInfo is null)
            {
                throw new ArgumentException("Category info not found.");
            }

            ValidateWorkCategoryInfo(parsedInfo.Info, parsedInfo.Income, parsedInfo.Skills);

            existingInfo.Info = parsedInfo.Info ?? existingInfo.Info;
            existingInfo.Skills = parsedInfo.Skills ?? existingInfo.Skills;
            existingInfo.Income = parsedInfo.Income ?? existingInfo.Income;
            existingInfo.LookingFor = parsedInfo.LookingFor ?? existingInfo.LookingFor;
        }
        else if (typeof(TCategory) == typeof(UpdateFriendsCategoryInfoDto))
        {
            var parsedInfo = (info as UpdateFriendsCategoryInfoDto)!;

            var existingInfo = await _dataContext.FriendsCategoryInfos.FirstOrDefaultAsync(f => f.Id == infoId);

            if (existingInfo is null)
            {
                throw new ArgumentException("Category info not found.");
            }

            ValidateFriendsCategoryInfo(parsedInfo.Info);

            existingInfo.Info = parsedInfo.Info ?? existingInfo.Info;
        }
        else if (typeof(TCategory) == typeof(UpdateLoveCategoryInfoDto))
        {
            var parsedInfo = (info as UpdateLoveCategoryInfoDto)!;

            var existingInfo = await _dataContext.LoveCategoryInfos.FirstOrDefaultAsync(l => l.Id == infoId);

            if (existingInfo is null)
            {
                throw new ArgumentException("Category info not found.");
            }

            await ValidateLoveCategoryInfo(parsedInfo.GenderId, parsedInfo.Info, parsedInfo.MinAge, parsedInfo.MaxAge);

            existingInfo.Info = parsedInfo.Info ?? existingInfo.Info;
            existingInfo.GenderId = parsedInfo.GenderId ?? existingInfo.GenderId;
            existingInfo.MaxAge = parsedInfo.MaxAge ?? existingInfo.MaxAge;
            existingInfo.MinAge = parsedInfo.MinAge ?? existingInfo.MinAge;
        }

        await _dataContext.SaveChangesAsync();
    }

    public async Task DeleteCategoryInfo(CategoryType type, Guid userId)
    {
        switch (type)
        {
            case CategoryType.Work:
            {
                var infoToDelete = await _dataContext.WorkCategoryInfos.FirstOrDefaultAsync(w => w.UserId == userId);

                if (infoToDelete is null)
                {
                    throw new ArgumentException("Info doesn't exist");
                }

                _dataContext.WorkCategoryInfos.Remove(infoToDelete);
                break;
            }
            case CategoryType.Friends:
            {
                var infoToDelete = await _dataContext.FriendsCategoryInfos.FirstOrDefaultAsync(f => f.UserId == userId);

                if (infoToDelete is null)
                {
                    throw new ArgumentException("Info doesn't exist");
                }

                _dataContext.FriendsCategoryInfos.Remove(infoToDelete);
                break;
            }
            default:
            {
                var infoToDelete = await _dataContext.LoveCategoryInfos.FirstOrDefaultAsync(l => l.UserId == userId);

                if (infoToDelete is null)
                {
                    throw new ArgumentException("Info doesn't exist");
                }

                _dataContext.LoveCategoryInfos.Remove(infoToDelete);
                break;
            }
        }

        await _dataContext.SaveChangesAsync();
    }

    public async Task SendForgotPasswordEmail(string email)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            throw new ArgumentException("Invalid email.");
        }

        var token =  _jwtService.GenerateToken(user.Id);
        var link = _emaiLRedirectUrl + "?token=" + token;

        var message = $"Tetatet App. Link to restore password: {link}";

        await _emailService.SendEmail(user.Email, message);
    }

    public async Task UpdatePassword(Guid userId, string password)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            throw new ArgumentException("Invalid user id.");
        }
        
        await ValidateAuthParameters(null, null, password);

        var hashedPassword = PasswordHasher.HashPassword(password);

        user.Password = hashedPassword;

        await _dataContext.SaveChangesAsync();
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

        var isValidFullNameRegex = Regex.IsMatch(fullName ?? "", @"^[a-zA-Z ]+$");

        if (fullName is not null && (fullName.Trim().Count(c => !char.IsWhiteSpace(c)) < 3 || !isValidFullNameRegex))
        {
            throw new ArgumentException(
                "Full name should contain more than 3 symbols and should not contain numbers and special symbols.");
        }

        var isValidAboutRegex = Regex.IsMatch(about ?? "", @"^[a-z0-9., ]+$");

        if (about is not null && (about.Trim().Count(c => !char.IsWhiteSpace(c)) < 10 || !isValidAboutRegex))
        {
            throw new ArgumentException(
                "About should contain more than 10 symbols and should not contain special symbols.");
        }
    }

    private async Task ValidateLoveCategoryInfo(Guid? preferableGenderId, string? relationshipGoals, int? minAge,
        int? maxAge)
    {
        if (preferableGenderId is not null && !await _dataContext.Genders.AnyAsync(l => l.Id == preferableGenderId))
        {
            throw new ArgumentException("Invalid gender.");
        }

        var isValidRelationshipGoalsRegex = Regex.IsMatch(relationshipGoals ?? "", @"^[a-zA-Z ,.]+$");

        if (relationshipGoals is not null &&
            !(relationshipGoals.Length is >= 10 and <= 1000 && isValidRelationshipGoalsRegex))
        {
            throw new ArgumentException(
                "Relationship goals should be in latin, without special signs, min 10 symbols and max 1000 symbols.");
        }

        if ((minAge is not null && maxAge is null) || (minAge is null && maxAge is not null) || minAge >= maxAge)
        {
            throw new ArgumentException("Invalid minAge and maxAge.");
        }

        if (minAge is < 18 or > 98)
        {
            throw new ArgumentException("Min age should be between 18 and 98");
        }

        if (maxAge is < 19 or > 99)
        {
            throw new ArgumentException("Max age should be between 19 and 99");
        }
    }

    private void ValidateWorkCategoryInfo(string? occupation, int? income, string? skills)
    {
        var isValidOccupationRegex = Regex.IsMatch(occupation ?? "", @"^[a-zA-Z ,.]+$");

        if (occupation is not null && !(occupation.Length is >= 3 and <= 120 && isValidOccupationRegex))
        {
            throw new ArgumentException(
                "Occupation should be in latin, without special signs, min 3 symbols and max 120 symbols.");
        }

        if (income is < 1 or > 999_999_999)
        {
            throw new ArgumentException(
                "Income should be between 1 and 999,999,999.");
        }

        var isValidSkillsRegex = Regex.IsMatch(skills ?? "", @"^[a-zA-Z ,.]+$");

        // TODO: clarify about skills in DB
        if (skills is not null && !(skills.Length is >= 3 and <= 120 && isValidSkillsRegex))
        {
            throw new ArgumentException(
                "Skills should be in latin, without special signs, min 3 symbols and max 120 symbols.");
        }
    }

    private void ValidateFriendsCategoryInfo(string? aboutMe)
    {
        var isValidRegex = Regex.IsMatch(aboutMe ?? "", @"^[a-zA-Z ,.]+$");

        if (aboutMe is not null && !(aboutMe.Length is >= 10 and <= 1000 && isValidRegex))
        {
            throw new ArgumentException(
                "About me should be in latin, without special signs, min 10 symbols and max 1000 symbols.");
        }
    }


    private async Task ValidateAuthParameters(string? email, string? phone, string password, bool logIn = false)
    {
        if (email is not null && !Regex.IsMatch(email, @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$"))
        {
            throw new ArgumentException("Invalid email.");
        }

        if (phone is not null && !Regex.IsMatch(phone, @"^\+1-\d{3}-\d{3}-\d{4}$"))
        {
            throw new ArgumentException("Invalid phone.");
        }

        if (!Regex.IsMatch(password, @"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+$"))
        {
            throw new ArgumentException("Password must be in latin, without spaces.");
        }

        if (!logIn && email is not null && await _dataContext.Users.AnyAsync(u => u.Email == email))
        {
            throw new ArgumentException("Email already exists");
        }

        if (!logIn && phone is not null && await _dataContext.Users.AnyAsync(u => u.Phone == phone))
        {
            throw new ArgumentException("Phone already exists");
        }
    }
}