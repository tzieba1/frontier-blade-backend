using MongoDB.Driver;

public class TimeSheetRepository: BaseRepository<TimeSheet>
{
    private readonly ILogger<TimeSheetRepository> _logger;
    public TimeSheetRepository(ILogger<TimeSheetRepository> logger, IMongoClient mongoClient) 
        : base(logger, mongoClient, "FBSDB", "TimeSheets") {
        _logger = logger;
        }
}
