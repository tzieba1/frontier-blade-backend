using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TimeSheetController : BaseController<TimeSheet>
{
    private readonly TimeSheetRepository _timeSheetRepository;

    public TimeSheetController(TimeSheetRepository timeSheetRepository)
    {
        _timeSheetRepository = timeSheetRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin, Supervisor")]
    public async Task<ActionResult<List<TimeSheet>>> GetAll()
    {
        var result = await _timeSheetRepository.GetAllAsync();
        return  Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin, Employee, Supervisor")]
    public async Task<ActionResult<TimeSheet>> GetById(Guid id)
    {
        var result = await _timeSheetRepository.GetByIdAsync(id);
        return result == null || result.Data == null ? NotFound() : Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, Employee, Supervisor")]
    public async Task<IActionResult> Create(TimeSheet newTimeSheet)
    {
        Guid userId = Guid.TryParse(User?.Identity?.Name, out var parsedId)
            ? parsedId
            : Guid.Empty;

        // Check if the user is authorized
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        SetCreateFields(newTimeSheet, userId);
        await _timeSheetRepository.CreateAsync(newTimeSheet);
        return CreatedAtAction(nameof(GetById), new { id = newTimeSheet.Id }, newTimeSheet);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin, Employee, Supervisor")]
    public async Task<IActionResult> Update(Guid id, TimeSheet updatedTimeSheet)
    {
        Guid userId = Guid.TryParse(User?.Identity?.Name, out var parsedId)
            ? parsedId
            : Guid.Empty;

        // Check if the user is authenticated
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var timeSheet = await _timeSheetRepository.GetByIdAsync(id);

        // Check if the time sheet exists
        if (timeSheet == null || timeSheet.Data == null)
        {
            return NotFound();
        }

        SetUpdateFields(timeSheet.Data, userId);
        timeSheet.Data.Approvals = updatedTimeSheet.Approvals;
        timeSheet.Data.Entries = updatedTimeSheet.Entries;

        var result = await _timeSheetRepository.UpdateAsync(id, timeSheet.Data);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _timeSheetRepository.DeleteAsync(id);
        return result.Success ? NoContent() : NotFound();
    }
}
