namespace LinqExpressionsToJS
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Linq.Expressions;

    public class ExpressionConverter
    {
        private readonly Visitor _visitor = new Visitor();

        public string Convert(LambdaExpression expression)
        {
            return string.Concat("function() { return ", _visitor.Visit(expression), "}");
        }

        private class Visitor : DynamicExpressionVisitor
        {
            private readonly Dictionary<string, string> _arrayMaps = new Dictionary<string, string>();

            private readonly StringBuilder _builder = new StringBuilder();
            private readonly Stack<string> _stack = new Stack<string>();

            public Dictionary<string, string> ArrayMaps
            {
                get { return _arrayMaps; }
            }

            public string Visit(LambdaExpression expression)
            {
                base.Visit(expression);
                //while (_stack.Count > 0) _builder.Append(_stack.Pop());
                return _builder.ToString();
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                _stack.Push(node.Member.Name);
                //_builder.Append(node.Member.Name);
                return base.VisitMember(node);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                var left = Visit(node.Left);
                
                _stack.Push(BinaryNodeTypeToOperator(node.NodeType).ToString());
                var right = Visit(node.Right);
                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var ret = base.VisitMethodCall(node);
                var constant = _stack.Pop();
                var fieldName = _stack.Pop();
                var methodName = node.Method.Name;
                var fieldAndMethod = string.Format("{0}.{1}", fieldName, methodName);
                string mapTo;
                if (_arrayMaps.TryGetValue(fieldAndMethod, out mapTo))
                {
                    _builder.AppendFormat("{0}[{1}]", mapTo, constant);                    
                }
                else
                {
                    _builder.AppendFormat("{0}({1})", fieldAndMethod, constant);                                        
                }

                return ret;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (false == IsCompilerGenerated(node.Type))
                {
                    _stack.Push(node.Value.ToString());
                }

                return base.VisitConstant(node);
            }

            private char BinaryNodeTypeToOperator(ExpressionType nodeType)
            {
                char op;
                switch (nodeType)
                {
                    case ExpressionType.Add:
                        op = '+';
                        break;
                    case ExpressionType.Subtract:
                        op = '-';
                        break;
                    case ExpressionType.Multiply:
                        op = '*';
                        break;
                    case ExpressionType.Divide:
                        op = '/';
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                return op;
            }

            private bool IsCompilerGenerated(Type t)
            {
                if (t == null)
                    return false;

                return t.IsDefined(typeof(CompilerGeneratedAttribute), false)
                    || IsCompilerGenerated(t.DeclaringType);
            }
        }

        public void MapToArray(string fieldAndMethodName, string arrayName)
        {
            _visitor.ArrayMaps.Add(fieldAndMethodName, arrayName);
        }

    }
}
