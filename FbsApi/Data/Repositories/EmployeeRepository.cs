using MongoDB.Driver;

public class EmployeeRepository
{
    private readonly IMongoCollection<Employee> _employees;

    public EmployeeRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("FBSDB");
        _employees = database.GetCollection<Employee>("Employees");
    }

    public async Task<List<Employee>> GetEmployeesAsync() =>
        await _employees.Find(employee => true).ToListAsync();

    public async Task<Employee> GetEmployeeAsync(Guid id) =>
        await _employees.Find(employee => employee.Id == id).FirstOrDefaultAsync();

    public async Task CreateEmployeeAsync(Employee newEmployee) =>
        await _employees.InsertOneAsync(newEmployee);

    public async Task<bool> UpdateEmployeeAsync(Guid id, Employee updatedEmployee)
    {
        var result = await _employees.ReplaceOneAsync(employee => employee.Id == id, updatedEmployee);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        var result = await _employees.DeleteOneAsync(employee => employee.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
