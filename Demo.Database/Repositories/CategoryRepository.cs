using Demo.Application.Repositories;
using Demo.Core.Models;
using MongoDB.Driver;

namespace Demo.Database.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}