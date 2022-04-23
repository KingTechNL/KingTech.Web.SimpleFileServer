using MongoDbGenericRepository.Models;

namespace THive.Core.DeviceApi.Models;

public interface IApiModel<TPrimaryKey> : IDocument<TPrimaryKey>
    where TPrimaryKey : IEquatable<TPrimaryKey>
{
    
}