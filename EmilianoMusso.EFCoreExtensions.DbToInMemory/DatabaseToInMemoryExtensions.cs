using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;

namespace EmilianoMusso.EFCoreExtensions.DbToInMemory
{
    public static class DatabaseToInMemoryExtensions
    {
        private static string GetSelectQuery<T>(DbContext context, string tableName, int topRecords, bool randomOrder) where T : class
        {
            var mapping = context.Model.FindEntityType(tableName);
            var fullTableName = mapping.GetSchemaQualifiedTableName();

            var query = new StringBuilder("SELECT")
                .AppendLine();

            if (topRecords > 0) query.AppendLine($"TOP({topRecords})");

            query.AppendLine(string.Join(", ", mapping.GetProperties().Select(x => x.Name)))
                 .Append("FROM ")
                 .AppendLine(fullTableName);

            if (randomOrder) query.AppendLine("ORDER BY NEWID()");
                
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

        public static void LoadTableExt<T>(this DbContext context, string connectionString, int topRecords, bool randomOrder) where T : class, new()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = GetSelectQuery<T>(context, typeof(T).ToString(), topRecords, randomOrder);
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
