using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TeamRepository
{
    private readonly IMongoCollection<Team> _teams;

    public TeamRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("FBSDB");
        _teams = database.GetCollection<Team>("Teams");
    }

    public async Task<List<Team>> GetTeamsAsync() =>
        await _teams.Find(team => true).ToListAsync();

    public async Task<Team> GetTeamAsync(string id) =>
        await _teams.Find(team => team.Id == id).FirstOrDefaultAsync();

    public async Task CreateTeamAsync(Team newTeam) =>
        await _teams.InsertOneAsync(newTeam);

    public async Task<bool> UpdateTeamAsync(string id, Team updatedTeam)
    {
        var result = await _teams.ReplaceOneAsync(team => team.Id == id, updatedTeam);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteTeamAsync(string id)
    {
        var result = await _teams.DeleteOneAsync(team => team.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
