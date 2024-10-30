using MongoDB.Driver;

public class UserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("FBSDB");
        _users = database.GetCollection<User>("Users");
    }

    public async Task<List<User>> GetUsersAsync() =>
        await _users.Find(user => true).ToListAsync();

    public async Task<User> GetUserAsync(string id) =>
        await _users.Find<User>(user => user.Id == id).FirstOrDefaultAsync();

    // Create a new user
    public async Task CreateUserAsync(User newUser) =>
        await _users.InsertOneAsync(newUser);

    // Update an existing user
    public async Task<bool> UpdateUserAsync(string id, User updatedUser)
    {
        var result = await _users.ReplaceOneAsync(user => user.Id == id, updatedUser);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    // Delete a user by ID
    public async Task<bool> DeleteUserAsync(string id)
    {
        var result = await _users.DeleteOneAsync(user => user.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

}
