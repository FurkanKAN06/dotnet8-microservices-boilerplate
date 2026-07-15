using System.Linq;
using System.Linq.Expressions;
using BuildingBlocks.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyGlobalQueryFilter(this ModelBuilder modelBuilder)
        {
            System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> entityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in entityTypes)
            {
                ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
                System.Reflection.MethodInfo propertyMethodInfo = typeof(EF).GetMethod("Property")!.MakeGenericMethod(typeof(bool));
                MethodCallExpression isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));
                BinaryExpression compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
                LambdaExpression lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
