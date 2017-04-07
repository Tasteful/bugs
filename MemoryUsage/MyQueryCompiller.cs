using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace MemoryUsage
{
    public class MyQueryCompiller : QueryCompiler {
        private readonly ISensitiveDataLogger<QueryCompiler> _logger;
        private static readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter
            = new EvaluatableExpressionFilter();

        public MyQueryCompiller(IQueryContextFactory queryContextFactory, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator, IDatabase database, ISensitiveDataLogger<QueryCompiler> logger, MethodInfoBasedNodeTypeRegistry methodInfoBasedNodeTypeRegistry, ICurrentDbContext currentContext)
            : base(queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger, methodInfoBasedNodeTypeRegistry, currentContext)
        {
            _logger = logger;
        }

        protected override Expression ExtractParameters(Expression query, QueryContext queryContext)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (queryContext == null) throw new ArgumentNullException(nameof(queryContext));

            return MyParameterExtractingExpressionVisitor
                .ExtractParameters(query, queryContext, _evaluatableExpressionFilter, _logger);
        }

        private class EvaluatableExpressionFilter : EvaluatableExpressionFilterBase
        {
            private static readonly PropertyInfo _dateTimeNow
                = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Now));

            private static readonly PropertyInfo _dateTimeUtcNow
                = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.UtcNow));

            private static readonly MethodInfo _guidNewGuid
                = typeof(Guid).GetTypeInfo().GetDeclaredMethod(nameof(Guid.NewGuid));

            private static readonly List<MethodInfo> _randomNext
                = typeof(Random).GetTypeInfo().GetDeclaredMethods(nameof(Random.Next)).ToList();

            public override bool IsEvaluatableMethodCall(MethodCallExpression methodCallExpression)
            {
                if (_guidNewGuid.Equals(methodCallExpression.Method)
                    || _randomNext.Contains(methodCallExpression.Method))
                {
                    return false;
                }

                return base.IsEvaluatableMethodCall(methodCallExpression);
            }

            public override bool IsEvaluatableMember(MemberExpression memberExpression)
                => !Object.Equals(memberExpression.Member, _dateTimeNow)
                   && !Object.Equals(memberExpression.Member, _dateTimeUtcNow);
        }
    }
}