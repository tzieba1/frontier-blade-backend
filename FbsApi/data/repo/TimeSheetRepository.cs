using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TimeSheetRepository
{
    private readonly IMongoCollection<TimeSheet> _timeSheets;

    public TimeSheetRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("FBSDB");
        _timeSheets = database.GetCollection<TimeSheet>("TimeSheets");
    }

    public async Task<List<TimeSheet>> GetTimeSheetsAsync() =>
        await _timeSheets.Find(timeSheet => true).ToListAsync();

    public async Task<TimeSheet> GetTimeSheetAsync(string id) =>
        await _timeSheets.Find(timeSheet => timeSheet.Id == id).FirstOrDefaultAsync();

    public async Task CreateTimeSheetAsync(TimeSheet newTimeSheet) =>
        await _timeSheets.InsertOneAsync(newTimeSheet);

    public async Task<bool> UpdateTimeSheetAsync(string id, TimeSheet updatedTimeSheet)
    {
        var result = await _timeSheets.ReplaceOneAsync(timeSheet => timeSheet.Id == id, updatedTimeSheet);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteTimeSheetAsync(string id)
    {
        var result = await _timeSheets.DeleteOneAsync(timeSheet => timeSheet.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
