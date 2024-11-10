using MongoDB.Driver;

public class ApprovalRepository: BaseRepository<Approval>
{
    private readonly ILogger<ApprovalRepository> _logger;
    public ApprovalRepository(ILogger<ApprovalRepository> logger, IMongoClient mongoClient)
        : base(logger, mongoClient, "FBSDB", "Approvals")
    {
        _logger = logger;
    }
}
