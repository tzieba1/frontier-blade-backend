using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TimeSheetController : ControllerBase
{
    private readonly TimeSheetRepository _timeSheetRepository;

    public TimeSheetController(TimeSheetRepository timeSheetRepository)
    {
        _timeSheetRepository = timeSheetRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<TimeSheet>>> Get() =>
        await _timeSheetRepository.GetTimeSheetsAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TimeSheet>> GetById(string id)
    {
        var timeSheet = await _timeSheetRepository.GetTimeSheetAsync(id);
        return timeSheet == null ? NotFound() : Ok(timeSheet);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TimeSheet newTimeSheet)
    {
        await _timeSheetRepository.CreateTimeSheetAsync(newTimeSheet);
        return CreatedAtAction(nameof(GetById), new { id = newTimeSheet.Id }, newTimeSheet);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, TimeSheet updatedTimeSheet)
    {
        var updated = await _timeSheetRepository.UpdateTimeSheetAsync(id, updatedTimeSheet);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _timeSheetRepository.DeleteTimeSheetAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
