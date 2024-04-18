using System;
using System.Text;

namespace EFCoreExtensions.DbToInMemory.Exceptions;

public class LoadDataException : Exception
{
    private string ExecutedQuery { get; }
    public LoadDataException() { }

    public LoadDataException(string message, string executedQuery) : base(message)
    {
        ExecutedQuery = executedQuery;
    }

    public LoadDataException(string message) : base(message)
    {
        ExecutedQuery = "Not specified";
    }

    public LoadDataException(string message, Exception innerException) : base(message, innerException)
    {
        ExecutedQuery = "Not specified";
    }

    public override string ToString()
    {
        var sb = new StringBuilder(ToString())
            .AppendLine(ExecutedQuery);

        return sb.ToString();
    }
}
