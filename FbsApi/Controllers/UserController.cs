using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController<User>
{
  private readonly UserRepository _userRepository;

  public UserController(UserRepository userRepository)
  {
    _userRepository = userRepository;
  }

  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<ActionResult<List<User>>> GetAll()
  {
    var result = await _userRepository.GetAllAsync();
    return Ok(result.Data);
  }

  [HttpGet("{id:guid}")]
  [Authorize]
  public async Task<ActionResult<User>> GetById(Guid id)
  {
    var result = await _userRepository.GetByIdAsync(id);
    return result == null || result.Data == null ? NotFound() : Ok(result.Data);
  }

  [HttpPost]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Create(User newUser)
  {
    Guid userId = Guid.TryParse(User?.Identity?.Name, out var parsedId)
            ? parsedId
            : Guid.Empty;

    // Check if the user is authenticated
    if (userId == Guid.Empty)
    {
      return Unauthorized();
    }

    SetCreateFields(newUser, userId);
    await _userRepository.CreateAsync(newUser);
    return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
  }

  [HttpPut("{id:guid}")]
  [Authorize]
  public async Task<IActionResult> Update(Guid id, User updatedUser)
  {
    Guid userId = Guid.TryParse(User?.Identity?.Name, out var parsedId)
            ? parsedId
            : Guid.Empty;

    // Check if the user is authenticated
    if (userId == Guid.Empty)
    {
      return Unauthorized();
    }

    var user = await _userRepository.GetByIdAsync(id);

    // Check if the user exists
    if (user == null || user.Data == null)
    {
      return NotFound();
    }

    SetUpdateFields(user.Data, userId);
    user.Data.Email = updatedUser.Email;
    user.Data.FirstName = updatedUser.FirstName;
    user.Data.LastName = updatedUser.LastName;
    user.Data.Roles = updatedUser.Roles;

    var result = await _userRepository.UpdateAsync(id, user.Data);
    return result.Success ? NoContent() : NotFound();
  }

  [HttpDelete("{id:guid}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Delete(Guid id)
  {
    var result = await _userRepository.DeleteAsync(id);
    return result.Success ? NoContent() : NotFound();
  }
}
