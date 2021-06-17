using System;
using System.Text;

namespace EmilianoMusso.EFCoreExtensions.DbToInMemory
{
    public class LoadDataException : Exception
    {
        public string ExecutedQuery { get; set; }
        public LoadDataException() { }

        public LoadDataException(string message, string executedQuery) : base(message)
        {
            this.ExecutedQuery = executedQuery;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(this.ToString())
                .AppendLine("")
                .AppendLine(this.ExecutedQuery);

            return sb.ToString();
        }
    }
}
