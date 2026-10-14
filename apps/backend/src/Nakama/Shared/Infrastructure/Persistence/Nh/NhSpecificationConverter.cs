using Nakama.Shared.Domain.Specification;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

public static class NhSpecificationConverter<TEntity>
    where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(
        IQueryable<TEntity> query,
        ISpecification<TEntity> specification,
        bool includePaging = true)
    {
        IQueryable<TEntity> filtered = query.Where(specification.Criteria);
        IQueryable<TEntity> ordered = ApplyOrders(filtered, specification.Orders);

        return includePaging && specification.IsPagingEnabled
            ? ordered.Skip(specification.Skip).Take(specification.Take)
            : ordered;
    }

    public static IReadOnlyList<TEntity> FirstOfEachGroup(
        IEnumerable<TEntity> orderedEntities,
        ISpecification<TEntity> specification)
    {
        Func<TEntity, object> key = specification.GroupBy!.Compile();
        IEnumerable<TEntity> firstOfEachGroup = orderedEntities
            .GroupBy(key)
            .Select(group => group.First());

        return specification.IsPagingEnabled
            ? firstOfEachGroup.Skip(specification.Skip).Take(specification.Take).ToList()
            : firstOfEachGroup.ToList();
    }

    public static int CountGroups(IEnumerable<TEntity> entities, ISpecification<TEntity> specification) =>
        entities.GroupBy(specification.GroupBy!.Compile()).Count();

    private static IQueryable<TEntity> ApplyOrders(IQueryable<TEntity> query, IReadOnlyList<OrderItem<TEntity>> orders)
    {
        IOrderedQueryable<TEntity>? ordered = null;

        foreach (OrderItem<TEntity> order in orders)
        {
            bool ascending = order.Type == OrderType.Ascending;

            if (ordered is null)
            {
                ordered = ascending ? query.OrderBy(order.Expression) : query.OrderByDescending(order.Expression);
            }
            else
            {
                ordered = ascending ? ordered.ThenBy(order.Expression) : ordered.ThenByDescending(order.Expression);
            }
        }

        return ordered ?? query;
    }
}
