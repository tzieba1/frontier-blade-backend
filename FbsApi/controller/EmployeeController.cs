using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeRepository _employeeRepository;

    public EmployeeController(EmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Employee>>> Get() =>
        await _employeeRepository.GetEmployeesAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetById(string id)
    {
        var employee = await _employeeRepository.GetEmployeeAsync(id);
        return employee == null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Employee newEmployee)
    {
        await _employeeRepository.CreateEmployeeAsync(newEmployee);
        return CreatedAtAction(nameof(GetById), new { id = newEmployee.Id }, newEmployee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Employee updatedEmployee)
    {
        var updated = await _employeeRepository.UpdateEmployeeAsync(id, updatedEmployee);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _employeeRepository.DeleteEmployeeAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
