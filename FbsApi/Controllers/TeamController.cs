using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TeamController : BaseController<Team>
{
    private readonly TeamRepository _teamRepository;

    public TeamController(TeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Team>>> GetAll() 
    {
        var result = await _teamRepository.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Team>> GetById(Guid id)
    {
        var result = await _teamRepository.GetByIdAsync(id);
        return result == null || result.Data == null ? NotFound() : Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Team newTeam)
    {
        Guid userId = Guid.TryParse(User?.Identity?.Name, out var parsedId)
            ? parsedId
            : Guid.Empty;
        
        // Check if the user is authenticated
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        SetCreateFields(newTeam, userId);
        await _teamRepository.CreateAsync(newTeam);
        return CreatedAtAction(nameof(GetById), new { id = newTeam.Id }, newTeam);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, Team updatedTeam)
    {
        Guid userId = Guid.TryParse(User?.Identity?.Name, out var parsedId)
            ? parsedId
            : Guid.Empty;

        // Check if the user is authenticated
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var teamResult = await _teamRepository.GetByIdAsync(id);
        
        // Check if the team exists
        if (teamResult == null || teamResult.Data == null)
        {
            return NotFound();
        }

        SetUpdateFields(teamResult.Data, userId);
        teamResult.Data.Name = updatedTeam.Name;
        teamResult.Data.Members = updatedTeam.Members;

        var result = await _teamRepository.UpdateAsync(id, teamResult.Data);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _teamRepository.DeleteAsync(id);
        return result.Success ? NoContent() : NotFound();
    }
}
