using MongoDbGenericRepository;
using MongoDbGenericRepository.Models;

namespace THive.Core.DeviceApi.DbStorage;

public class GenericMongoDbRepository<TModel, TPrimaryKey> : BaseMongoRepository<TPrimaryKey>, IGenericMongoDbRepository<TModel>
    where TModel : class, IModel
    where TPrimaryKey : IEquatable<TPrimaryKey>
{
    public GenericMongoDbRepository(MongoDbSettings settings) 
        : base(settings.GetConnectionString(), settings.DatabaseName)
    {
    }

    public async Task<TModel> Get(TPrimaryKey key)
    {
        var result = GetByIdAsync<TModel>(key);
    }
}