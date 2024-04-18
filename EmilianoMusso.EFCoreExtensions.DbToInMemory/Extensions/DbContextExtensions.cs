using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFCoreExtensions.DbToInMemory.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreExtensions.DbToInMemory.Extensions
{
    public static class DbContextExtensions
    {
        private static DbToInMemoryOptions CreateOptions<T>(string connectionString, int topRecords, bool randomOrder) where T : class, new()
        {
            return new DbToInMemoryOptions()
            {
                ConnectionString = connectionString,
                TableName = typeof(T).ToString(),
                TopRecords = topRecords,
                HasRandomOrder = randomOrder
            };
        }

        public static void LoadTable<T>(this DbContext context, string connectionString, int topRecords, bool randomOrder, Expression<Func<T, bool>> filter) where T : class, new()
        {
            var options = CreateOptions<T>(connectionString, topRecords, randomOrder);
            LoadTable(context, options, filter);
        }

        public static void LoadTable<T>(this DbContext context, DbToInMemoryOptions options, Expression<Func<T, bool>> filter) where T : class, new()
        {
            var query = DbToInMemoryExtensions.GetSelectQuery(context, options, filter);
            DbToInMemoryExtensions.ExecuteSQLReader<T>(context, options.ConnectionString, query);
        }

        public static async Task LoadTableAsync<T>(this DbContext context, string connectionString, int topRecords, bool randomOrder, Expression<Func<T, bool>> filter) where T : class, new()
        {
            var options = CreateOptions<T>(connectionString, topRecords, randomOrder);
            await LoadTableAsync(context, options, filter);
        }

        public static async Task LoadTableAsync<T>(this DbContext context, DbToInMemoryOptions options, Expression<Func<T, bool>> filter) where T : class, new()
        {
            var query = DbToInMemoryExtensions.GetSelectQuery(context, options, filter);
            await DbToInMemoryExtensions.ExecuteSQLReaderAsync<T>(context, options.ConnectionString, query);
        }
    }
}
