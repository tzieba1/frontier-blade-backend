using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ApprovalController : ControllerBase
{
    private readonly ApprovalRepository _approvalRepository;

    public ApprovalController(ApprovalRepository approvalRepository)
    {
        _approvalRepository = approvalRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Approval>>> Get() =>
        await _approvalRepository.GetApprovalsAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Approval>> GetById(string id)
    {
        var approval = await _approvalRepository.GetApprovalAsync(id);
        return approval == null ? NotFound() : Ok(approval);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Approval newApproval)
    {
        await _approvalRepository.CreateApprovalAsync(newApproval);
        return CreatedAtAction(nameof(GetById), new { id = newApproval.Id }, newApproval);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Approval updatedApproval)
    {
        var updated = await _approvalRepository.UpdateApprovalAsync(id, updatedApproval);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _approvalRepository.DeleteApprovalAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
