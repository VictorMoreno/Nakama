using System.Linq.Expressions;

namespace Nakama.Shared.Domain.Specification;

public sealed record OrderItem<TEntity>(Expression<Func<TEntity, object>> Expression, OrderType Type);
