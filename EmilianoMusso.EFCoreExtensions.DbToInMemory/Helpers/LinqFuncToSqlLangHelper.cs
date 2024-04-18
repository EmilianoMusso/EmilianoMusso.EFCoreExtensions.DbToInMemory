using System.Collections.Generic;
using System.Linq;

namespace EFCoreExtensions.DbToInMemory.Helpers
{
    public static class LinqFuncToSqlLangHelper
    {
        // LINQ
        public const string LinqAndAlso = "AndAlso";
        public const string LinqOrElse = "OrElse";
        public const string LinqContains = ".Contains";
        public const string LinqStartsWith = ".StartsWith";
        public const string LinqEndsWith = ".EndsWith";

        public const string LinqSlash = "\\";
        public const string LinqQuote = "\"";
        public const string LinqEqual = "==";
        public const string LinqNotEqual = "!=";

        // SQL
        public const string WhereFunc = "WHERE ";

        public const string SqlAnd = "AND";
        public const string SqlOr = "OR";
        public const string SqlLike = " LIKE";

        public const string SqlSlash = "";
        public const string SqlQuote = "'";
        public const string SqlEqual = "=";
        public const string SqlNotEqual = "<>";

        private static readonly Dictionary<string, string> keyDictionary = new()
        {
            [LinqAndAlso] = SqlAnd,
            [LinqOrElse] = SqlOr,
            [LinqContains] = SqlLike,
            [LinqStartsWith] = SqlLike,
            [LinqEndsWith] = SqlLike
        };

        private static readonly Dictionary<string, string> opDictionary = new()
        {
            [LinqSlash] = SqlSlash,
            [LinqQuote] = SqlQuote,
            [LinqEqual] = SqlEqual,
            [LinqNotEqual] = SqlNotEqual
        };

        private static Dictionary<string, string> Keywords => keyDictionary;

        private static Dictionary<string, string> Operators => opDictionary;

        public static string GetKeyword(string key)
        {
            Keywords.TryGetValue(key, out var sqlFunc);
            return sqlFunc;
        }

        public static string GetOperator(string key)
        {
            Operators.TryGetValue(key, out var sqlOperator);
            return sqlOperator;
        }

        public static string ReplaceOperators(this string linqExpression)
        {
            foreach (var op in Operators)
            {
                var val = GetOperator(op.Key);
                linqExpression = linqExpression.Replace(op.Key, val);
            }

            return linqExpression;
        }

        public static string ReplaceKeywords(this string linqExpression)
        {
            foreach (var key in Keywords)
            {
                var val = GetKeyword(key.Key);
                linqExpression = linqExpression.Replace(key.Key, val);
            }

            return linqExpression;
        }

        /// <summary>
        /// EXPERIMENTAL: A method which, given a string that represent a full LINQ Expression, converts it to SQL language,
        /// thus avoiding expression-tree traversing
        /// </summary>
        /// <param name="linqWhereExpression"></param>
        /// <returns></returns>
        public static string GetSQLWhereClause(this string linqWhereExpression)
        {
            var separators = new char[] { ' ', '(', ')' };
            var linqExprSegments = linqWhereExpression.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);

            if (linqExprSegments.Length == 0)
                return "";

            var whereClause = WhereFunc;

            for (var i = 2; i < linqExprSegments.Length; i++)
            {
                linqExprSegments[i] = linqExprSegments[i].Replace(linqExprSegments[0] + ".", "")
                                                         .ReplaceOperators();

                if (linqExprSegments[i].Any(x => x == '\''))
                {
                    linqExprSegments[i] = linqExprSegments[i].Replace("\'", "");

                    if (linqExprSegments[i - 1].Contains(LinqContains))
                        linqExprSegments[i] = $"\'%{linqExprSegments[i]}%\'";
                    else if (linqExprSegments[i - 1].Contains(LinqStartsWith))
                        linqExprSegments[i] = $"\'{linqExprSegments[i]}%\'";
                    else if (linqExprSegments[i - 1].Contains(LinqEndsWith))
                        linqExprSegments[i] = $"\'%{linqExprSegments[i]}\'";
                }

                linqExprSegments[i - 1] = linqExprSegments[i - 1].ReplaceKeywords();
            }

            whereClause += string.Join(" ", linqExprSegments.Where((_, i) => i >= 2).ToArray()).Replace("  ", " ");
            return whereClause.Trim();
        }
    }
}
