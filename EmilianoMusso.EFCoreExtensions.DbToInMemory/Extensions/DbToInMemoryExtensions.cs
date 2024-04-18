using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EFCoreExtensions.DbToInMemory.Exceptions;
using EFCoreExtensions.DbToInMemory.Helpers;
using EFCoreExtensions.DbToInMemory.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EFCoreExtensions.DbToInMemory.Extensions;

public static class DbToInMemoryExtensions
{
    private static T CreateObject<T>(SqlDataReader dr) where T : class, new()
    {
        var values = new object[dr.FieldCount - 1];
        dr.GetValues(values);

        var obj = new T();
        for (var i = 0; i < values.Length; i++)
        {
            var propertyName = dr.GetName(i);
            obj.GetType().GetProperty(propertyName).SetValue(obj, values[i]);
        }

        return obj;
    }

    public static string GetSelectQuery<T>(DbContext context, DbToInMemoryOptions options, Expression<Func<T, bool>> filter) where T : class
    {
        var mapping = context.Model.FindEntityType(options.TableName);
        var fullTableName = mapping.GetSchemaQualifiedTableName();

        var query = new StringBuilder("SELECT")
            .AppendLine();

        if (options.TopRecords > 0)
        {
            query.AppendLine($"TOP({options.TopRecords})");
        }

        query.AppendLine(string.Join(", ", mapping.GetProperties().Select(x => $"[{x.Name}]")))
             .Append("FROM ")
             .AppendLine(fullTableName);

        if (filter != null)
        {
            var whereClause = filter.ToString().GetSQLWhereClause();
            query.AppendLine(whereClause);
        }

        if (options.HasRandomOrder)
        {
            query.AppendLine("ORDER BY NEWID()");
        }

        return query.ToString();
    }

    public static void ExecuteSQLReader<T>(DbContext context, string connectionString, string query) where T : class, new()
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var sqlCmd = new SqlCommand(query, connection);

            using var dr = sqlCmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var obj = CreateObject<T>(dr);
                    context.Set<T>().Add(obj);
                }
            }
        }
        catch (Exception ex)
        {
            throw new LoadDataException(ex.Message, query);
        }
    }

    public static async Task ExecuteSQLReaderAsync<T>(DbContext context, string connectionString, string query) where T : class, new()
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var sqlCmd = new SqlCommand(query, connection);

            await using var dr = await sqlCmd.ExecuteReaderAsync();
            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    var obj = CreateObject<T>(dr);
                    context.Set<T>().Add(obj);
                }
            }
        }
        catch (Exception ex)
        {
            throw new LoadDataException(ex.Message, query);
        }
    }
}
