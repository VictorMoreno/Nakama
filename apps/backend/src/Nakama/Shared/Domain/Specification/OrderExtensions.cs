using System.Linq.Expressions;

namespace Nakama.Shared.Domain.Specification;

public static class OrderExtensions
{
    public static List<OrderItem<TEntity>> ThenBy<TEntity>(
        this List<OrderItem<TEntity>> orders,
        Expression<Func<TEntity, object>> newOrder)
    {
        orders.Add(new OrderItem<TEntity>(newOrder, OrderType.Ascending));

        return orders;
    }

    public static List<OrderItem<TEntity>> ThenByDescending<TEntity>(
        this List<OrderItem<TEntity>> orders,
        Expression<Func<TEntity, object>> newOrder)
    {
        orders.Add(new OrderItem<TEntity>(newOrder, OrderType.Descending));

        return orders;
    }
}
