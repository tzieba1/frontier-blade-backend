using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ApprovalController : BaseController<Approval>
{
    private readonly ApprovalRepository _approvalRepository;

    public ApprovalController(ApprovalRepository approvalRepository)
    {
        _approvalRepository = approvalRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin, Supervisor")]
    public async Task<ActionResult<List<Approval>>> GetAll()
    {
        var result = await _approvalRepository.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin, Employee, Supervisor")]
    public async Task<ActionResult<Approval>> GetById(Guid id)
    {
        var result = await _approvalRepository.GetByIdAsync(id);
        return result == null || result.Data == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, Supervisor")]
    public async Task<IActionResult> Create(Approval newApproval)
    {
        // Get the user ID from the claims
        var userIdClaim = User.FindFirst("UserId")?.Value;
        Guid userId = Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : Guid.Empty;
        
        // Check if the user is authenticated
        if (userId == Guid.Empty)
        {
            Console.WriteLine("User is not authenticated (empty GUID)");
            return Unauthorized();
        }

        SetCreateFields(newApproval, userId);
        var result = await _approvalRepository.CreateAsync(newApproval);

        if (!result.Success)
        {
            return BadRequest("Unable to create approval record.");
        }

        if (result.Data == null)
        {
            return NotFound();
        } else {
            Console.WriteLine($"Approval created successfully with id {result.Data.Id}");
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin, Supervisor")]
    public async Task<IActionResult> Update(Guid id, Approval updatedApproval)
    {
        // Get the user ID from the claims
        var userIdClaim = User.FindFirst("UserId")?.Value;
        Guid userId = Guid.TryParse(userIdClaim, out var parsedId) ? parsedId : Guid.Empty;

        // Check if the user is authenticated
        if (userId == Guid.Empty)
        {
        return Unauthorized();
        }

        var approval = await _approvalRepository.GetByIdAsync(id);

        // Check if the approval exists
        if (approval == null || approval.Data == null)
        {
            return NotFound();
        }

        SetUpdateFields(approval.Data, userId);
        approval.Data.Status = updatedApproval.Status;
        approval.Data.Comments = updatedApproval.Comments;
        approval.Data.TimeStamp = updatedApproval.TimeStamp;

        var result = await _approvalRepository.UpdateAsync(id, approval.Data);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin, Supervisor")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _approvalRepository.DeleteAsync(id);
        return result.Success ? NoContent() : NotFound();
    }
}
