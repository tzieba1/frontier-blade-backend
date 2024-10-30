using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly TeamRepository _teamRepository;

    public TeamController(TeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Team>>> Get() =>
        await _teamRepository.GetTeamsAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Team>> GetById(string id)
    {
        var team = await _teamRepository.GetTeamAsync(id);
        return team == null ? NotFound() : Ok(team);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Team newTeam)
    {
        await _teamRepository.CreateTeamAsync(newTeam);
        return CreatedAtAction(nameof(GetById), new { id = newTeam.Id }, newTeam);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Team updatedTeam)
    {
        var updated = await _teamRepository.UpdateTeamAsync(id, updatedTeam);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _teamRepository.DeleteTeamAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
