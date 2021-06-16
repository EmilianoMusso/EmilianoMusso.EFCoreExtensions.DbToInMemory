using System.Collections.Generic;
using System.Linq;

namespace EmilianoMusso.EFCoreExtensions.DbToInMemory.Helpers
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

        private static Dictionary<string, string> Keywords
        {
            get
            {
                var keyDictionary = new Dictionary<string, string>();

                keyDictionary.Add(LinqAndAlso, SqlAnd);
                keyDictionary.Add(LinqOrElse, SqlOr);
                keyDictionary.Add(LinqContains, SqlLike);
                keyDictionary.Add(LinqStartsWith, SqlLike);
                keyDictionary.Add(LinqEndsWith, SqlLike);

                return keyDictionary;
            }
        }

        private static Dictionary<string, string> Operators
        {
            get
            {
                var opDictionary = new Dictionary<string, string>();

                opDictionary.Add(LinqSlash, SqlSlash);
                opDictionary.Add(LinqQuote, SqlQuote);
                opDictionary.Add(LinqEqual, SqlEqual);
                opDictionary.Add(LinqNotEqual, SqlNotEqual);

                return opDictionary;
            }
        }

        public static string GetKeyword(string key)
        {
            Keywords.TryGetValue(key, out string sqlFunc);
            return sqlFunc;
        }

        public static string GetOperator(string key)
        {
            Operators.TryGetValue(key, out string sqlOperator);
            return sqlOperator;
        }

        public static string ReplaceOperators(this string linqExpression)
        {
            foreach(var op in Operators)
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
            var linqExprSegments = linqWhereExpression.ToString().Split(separators);

            var whereClause = LinqFuncToSqlLangHelper.WhereFunc;

            for (int i = 2; i < linqExprSegments.Length; i++)
            {
                linqExprSegments[i] = linqExprSegments[i].Replace(linqExprSegments[0] + ".", "")
                                                         .ReplaceOperators();

                if (linqExprSegments[i].Any(x => x == (char)39))
                {
                    if (linqExprSegments[i - 1].Contains(LinqFuncToSqlLangHelper.LinqContains))
                    {
                        linqExprSegments[i] = linqExprSegments[i].First() + "%" + linqExprSegments[i].Substring(1, linqExprSegments[i].Length - 2) + "%'";
                    }
                    else if (linqExprSegments[i - 1].Contains(LinqFuncToSqlLangHelper.LinqStartsWith))
                    {
                        linqExprSegments[i] = linqExprSegments[i].Substring(0, linqExprSegments[i].Length - 1) + "%'";
                    }
                    else if (linqExprSegments[i - 1].Contains(LinqFuncToSqlLangHelper.LinqEndsWith))
                    {
                        linqExprSegments[i] = linqExprSegments[i].First() + "%" + linqExprSegments[i].Substring(1, linqExprSegments[i].Length - 1);
                    }
                }

                linqExprSegments[i - 1] = linqExprSegments[i - 1].ReplaceKeywords();
            }

            whereClause += string.Join(" ", linqExprSegments.Where((x, i) => i >= 2).ToArray()).Replace("  ", " ");
            return whereClause.Trim();
        }
    }
}
