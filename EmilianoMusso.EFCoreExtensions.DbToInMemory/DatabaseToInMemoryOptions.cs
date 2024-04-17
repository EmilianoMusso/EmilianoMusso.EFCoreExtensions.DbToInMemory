using Microsoft.EntityFrameworkCore;

namespace EmilianoMusso.EFCoreExtensions.DbToInMemory
{
    public class DatabaseToInMemoryOptions
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public DbContext Context { get; set; }
        public int TopRecords { get; set; }
        public bool HasRandomOrder { get; set; }
    }
}
