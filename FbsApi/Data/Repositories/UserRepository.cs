using MongoDB.Driver;

public class UserRepository : BaseRepository<User>
{
    private readonly ILogger<UserRepository> _logger;
    public UserRepository(ILogger<UserRepository> logger, IMongoClient mongoClient)
        : base(logger, mongoClient, "FBSDB", "Users")
    {
        _logger = logger;
    }

    public async Task<Result<User>> FindByEmailAsync(string email)
    {
        try
        {
            var user = await Collection.Find(user => user.Email == email).FirstOrDefaultAsync();
            return user != null
                ? Result<User>.SuccessResult(user)
                : Result<User>.FailureResult("User not found");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user by email: {ex}");
            return Result<User>.FailureResult($"Error fetching user by email: {ex.Message}");
        }
    }
}
