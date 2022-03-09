using MongoDB.Driver;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories;

public class ItemsRepository : IItemsRepository
{
    #region Fields :
    private const string collectionName = "items";
    private readonly IMongoCollection<Item> dbCollection;
    private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;
    #endregion

    #region CTORS :
    public ItemsRepository(IMongoDatabase database)
    {
        dbCollection = database.GetCollection<Item>(collectionName);
    }
    #endregion

    #region Methods :
    public async Task<IReadOnlyCollection<Item>> GetAllAsync() => await dbCollection.Find(filterBuilder.Empty).ToListAsync();
    public async Task<Item> GetAsync(Guid id)
    {
        FilterDefinition<Item> filter = filterBuilder.Eq(x => x.Id, id);
        return await dbCollection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task CreateAsync(Item item)
    {
        if (item is null) throw new ArgumentException(null, nameof(item));
        await dbCollection.InsertOneAsync(item);
    }
    public async Task UpdateAsync(Item item)
    {
        if (item is null) throw new ArgumentException(null, nameof(item));
        FilterDefinition<Item> filter = filterBuilder.Eq(x => x.Id, item.Id);
        await dbCollection.ReplaceOneAsync(filter, item);
    }
    public async Task DeleteAsync(Guid id)
    {
        FilterDefinition<Item> filter = filterBuilder.Eq(x => x.Id, id);
        await dbCollection.DeleteOneAsync(filter);
    }
    #endregion
}
