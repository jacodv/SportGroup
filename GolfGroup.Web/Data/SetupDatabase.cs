using System;
using System.Threading.Tasks;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace GolfGroup.Api.Data
{
  public static class SetupDatabase
  {
    public static async Task Init(IServiceProvider serviceProvider)
    {
      await _createGroupIndexes(((MongoRepository<Group>) serviceProvider.GetService<IRepository<Group>>()).Collection);
      await _createPlayerIndexes(((MongoRepository<Player>) serviceProvider.GetService<IRepository<Player>>()).Collection);
      await _createTournamentIndexes(((MongoRepository<Tournament>) serviceProvider.GetService<IRepository<Tournament>>()).Collection);
    }

    private static async Task _createGroupIndexes(IMongoCollection<Group> userCollection)
    {
      var indexKeys = Builders<Group>.IndexKeys.Ascending(x => x.Name);
      await _createIndex(userCollection, indexKeys);
    }

    private static async Task _createPlayerIndexes(IMongoCollection<Player> userCollection)
    {
      var indexKeys = Builders<Player>.IndexKeys.Ascending(x => x.Email);
      await _createIndex(userCollection, indexKeys, false);
      var groupsKeys = Builders<Player>.IndexKeys.Ascending(x => x.Groups);
      await _createIndex(userCollection, groupsKeys, false);
    }

    private static async Task _createTournamentIndexes(IMongoCollection<Tournament> userCollection)
    {
      var indexKeys = Builders<Tournament>.IndexKeys.Ascending(x => x.Date);
      await _createIndex(userCollection, indexKeys, false);
      var groupKeys = Builders<Tournament>.IndexKeys.Ascending(x => x.Group);
      await _createIndex(userCollection, groupKeys, false);
      var groupAndDateKeys = Builders<Tournament>.IndexKeys
        .Ascending(x => x.Group)
        .Ascending(x => x.Date);
      await _createIndex(userCollection, groupAndDateKeys);

    }

    private static async Task _createIndex<T>(IMongoCollection<T> userCollection, IndexKeysDefinition<T> indexKeys, bool isUnique = true)
    {
      var createIndexOptions = new CreateIndexOptions()
      {
        Unique = isUnique
      };
      var createIndexModel = new CreateIndexModel<T>(indexKeys, createIndexOptions);
      await userCollection.Indexes.CreateOneAsync(createIndexModel);
    }
  }
}
