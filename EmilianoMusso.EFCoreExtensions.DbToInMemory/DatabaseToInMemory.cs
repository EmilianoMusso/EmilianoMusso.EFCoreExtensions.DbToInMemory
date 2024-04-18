﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EFCoreExtensions.DbToInMemory.Extensions;

namespace EFCoreExtensions.DbToInMemory
{
    public class DatabaseToInMemory
    {
        private readonly DbContext _context;
        private readonly string _connectionString;
        private readonly bool _randomOrder;
        private DatabaseToInMemoryOptions _options;

        public DatabaseToInMemory(DbContext context, string connectionString, bool randomOrder = true)
        {
            _context = context;
            _connectionString = connectionString;
            _randomOrder = randomOrder;
        }

        public DatabaseToInMemory(DbContext context, DatabaseToInMemoryOptions options)
        {
            _context = context;
            _options = options;
        }

        public DatabaseToInMemory LoadTable<T>(Expression<Func<T, bool>> filter = null, int topRecords = 10) where T : class, new()
        {
            _context.InternalLoadTable(_connectionString, topRecords, _randomOrder, filter);
            return this;
        }

        public DatabaseToInMemory LoadTable<T>(Expression<Func<T, bool>> filter = null) where T : class, new()
        {
            _context.InternalLoadTable(_options, filter);
            return this;
        }

        public int PersistToMemory()
        {
            return _context.SaveChanges();
        }

        public async Task<int> PersistToMemoryAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
