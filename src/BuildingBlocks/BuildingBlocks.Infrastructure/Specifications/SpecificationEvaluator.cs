using BuildingBlocks.Application.Specifications;
using BuildingBlocks.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BuildingBlocks.Infrastructure.Specifications
{
    public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification, bool evaluatePagination = true)
        {
            IQueryable<TEntity> query = inputQuery;

            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            foreach (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<TEntity, object> include in specification.Includes.Select(x => query.Include(x)))
            {
                query = include;
            }

            foreach (string includeString in specification.IncludeStrings)
            {
                query = query.Include(includeString);
            }

            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            if (evaluatePagination && specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            return query;
        }
    }
}
