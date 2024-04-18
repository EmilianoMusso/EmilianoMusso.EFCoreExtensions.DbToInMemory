using EFCoreExtensions.DbToInMemory.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCoreExtensions.DbToInMemory.Tests;

[TestClass]
internal class LinqFuncToSqlLangHelperTests
{
    [TestMethod]
    public void LINQ_Expression_should_be_translated_to_SQL()
    {
        var linqExpression = "x => x.Property01.Contains(\"A\") AndAlso x.Property02 == 1";
        var expectedClause = "WHERE Property01 LIKE '%A%' AND Property02 = 1";

        var result = linqExpression.GetSQLWhereClause();
        result.Should().Be(expectedClause);

        linqExpression = "test => test.Number != 5 OrElse test.StringValue.StartsWith(\"test\")";
        expectedClause = "WHERE Number <> 5 OR StringValue LIKE 'test%'";

        result = linqExpression.GetSQLWhereClause();
        result.Should().Be(expectedClause);

        linqExpression = "x  =>     x.Property01.Contains(\"A\")   OrElse x.Property02.EndsWith(\"e\") AndAlso    x.Property03  !=    2";
        expectedClause = "WHERE Property01 LIKE '%A%' OR Property02 LIKE '%e' AND Property03 <> 2";

        result = linqExpression.GetSQLWhereClause();
        result.Should().Be(expectedClause);
    }
}
