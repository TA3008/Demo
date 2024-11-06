﻿using MongoDB.Bson;
using MongoDB.Driver;
using Demo.Common.Extensions;
using Demo.Application.Models;
using Demo.Application.Repositories;
using Demo.Core.Models;

namespace Demo.Database.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(IMongoDatabase db) : base(db)
        {
        }

        public async Task<List<Order>> FindAsync(OrderFilter filter)
        {
            var builder = Builders<Order>.Filter;
            FilterDefinition<Order> filterDefinition = Builders<Order>.Filter.Empty;
            if (!string.IsNullOrWhiteSpace(filter.Code))
                filterDefinition &= builder.Eq(m => m.Code, filter.Code.Trim());

            if (filter.CourseTitles != null && filter.CourseTitles.Count > 0)
            {
                var courseFilters = filter.CourseTitles
                    .Select(title => builder.ElemMatch(m => m.courses, c => c.Title == title));
                filterDefinition &= builder.Or(courseFilters);
            }

            if (filter.OrderStatus.HasValue)
                filterDefinition &= builder.Eq(m => m.Status, filter.OrderStatus.Value);

            if (!string.IsNullOrWhiteSpace(filter.query))
            {
                filterDefinition &= builder.Or(builder.Regex(m => m.CustomerName, new BsonRegularExpression($"{filter.query.Trim()}", "i")) |
                    builder.Regex(m => m.CustomerPhone, new BsonRegularExpression($"{filter.query.Trim()}", "i")));
            }

            if (filter.CreatedFrom.HasValue)
                filterDefinition &= builder.Gte(m => m.Created, new DateTime(filter.CreatedFrom.Value.Year, filter.CreatedFrom.Value.Month, filter.CreatedFrom.Value.Day));

            if (filter.CreatedTo.HasValue)
                filterDefinition &= builder.Lte(m => m.Created, new DateTime(filter.CreatedTo.Value.Year, filter.CreatedTo.Value.Month, filter.CreatedTo.Value.Day, 23, 59, 59));

            return await _collection.Find(filterDefinition).ToListAsync();
        }
    }
}
