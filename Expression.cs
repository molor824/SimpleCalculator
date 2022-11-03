using System.Text;

public abstract class Expr
{
    public class Binary : Expr
    {
        public Expr Left, Right;
        public Token.Symbol Symbol;

        public Binary(Expr left, Token.Symbol symbol, Expr right)
        {
            Left = left;
            Symbol = symbol;
            Right = right;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var lstrs = (Left.ToString() ?? "").Split('\n');
            var rstrs = (Right.ToString() ?? "").Split('\n');

            builder.Append($"Binary {Symbol.Value}:\n");

            foreach (var lstr in lstrs)
                builder.Append($"  {lstr}\n");

            foreach (var rstr in rstrs)
                builder.Append($"  {rstr}\n");

            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }
        public override EvalValue? Eval()
        {
            var leval = Left.Eval();
            var reval = Right.Eval();

            if (leval is EvalValue.Number lnum && reval is EvalValue.Number rnum)
            {
                switch (Symbol.Value)
                {
                    case "+":
                        lnum.Value += rnum.Value;
                        return lnum;
                    case "-":
                        lnum.Value -= rnum.Value;
                        return lnum;
                    case "*":
                        lnum.Value *= rnum.Value;
                        return lnum;
                    case "/":
                        lnum.Value /= rnum.Value;
                        return lnum;
                    case "**":
                        lnum.Value = (decimal)Math.Pow((double)lnum.Value, (double)rnum.Value);
                        return lnum;
                    case "<":
                        return new EvalValue.Boolean(lnum.Value < rnum.Value);
                    case ">":
                        return new EvalValue.Boolean(lnum.Value > rnum.Value);
                    case "<=":
                        return new EvalValue.Boolean(lnum.Value <= rnum.Value);
                    case ">=":
                        return new EvalValue.Boolean(lnum.Value >= rnum.Value);
                    case "==":
                        return new EvalValue.Boolean(lnum.Value == rnum.Value);
                    case "!=":
                        return new EvalValue.Boolean(lnum.Value != rnum.Value);
                };
            }
            if (leval is EvalValue.Boolean lbool && reval is EvalValue.Boolean rbool)
            {
                switch (Symbol.Value)
                {
                    case "&&":
                        lbool.Value &= rbool.Value;
                        return lbool;
                    case "||":
                        lbool.Value |= rbool.Value;
                        return lbool;
                    case "^":
                        lbool.Value ^= rbool.Value;
                        return lbool;
                }
            }

            return null;
        }
    }
    public class Unary : Expr
    {
        public Expr Child;
        public Token.Symbol Symbol;

        public Unary(Token.Symbol symbol, Expr child)
        {
            Child = child;
            Symbol = symbol;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var strs = (Child.ToString() ?? "").Split('\n');

            builder.Append($"Unary {Symbol.Value}:\n");

            foreach (var str in strs)
                builder.Append($"  {str}\n");

            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }
        public override EvalValue? Eval()
        {
            var eval = Child.Eval();

            if (eval is EvalValue.Number number)
            {
                if (Symbol.Value == "-")
                {
                    number.Value = -number.Value;
                    return number;
                }
            }
            else if (eval is EvalValue.Boolean boolean)
            {
                if (Symbol.Value == "!")
                {
                    boolean.Value = !boolean.Value;
                    return boolean;
                }
            }

            return null;
        }
    }
    public class Boolean : Expr
    {
        public bool Value;
        public Boolean(bool value) { Value = value; }
        public override string ToString()
        {
            return $"Bool {Value}";
        }
        public override EvalValue? Eval() => new EvalValue.Boolean(Value);
    }
    public class Number : Expr
    {
        public decimal Value;

        public Number(decimal value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Number {Value}";
        }
        public override EvalValue? Eval() => new EvalValue.Number(Value);
    }
    public class Constant : Expr
    {
        public Token.Ident Value;
        public Constant(Token.Ident value) { Value = value; }

        static readonly Dictionary<string, decimal> DecimalConstants = new() {
            {"PI", 3.141592653589793238462643m},
            {"e", 2.71828182845904523536m},
            {"PHI", 1.61803398874989484820m}
        };

        public override string ToString()
        {
            return $"Constant: {Value.Value}";
        }

        public override EvalValue? Eval()
        {
            if (DecimalConstants.TryGetValue(Value.Value, out var evalV)) return new EvalValue.Number(evalV);

            return null;
        }
    }
    public class Function : Expr
    {
        static readonly Dictionary<string, Func<decimal, decimal>> NumFunc = new()
        {
            {"sqrt", a => (decimal)Math.Sqrt((double)a)},
            {"sin", a => (decimal)Math.Sin((double)a)},
            {"cos", a => (decimal)Math.Cos((double)a)},
            {"sinh", a => (decimal)Math.Sinh((double)a)},
            {"cosh", a => (decimal)Math.Cosh((double)a)},
            {"tan", a => (decimal)Math.Tan((double)a)},
            {"tanh", a => (decimal)Math.Tanh((double)a)},
            {"asin", a => (decimal)Math.Asin((double)a)},
            {"acos", a => (decimal)Math.Acos((double)a)},
            {"asinh", a => (decimal)Math.Asinh((double)a)},
            {"acosh", a => (decimal)Math.Acosh((double)a)},
            {"floor", Math.Floor},
            {"round", Math.Round},
            {"ceil", Math.Ceiling},
            {"cbrt", a => (decimal)Math.Cbrt((double)a)},
            {"abs", Math.Abs},
            {"sign", a => (decimal)Math.Sign(a)}
        };
        public Expr Child;
        public Token.Ident Name;

        public Function(Token.Ident name, Expr child)
        {
            Child = child;
            Name = name;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var strs = (Child.ToString() ?? "").Split('\n');

            builder.Append($"Function {Name.Value}:\n");

            foreach (var str in strs)
                builder.Append($"  {str}\n");

            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }
        public override EvalValue? Eval()
        {
            var eval = Child.Eval();

            if (eval is EvalValue.Number number)
            {
                if (NumFunc.TryGetValue(Name.Value, out var func))
                {
                    number.Value = func(number.Value);
                    return number;
                }
            }

            return null;
        }
    }
    public class Error : Expr
    {
        public override EvalValue? Eval() => null;
    }

    public abstract EvalValue? Eval();
}
public abstract class EvalValue
{
    public class Boolean : EvalValue
    {
        public bool Value;

        public Boolean(bool value) { Value = value; }
        public override string ToString() => Value.ToString();
    }
    public class Number : EvalValue
    {
        public decimal Value;

        public Number(decimal value) { Value = value; }
        public override string ToString() => Value.ToString();
    }
}