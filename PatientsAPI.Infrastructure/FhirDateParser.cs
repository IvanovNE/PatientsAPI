using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PatientsAPI.Infrastructure
{
    public static class FhirDateParser
    {
        private const string PrefixEq = "eq";
        private const string PrefixNe = "ne";
        private const string PrefixGt = "gt";
        private const string PrefixLt = "lt";
        private const string PrefixGe = "ge";
        private const string PrefixLe = "le";

        public static Expression<Func<T, bool>> BuildPredicate<T>(
            Expression<Func<T, DateTime>> dateSelector,
            string dateParameter)
        {
            var (prefix, dateTime, isDateOnly) = ParseParameter(dateParameter);

            var parameter = dateSelector.Parameters[0];
            var property = dateSelector.Body;

            Expression comparison = prefix switch
            {
                PrefixEq => isDateOnly
                    ? BuildDateOnlyEqualExpression(property, dateTime)
                    : BuildEqualExpression(property, dateTime),

                PrefixNe => isDateOnly
                    ? Expression.Not(BuildDateOnlyEqualExpression(property, dateTime))
                    : Expression.Not(BuildEqualExpression(property, dateTime)),

                PrefixGt => BuildGreaterThanExpression(property, dateTime),
                PrefixLt => BuildLessThanExpression(property, dateTime),
                PrefixGe => BuildGreaterThanOrEqualExpression(property, dateTime),
                PrefixLe => BuildLessThanOrEqualExpression(property, dateTime),

                _ => isDateOnly
                    ? BuildDateOnlyEqualExpression(property, dateTime)
                    : BuildEqualExpression(property, dateTime)
            };

            return Expression.Lambda<Func<T, bool>>(comparison, parameter);
        }

        private static (string prefix, DateTime dateTime, bool isDateOnly) ParseParameter(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                throw new ArgumentException("Date parameter cannot be empty");

            string[] prefixes = { PrefixGe, PrefixLe, PrefixEq, PrefixNe, PrefixGt, PrefixLt };

            foreach (var p in prefixes)
            {
                if (parameter.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    var dateString = parameter[p.Length..];
                    var (dateTime, isDateOnly) = ParseDate(dateString);
                    return (p.ToLowerInvariant(), dateTime, isDateOnly);
                }
            }

            var (dt, isDate) = ParseDate(parameter);
            return (PrefixEq, dt, isDate);
        }

        private static (DateTime dateTime, bool isDateOnly) ParseDate(string dateString)
        {
            if (dateString.Length == 10 && dateString[4] == '-' && dateString[7] == '-')
            {
                if (DateTime.TryParse(dateString, out var date))
                    return (date.Date, true);
            }

            if (DateTime.TryParse(dateString, out var dateTime))
                return (dateTime, false);

            throw new ArgumentException($"Invalid date format: {dateString}");
        }

        private static Expression BuildEqualExpression(Expression property, DateTime value)
        {
            var constant = Expression.Constant(value);
            return Expression.Equal(property, constant);
        }

        private static Expression BuildDateOnlyEqualExpression(Expression property, DateTime value)
        {
            var dateProperty = Expression.Property(property, nameof(DateTime.Date));
            var constant = Expression.Constant(value.Date);
            return Expression.Equal(dateProperty, constant);
        }

        private static Expression BuildGreaterThanExpression(Expression property, DateTime value)
        {
            var constant = Expression.Constant(value);
            return Expression.GreaterThan(property, constant);
        }

        private static Expression BuildLessThanExpression(Expression property, DateTime value)
        {
            var constant = Expression.Constant(value);
            return Expression.LessThan(property, constant);
        }

        private static Expression BuildGreaterThanOrEqualExpression(Expression property, DateTime value)
        {
            var constant = Expression.Constant(value);
            return Expression.GreaterThanOrEqual(property, constant);
        }

        private static Expression BuildLessThanOrEqualExpression(Expression property, DateTime value)
        {
            var constant = Expression.Constant(value);
            return Expression.LessThanOrEqual(property, constant);
        }
    }
}
