using Demo.Application.Repositories;
using Demo.Core.Models;
using MongoDB.Driver;

namespace Demo.Database.Repositories
{
    public class ClassRepository : BaseRepository<Class>, IClassRepository
    {
        public ClassRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}