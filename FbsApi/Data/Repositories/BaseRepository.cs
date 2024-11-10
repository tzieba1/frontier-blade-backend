using MongoDB.Driver;

public class BaseRepository<T> where T : BaseModel
{
  private readonly IMongoCollection<T> _collection;

  private readonly ILogger<BaseRepository<T>> _logger;

  protected IMongoCollection<T> Collection => _collection;

  public BaseRepository(ILogger<BaseRepository<T>> logger, IMongoClient mongoClient, string databaseName, string collectionName)
  {
    var database = mongoClient.GetDatabase(databaseName);
    _collection = database.GetCollection<T>(collectionName);
    _logger = logger;
  }

  public virtual async Task<Result<List<T>>> GetAllAsync()
  {
    try
    {
      var items = await _collection.Find(item => true).ToListAsync();
      return Result<List<T>>.SuccessResult(items);
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error fetching items: {ex}");
      return Result<List<T>>.FailureResult($"Error fetching item: {ex.Message}");
    }
  }

  public virtual async Task<Result<T>> GetByIdAsync(Guid id)
  {
    try
    {
      var item = await _collection.Find(item => item.Id == id).FirstOrDefaultAsync();
      return item != null
          ? Result<T>.SuccessResult(item)
          : Result<T>.FailureResult("Item not found");
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error fetching item: {ex}");
      return Result<T>.FailureResult($"Error fetching item: {ex.Message}");
    }
  }

  public virtual async Task<Result<T>> CreateAsync(T newItem)
  {
    try
    {
      await _collection.InsertOneAsync(newItem);
      return Result<T>.SuccessResult(newItem); // Item created successfully
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error creating item: {ex}");
      return Result<T>.FailureResult($"Error creating item: {ex.Message}");
    }
  }

  public virtual async Task<Result> UpdateAsync(Guid id, T updatedItem)
  {
    try
    {
      var result = await _collection.ReplaceOneAsync(item => item.Id == id, updatedItem);
      return result.IsAcknowledged && result.ModifiedCount > 0
          ? Result.SuccessResult()
          : Result.FailureResult("Update failed or item not found");
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error updating item: {ex}");
      return Result.FailureResult($"Error updating item: {ex.Message}");
    }
  }

  public virtual async Task<Result> DeleteAsync(Guid id)
  {
    try
    {
      var result = await _collection.DeleteOneAsync(item => item.Id == id);
      return result.IsAcknowledged && result.DeletedCount > 0
          ? Result.SuccessResult()
          : Result.FailureResult("Deletion failed or item not found");
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error deleting item: {ex}");
      return Result.FailureResult($"Error deleting item: {ex.Message}");
    }
  }
}
