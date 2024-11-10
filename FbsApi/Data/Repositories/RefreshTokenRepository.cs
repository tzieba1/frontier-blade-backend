using FbsApi.Data.Models;
using MongoDB.Driver;

public class RefreshTokenRepository : BaseRepository<RefreshToken>
{
  private readonly ILogger<RefreshTokenRepository> _logger;
  public RefreshTokenRepository(ILogger<RefreshTokenRepository> logger, IMongoClient mongoClient)
        : base(logger, mongoClient, "FBSDB", "RefreshTokens")
  {
    _logger = logger;
  }

  public async Task<RefreshToken> FindByTokenAsync(string token)
  {
    var filter = Builders<RefreshToken>.Filter.Eq(x => x.Token, token);
    return await Collection.Find(filter).FirstOrDefaultAsync();
  }
}
