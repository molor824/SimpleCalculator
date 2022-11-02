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

    public Expr Expression() => Or();
    Expr Or()
    {
        var left = And();

        while (Peek(out var token) && token is Token.Symbol { Value: "||" } symbol)
        {
            _index++;
            left = new Expr.Binary(left, symbol, And());
        }

        return left;
    }
    Expr And()
    {
        var left = Xor();

        while (Peek(out var token) && token is Token.Symbol { Value: "&&" } symbol)
        {
            _index++;
            left = new Expr.Binary(left, symbol, Xor());
        }

        return left;
    }
    Expr Xor()
    {
        var left = Equal();

        while (Peek(out var token) && token is Token.Symbol { Value: "^" } symbol)
        {
            _index++;
            left = new Expr.Binary(left, symbol, Equal());
        }

        return left;
    }
    Expr Equal()
    {
        var left = Compare();

        while (Peek(out var token) && token is Token.Symbol { Value: "!=" or "==" } symbol)
        {
            _index++;
            left = new Expr.Binary(left, symbol, Compare());
        }

        return left;
    }
    Expr Compare()
    {
        var left = Term();

        while (Peek(out var token) && token is Token.Symbol { Value: "<" or ">" or "<=" or ">=" } symbol)
        {
            _index++;
            left = new Expr.Binary(left, symbol, Term());
        }

        return left;
    }
    Expr Term()
    {
        var left = Factor();

        while (Peek(out var token) && token is Token.Symbol { Value: "+" or "-" } symbol)
        {
            _index++;
            left = new Expr.Binary(left, symbol, Factor());
        }

        return left;
    }
    Expr Factor()
    {
        var left = Pow();

        while (Peek(out var token) && token is Token.Symbol { Value: "*" or "/" } symbol)
        {
            _index++;
            left = new Expr.Binary(left, symbol, Pow());
        }

        return left;
    }
    Expr Pow()
    {
        var left = Unary();

        while (Peek(out var token) && token is Token.Symbol { Value: "**" } symbol)
        {
            _index++;
            left = new Expr.Binary(left, symbol, Unary());
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
                return new Expr.Unary(symbol, Unary());
            }

            return Primary();
        }

        return new Expr.Error();
    }
    Expr Primary()
    {
        if (Advance(out var token))
        {
            if (token is Token.Ident { Value: "true" or "false" } boolLit)
                return new Expr.Boolean(boolLit.Value == "true");
            if (token is Token.Ident { Value: "PI" })
                return new Expr.Number((decimal)Math.PI);
            if (token is Token.Number num)
                return new Expr.Number(num.Value);
            if (token is Token.Ident ident)
            {
                if (Advance(out token) && token is Token.Symbol { Value: "(" })
                {
                    var expr = Expression();

                    if (Advance(out token) && token is Token.Symbol { Value: ")" })
                        return new Expr.Function(ident, expr);
                }
            }
            if (token is Token.Symbol { Value: "(" })
            {
                var expr = Expression();

                if (Advance(out token) && token is Token.Symbol { Value: ")" })
                    return new Expr.Group(expr);
            }
        }

        return new Expr.Error();
    }
}