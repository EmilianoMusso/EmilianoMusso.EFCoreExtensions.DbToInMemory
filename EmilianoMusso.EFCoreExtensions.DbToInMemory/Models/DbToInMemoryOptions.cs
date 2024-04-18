using Microsoft.EntityFrameworkCore;

namespace EFCoreExtensions.DbToInMemory.Models
{
    public class DbToInMemoryOptions
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public int TopRecords { get; set; }
        public bool HasRandomOrder { get; set; }
    }
}
