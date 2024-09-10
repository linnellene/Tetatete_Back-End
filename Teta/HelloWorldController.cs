using Microsoft.AspNetCore.Mvc;

namespace TetaBackend;

[Route("api/")]
[ApiController]
public class HelloWorldController : ControllerBase
{
    // GET: /api/
    [HttpGet]
    public ActionResult<string> HelloWorld()
    {
        return Ok("Hello world!");
    }
}

