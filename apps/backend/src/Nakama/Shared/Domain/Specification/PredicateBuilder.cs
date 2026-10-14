using System.Linq.Expressions;

namespace Nakama.Shared.Domain.Specification;

public static class PredicateBuilder
{
    public static Expression<Func<TEntity, bool>> New<TEntity>(bool initial = true) =>
        initial ? True<TEntity>() : False<TEntity>();

    public static Expression<Func<TEntity, bool>> True<TEntity>() => _ => true;

    public static Expression<Func<TEntity, bool>> False<TEntity>() => _ => false;

    public static Expression<Func<TEntity, bool>> And<TEntity>(
        this Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right)
    {
        if (IsConstant(left, true))
        {
            return right;
        }

        if (IsConstant(right, true))
        {
            return left;
        }

        return Combine(left, right, Expression.AndAlso);
    }

    public static Expression<Func<TEntity, bool>> Or<TEntity>(
        this Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right)
    {
        if (IsConstant(left, false))
        {
            return right;
        }

        if (IsConstant(right, false))
        {
            return left;
        }

        return Combine(left, right, Expression.OrElse);
    }

    public static Expression<Func<TEntity, bool>> Not<TEntity>(this Expression<Func<TEntity, bool>> expression) =>
        Expression.Lambda<Func<TEntity, bool>>(Expression.Not(expression.Body), expression.Parameters);

    private static Expression<Func<TEntity, bool>> Combine<TEntity>(
        Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right,
        Func<Expression, Expression, BinaryExpression> merge)
    {
        ParameterExpression parameter = left.Parameters[0];
        Expression rightBody = new ParameterReplacer(right.Parameters[0], parameter).Visit(right.Body);

        return Expression.Lambda<Func<TEntity, bool>>(merge(left.Body, rightBody), parameter);
    }

    private static bool IsConstant<TEntity>(Expression<Func<TEntity, bool>> expression, bool value) =>
        expression.Body is ConstantExpression { Value: bool constant } && constant == value;

    private sealed class ParameterReplacer(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == source ? target : base.VisitParameter(node);
    }
}
