using MongoDbGenericRepository;
using MongoDbGenericRepository.Models;

namespace THive.Core.DeviceApi.MongoDb;

public interface IGenericMongoRepository<TDocument, TPrimaryKey> : IBaseMongoRepository
    where TDocument : IDocument<TPrimaryKey>
    where TPrimaryKey : IEquatable<TPrimaryKey>
{
    
}