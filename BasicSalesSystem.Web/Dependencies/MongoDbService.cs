namespace BasicSalesSystem.Web.Dependencies.MongoDbService
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using MongoDB.Driver;

    public class MongoDbService : IDisposable
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private IClientSessionHandle _session;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            _mongoClient = new MongoClient(connectionString);
            _database = _mongoClient.GetDatabase(MongoUrl.Create(connectionString).DatabaseName);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>(string collection) where TEntity : class => _database.GetCollection<TEntity>(collection);

        public async Task<IClientSessionHandle> GetSessionAsync()
        {
            if (_session == null)
            {
                _session = await _mongoClient.StartSessionAsync();
            }
            return _session;
        }

        public async Task StartTransactionAsync()
        {
            await Task.FromResult(0).ContinueWith(t => _session?.StartTransaction());
        }

        public async Task CommitTransactionAsync()
        {
            if (_session != null && _session.IsInTransaction)
                await _session?.CommitTransactionAsync();
        }

        public async Task AbortTransactionAsync()
        {
            if (_session != null && _session.IsInTransaction)
                await _session?.AbortTransactionAsync();
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
