using System;
using System.Text;

namespace EmilianoMusso.EFCoreExtensions.DbToInMemory
{
    public class LoadDataException : Exception
    {
        private string ExecutedQuery { get; set; }
        public LoadDataException() { }

        public LoadDataException(string message, string executedQuery) : base(message)
        {
            this.ExecutedQuery = executedQuery;
        }

        public LoadDataException(string message) : base(message)
        {
        }

        public LoadDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public override string ToString()
        {
            var sb = new StringBuilder(this.ToString())
                .AppendLine(this.ExecutedQuery);

            return sb.ToString();
        }
    }
}
