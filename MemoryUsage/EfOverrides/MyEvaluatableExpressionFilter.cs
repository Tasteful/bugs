using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace MemoryUsage.EfOverrides
{
    public class MyEvaluatableExpressionFilter : EvaluatableExpressionFilterBase
    {
        private static readonly PropertyInfo _dateTimeNow
            = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.Now));
        private static readonly PropertyInfo _dateTimeUtcNow
            = typeof(DateTime).GetTypeInfo().GetDeclaredProperty(nameof(DateTime.UtcNow));
        private static readonly MethodInfo _guidNewGuid
            = typeof(Guid).GetTypeInfo().GetDeclaredMethod(nameof(Guid.NewGuid));
        private static readonly List<MethodInfo> _randomNext
            = typeof(Random).GetTypeInfo().GetDeclaredMethods(nameof(Random.Next)).ToList();

        public override bool IsEvaluatableMember(MemberExpression memberExpression)
        {
            return !Object.Equals(memberExpression.Member, _dateTimeNow)
                   && !Object.Equals(memberExpression.Member, _dateTimeUtcNow);
        }

        public override bool IsEvaluatableMethodCall(MethodCallExpression methodCallExpression)
        {
            if (_guidNewGuid.Equals(methodCallExpression.Method)
                || _randomNext.Contains(methodCallExpression.Method))
            {
                return false;
            }

            return base.IsEvaluatableMethodCall(methodCallExpression);
        }
    }
}