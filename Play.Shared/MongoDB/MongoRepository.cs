using System.Linq.Expressions;
using MongoDB.Driver;

namespace Play.Shared.MongoDB;

public class MongoRepository<TEntity> : IRepository<TEntity> where TEntity : IEntity
{
    #region Fields :
    private readonly IMongoCollection<TEntity> dbCollection;
    private readonly FilterDefinitionBuilder<TEntity> filterBuilder = Builders<TEntity>.Filter;
    #endregion

    #region CTORS :
    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        dbCollection = database.GetCollection<TEntity>(collectionName);
    }
    #endregion

    #region Methods :
    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync() => await dbCollection.Find(filterBuilder.Empty).ToListAsync();
    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter)
        => await dbCollection.Find(filter).ToListAsync();
    public async Task<TEntity> GetAsync(Guid id)
    {
        var filter = filterBuilder.Eq(x => x.Id, id);
        return await dbCollection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter)
        => await dbCollection.Find(filter).FirstOrDefaultAsync();
    public async Task CreateAsync(TEntity entity)
    {
        if (entity is null) throw new ArgumentException(null, nameof(entity));
        await dbCollection.InsertOneAsync(entity);
    }
    public async Task UpdateAsync(TEntity entity)
    {
        if (entity is null) throw new ArgumentException(null, nameof(entity));
        var filter = filterBuilder.Eq(x => x.Id, entity.Id);
        await dbCollection.ReplaceOneAsync(filter, entity);
    }
    public async Task DeleteAsync(Guid id)
    {
        var filter = filterBuilder.Eq(x => x.Id, id);
        await dbCollection.DeleteOneAsync(filter);
    }
    #endregion
}
