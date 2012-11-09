namespace LinqExpressionsToJS.Tests
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExpressionConverterTests
    {
        [TestMethod]
        public void Should_convert_simple_expression_to_javascript_function()
        {
            var convert = new ExpressionConverter();
            int a, b, c;
            a = b = c = 5;
            Expression<Func<int>> expression = () => a + b - c * a / b;
            var result = expression.Compile()();
            string javascript = convert.Convert(expression);
            Assert.AreEqual("function() {a+b-c*a/b}", javascript);
        }

        [TestMethod]
        public void Now_something_a_bit_more_advanced()
        {
            var convert = new ExpressionConverter();
            var fieldService = new FieldService();
            convert.MapToArray("fieldService.GetField", "felter");
            Expression<Func<Field>> bodyExpr = () => fieldService.GetField(123);
            string javascript = convert.Convert(bodyExpr);
            File.WriteAllText(@"c:\temp\Now_something_a_bit_more_advanced.js", string.Format("var foo = {0};", javascript));
            Assert.IsNotNull(javascript);
        }
    }

    public class FieldService
    {
        public Field GetField(int fieldId)
        {
            return new Field();
        }
    }

    public static class Fields
    {
        public static Field Id(int index)
        {
            return new Field();
        }
    }

    public class Field
    {
        public int Value { get; set; }
    }
}