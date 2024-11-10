using MongoDB.Driver;

public class ClaimRepository : BaseRepository<Claim>
{
    private readonly ILogger<ClaimRepository> _logger;

    public ClaimRepository(ILogger<ClaimRepository> logger, IMongoClient mongoClient)
        : base(logger, mongoClient, "FBSDB", "Claims")
    {
        _logger = logger;
    }

    // Custom method to get all claims for a specific user
    public async Task<List<Claim>> GetAllByUserIdAsync(Guid userId)
    {
        var filter = Builders<Claim>.Filter.Eq(x => x.UserId, userId);
        return await Collection.Find(filter).ToListAsync();
    }

    // Optional method to find a specific claim by type and user ID
    public async Task<Claim> GetByTypeAndUserIdAsync(ClaimType type, Guid userId)
    {
        var filter = Builders<Claim>.Filter.And(
            Builders<Claim>.Filter.Eq(x => x.Type, type),
            Builders<Claim>.Filter.Eq(x => x.UserId, userId)
        );
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }
}