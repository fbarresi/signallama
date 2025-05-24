using System.Linq.Expressions;
using LiteDB;

namespace Signallama.Web.Services;

public class DocumentStore : BackgroundService, IDisposable
{
    private readonly ILogger logger;
    private readonly LiteDatabase db;

    public DocumentStore(ILogger<DocumentStore> logger)
    {
        this.logger = logger;
        
        try
        {
            db = new LiteDatabase("Filename=local.db;Connection=Shared;");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while initializing Database Service");
        }
    }


    public IEnumerable<T> GetCollection<T>(string name, Expression<Func<T, bool>> filter, int skip = 0, int limit = -1)
        where T : new()
    {
        var collection = GetDbCollection<T>(name);
        var found = collection.Find(filter, skip, limit > 1 ? limit : int.MaxValue);
        return found.ToList();
    }

    public IEnumerable<T> GetCollection<T, K>(string name, Expression<Func<T, bool>> filter,
        Expression<Func<T, K>> orderBy, bool orderByDescending, int skip = 0, int limit = -1) where T : new()
    {
        var collection = GetDbCollection<T>(name);
        var query = collection.Query()
            .Where(filter);

        if (orderByDescending)
            query = query.OrderByDescending(orderBy);
        else
            query = query.OrderBy(orderBy);

        var found = query
            .Skip(skip)
            .Limit(limit).ToEnumerable();
        return found.ToList();
    }

    public int CountElementInCollection<T>(string name) where T : new()
    {
        return GetDbCollection<T>(name).Count();
    }

    public T InsertOrUpdateIntoCollection<T>(string name, T obj) where T : new()
    {
        var collection = GetDbCollection<T>(name);
        var updated = collection.Update(obj);
        if(!updated)
            collection.Insert(obj);
        return obj;
    }
    
    public T InsertIntoCollection<T>(string name, T obj) where T : new()
    {
        var collection = GetDbCollection<T>(name);
        collection.Insert(obj);
        return obj;
    }

    public T UpdateIntoCollection<T>(string name, T obj) where T : new()
    {
        var collection = GetDbCollection<T>(name);
        collection.Update(obj);
        return obj;
    }

    public bool RemoveFromCollection<T>(string name, T obj) where T : new()
    {
        var collection = GetDbCollection<T>(name);
        var objectId = obj.GetType().GetProperty("Id").GetValue(obj, null);
        return collection.Delete(objectId.ToString());
    }

    public void IndexCollection<T>(string name, Expression<Func<T, string>> index) where T : new()
    {
        var collection = GetDbCollection<T>(name);
        collection.EnsureIndex(index);
    }

    public void Dispose()
    {
        db?.Dispose();
    }

    private ILiteCollection<T> GetDbCollection<T>(string name) where T : new()
    {
        return db.GetCollection<T>(name);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Delay(-1, stoppingToken);
    }

    //allow to delete static
    public static void DropCollection(string name)
    {
        try
        {
            using var db = new LiteDatabase("Filename=local.db;Connection=Shared;");
            db.DropCollection(name);
        }
        catch (Exception _)
        {
            //ignore here
        }
    }
}