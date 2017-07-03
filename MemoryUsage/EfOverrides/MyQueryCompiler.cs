using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace MemoryUsage.EfOverrides
{
    public class MyQueryCompiler : QueryCompiler
    {
        private static readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter
            = new MyEvaluatableExpressionFilter();
        private readonly ISensitiveDataLogger _logger;

        public MyQueryCompiler(IQueryContextFactory queryContextFactory, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator, IDatabase database, ISensitiveDataLogger<QueryCompiler> logger, MethodInfoBasedNodeTypeRegistry methodInfoBasedNodeTypeRegistry, ICurrentDbContext currentContext)
            : base(queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger, methodInfoBasedNodeTypeRegistry, currentContext)
        {
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression ExtractParameters([NotNull] Expression query, [NotNull] QueryContext queryContext)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (queryContext == null) throw new ArgumentNullException(nameof(queryContext));

            return MyParameterExtractingExpressionVisitor
                .ExtractParameters(query, queryContext, _evaluatableExpressionFilter, _logger);
        }
    }
}
