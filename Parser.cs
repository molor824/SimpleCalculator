public class Parser
{
    List<Token> _tokens;
    int _index = -1;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }
    bool Get(int index, out Token token)
    {
        token = null!;

        if (index < 0 || index >= _tokens.Count) return false;

        token = _tokens[index];
        return true;
    }
    bool Current(out Token token) => Get(_index, out token);
    bool Peek(out Token token) => Get(_index + 1, out token);
    bool PeekPrevious(out Token token) => Get(_index - 1, out token);
    bool Advance(out Token token)
    {
        _index++;
        return Current(out token);
    }
    bool Deadvance(out Token token)
    {
        _index--;
        return Current(out token);
    }

    public Expr Expression()
    {
        var or = Or();
        if (or is Expr.Error err) return err;
        return or;
    }
    Expr Or()
    {
        var left = And();
        if (left is Expr.Error err) return err;

        while (Peek(out var token) && token is Token.Symbol { Value: "||" } symbol)
        {
            _index++;

            var right = And();
            if (right is Expr.Error err1) return err1;

            left = new Expr.Binary(left, symbol, right);
        }

        return left;
    }
    Expr And()
    {
        var left = Xor();
        if (left is Expr.Error err) return err;

        while (Peek(out var token) && token is Token.Symbol { Value: "&&" } symbol)
        {
            _index++;

            var right = Xor();
            if (right is Expr.Error err1) return err1;

            left = new Expr.Binary(left, symbol, right);
        }

        return left;
    }
    Expr Xor()
    {
        var left = Equal();
        if (left is Expr.Error err) return err;

        while (Peek(out var token) && token is Token.Symbol { Value: "^" } symbol)
        {
            _index++;

            var right = Equal();
            if (right is Expr.Error err1) return err1;

            left = new Expr.Binary(left, symbol, right);
        }

        return left;
    }
    Expr Equal()
    {
        var left = Compare();
        if (left is Expr.Error err) return err;

        while (Peek(out var token) && token is Token.Symbol { Value: "!=" or "==" } symbol)
        {
            _index++;

            var right = Compare();
            if (right is Expr.Error err1) return err1;

            left = new Expr.Binary(left, symbol, right);
        }

        return left;
    }
    Expr Compare()
    {
        var left = Term();
        if (left is Expr.Error err) return err;

        while (Peek(out var token) && token is Token.Symbol { Value: "<" or ">" or "<=" or ">=" } symbol)
        {
            _index++;

            var right = Term();
            if (right is Expr.Error err1) return err1;

            left = new Expr.Binary(left, symbol, right);
        }

        return left;
    }
    Expr Term()
    {
        var left = Factor();
        if (left is Expr.Error err) return err;

        while (Peek(out var token) && token is Token.Symbol { Value: "+" or "-" } symbol)
        {
            _index++;

            var right = Factor();
            if (right is Expr.Error err1) return err1;

            left = new Expr.Binary(left, symbol, right);
        }

        return left;
    }
    Expr Factor()
    {
        var left = Pow();
        if (left is Expr.Error err) return err;

        while (Peek(out var token) && token is Token.Symbol { Value: "*" or "/" } symbol)
        {
            _index++;

            var right = Pow();
            if (right is Expr.Error err1) return err1;

            left = new Expr.Binary(left, symbol, right);
        }

        return left;
    }
    Expr Pow()
    {
        var left = Unary();
        if (left is Expr.Error err) return err;

        while (Peek(out var token) && token is Token.Symbol { Value: "**" } symbol)
        {
            _index++;

            var right = Unary();
            if (right is Expr.Error err1) return err1;

            left = new Expr.Binary(left, symbol, right);
        }

        return left;
    }
    Expr Unary()
    {
        if (Peek(out var token))
        {
            if (token is Token.Symbol { Value: "-" or "!" } symbol)
            {
                _index++;

                var child = Unary();
                if (child is Expr.Error err) return err;

                return new Expr.Unary(symbol, child);
            }

            var primary = Primary();
            if (primary is Expr.Error err1) return err1;

            return primary;
        }

        return new Expr.Error();
    }
    Expr Primary()
    {
        if (Advance(out var token))
        {
            if (token is Token.Ident { Value: "true" or "false" } boolLit)
                return new Expr.Boolean(boolLit.Value == "true");
            else if (token is Token.Number num)
                return new Expr.Number(num.Value);
            else if (token is Token.Symbol { Value: "$" })
            {
                if (Advance(out token) && token is Token.Ident constant)
                    return new Expr.Constant(constant);
            }
            else if (token is Token.Ident ident)
            {
                if (Advance(out token) && token is Token.Symbol { Value: "(" })
                {
                    var expr = Expression();

                    if (Advance(out token) && token is Token.Symbol { Value: ")" })
                        return new Expr.Function(ident, expr);
                }
            }
            else if (token is Token.Symbol { Value: "(" })
            {
                var expr = Expression();

                if (Advance(out token) && token is Token.Symbol { Value: ")" })
                    return expr;
            }
        }

        return new Expr.Error();
    }
}