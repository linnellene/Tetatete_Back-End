using Microsoft.AspNetCore.Mvc;
using TetaBackend.User.Dto;
using TetaBackend.User.Interfaces;

namespace TetaBackend.User.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // post: api/User/register - registers new account
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] RegisterDto dto)
    {
        var isValid = await _userService.ValidateRegisterParameters(dto.Username, dto.Password);

        if (!isValid)
        {
            return BadRequest("Invalid parameters.");
        }

        var isSaved = await _userService.CreateUser(dto.Username, dto.Password);

        return isSaved ? Created() : BadRequest("Unexpected error.");
    }

}

