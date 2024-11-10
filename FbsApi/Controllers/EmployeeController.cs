using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : BaseController<Employee>
{
    private readonly EmployeeRepository _employeeRepository;

    public EmployeeController(EmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Employee>>> Get() =>
        await _employeeRepository.GetEmployeesAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Employee>> GetById(Guid id)
    {
        var employee = await _employeeRepository.GetEmployeeAsync(id);
        return employee == null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Employee newEmployee)
    {
        Guid userId = Guid.TryParse(User?.Identity?.Name, out var parsedId)
            ? parsedId
            : Guid.Empty;
        SetCreateFields(newEmployee, userId);

        await _employeeRepository.CreateEmployeeAsync(newEmployee);
        return CreatedAtAction(nameof(GetById), new { id = newEmployee.Id }, newEmployee);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Employee updatedEmployee)
    {
        var employee = await _employeeRepository.GetEmployeeAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        Guid userId = Guid.TryParse(User?.Identity?.Name, out var parsedId)
            ? parsedId
            : Guid.Empty;
        SetUpdateFields(employee, userId);

        employee.UserId = updatedEmployee.UserId;
        employee.Number = updatedEmployee.Number;
        employee.Type = updatedEmployee.Type;

        var result = await _employeeRepository.UpdateEmployeeAsync(id, employee);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _employeeRepository.DeleteEmployeeAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
