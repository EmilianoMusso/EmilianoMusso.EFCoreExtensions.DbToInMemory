using FluentAssertions;
using EmilianoMusso.EFCoreExtensions.DbToInMemory.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbToInMemoryTests
{
    [TestClass]
    public class LinqFuncToSqlLangHelperTests
    {
        [TestMethod]
        public void LINQ_Expression_should_be_translated_to_SQL()
        {
            var linqExpression = "x => x.Property01.Contains(\"A\") AndAlso x.Property02 == 1";
            var expectedClause = "WHERE Property01 LIKE '%A%' AND Property02 = 1";

            var result = LinqFuncToSqlLangHelper.GetSQLWhereClause(linqExpression);
            result.Should().Be(expectedClause);

            linqExpression = "test => test.Number != 5 OrElse test.StringValue.StartsWith(\"test\")";
            expectedClause = "WHERE Number <> 5 OR StringValue LIKE 'test%'";

            result = LinqFuncToSqlLangHelper.GetSQLWhereClause(linqExpression);
            result.Should().Be(expectedClause);

        }
    }
}
