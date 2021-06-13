using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EmilianoMusso.EFCoreExtensions.DbToInMemory
{
    public static class DatabaseToInMemoryExtensions
    {
        private static string GetSelectQuery<T>(DatabaseToInMemoryOptions options, Expression<Func<T, bool>> filter) where T : class
        {
            var mapping = options.Context.Model.FindEntityType(options.TableName);
            var fullTableName = mapping.GetSchemaQualifiedTableName();

            var query = new StringBuilder("SELECT")
                .AppendLine();

            if (options.TopRecords > 0) query.AppendLine($"TOP({options.TopRecords})");

            query.AppendLine(string.Join(", ", mapping.GetProperties().Select(x => x.Name)))
                 .Append("FROM ")
                 .AppendLine(fullTableName);

            // ---------------------------------------------------------------------------
            // EXPERIMENTAL: The following code tries to manage a full expression parsing
            // by substituting (work in progress)
            // ---------------------------------------------------------------------------
            if (filter != null)
            {
                var separators = new char[] { ' ', '(', ')' };
                var linqExprSegments = filter.ToString().Split(separators);

                var whereClause = "WHERE ";
                for (int i=4; i<linqExprSegments.Length; i++)
                {
                    linqExprSegments[i] = linqExprSegments[i].Replace("\\", "")
                                                             .Replace("\"", "'")
                                                             .Replace("==", "=")
                                                             .Replace(".Contains", " LIKE")
                                                             .Replace(".StartsWith", " LIKE")
                                                             .Replace(".EndsWith", " LIKE")
                                                             .Replace(linqExprSegments[0] + ".", "");

                    if (linqExprSegments[i].CompareTo("AndAlso") == 0) linqExprSegments[i] = "AND";
                    if (linqExprSegments[i].CompareTo("OrElse") == 0) linqExprSegments[i] = "OR";

                    whereClause += $"{linqExprSegments[i]} ";
                }

                query.AppendLine(whereClause);
            }

            if (options.HasRandomOrder) query.AppendLine("ORDER BY NEWID()");
                
            return query.ToString();
        }

        private static T CreateObject<T>(SqlDataReader dr) where T : class, new()
        {
            var values = new object[dr.FieldCount - 1];
            dr.GetValues(values);
            
            var obj = new T();
            for(int i = 0; i < values.Length; i++)
            {
                var propertyName = dr.GetName(i);
                obj.GetType().GetProperty(propertyName).SetValue(obj, values[i]);
            }

            return obj;
        }

        public static void LoadTableExt<T>(this DbContext context, string connectionString, int topRecords, bool randomOrder, Expression<Func<T, bool>> filter) where T : class, new()
        {
            var options = new DatabaseToInMemoryOptions()
            {
                ConnectionString = connectionString,
                Context = context,
                TableName = typeof(T).ToString(),
                TopRecords = topRecords,
                HasRandomOrder = randomOrder
            };

            using (var connection = new SqlConnection(options.ConnectionString))
            {
                connection.Open();

                var query = GetSelectQuery<T>(options, filter);
                var sqlCmd = new SqlCommand(query, connection);

                var dr = sqlCmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        var obj = CreateObject<T>(dr);
                        context.Set<T>().Add(obj);
                    }
                }
            }
        }
    }
}
