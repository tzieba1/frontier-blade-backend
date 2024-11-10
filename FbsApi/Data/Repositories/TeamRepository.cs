using MongoDB.Driver;

public class TeamRepository: BaseRepository<Team>
{
    private readonly ILogger<TeamRepository> _logger;
    public TeamRepository(ILogger<TeamRepository> logger, IMongoClient mongoClient) 
        : base(logger, mongoClient, "FBSDB", "Teams") {
        _logger = logger;
        }
}
