using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ApprovalRepository
{
    private readonly IMongoCollection<Approval> _approvals;

    public ApprovalRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("FBSDB");
        _approvals = database.GetCollection<Approval>("Approvals");
    }

    public async Task<List<Approval>> GetApprovalsAsync() =>
        await _approvals.Find(approval => true).ToListAsync();

    public async Task<Approval> GetApprovalAsync(string id) =>
        await _approvals.Find(approval => approval.Id == id).FirstOrDefaultAsync();

    public async Task CreateApprovalAsync(Approval newApproval) =>
        await _approvals.InsertOneAsync(newApproval);

    public async Task<bool> UpdateApprovalAsync(string id, Approval updatedApproval)
    {
        var result = await _approvals.ReplaceOneAsync(approval => approval.Id == id, updatedApproval);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteApprovalAsync(string id)
    {
        var result = await _approvals.DeleteOneAsync(approval => approval.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
