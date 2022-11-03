using System.Globalization;

public class Lexer
{
    readonly static string[] Symbols = {
        "+", "-", "**", "*", "/", "%", // arithmetic
        "!=", "==", "<<", ">>", "<=", ">=", "<", ">", // comparison
        "&&", "||", "^", "!", // logical
        "&", "|", "~", // bitwise logical
        "(", ")", "$" // other
    };

    string _source;
    int _index = -1;

    public Lexer(string source)
    {
        _source = source;
    }


    bool Get(int index, out char result)
    {
        result = default;
        if (index < 0 || index >= _source.Length) return false;

        result = _source[index];
        return true;
    }
    bool Current(out char result) => Get(_index, out result);
    bool Peek(out char result) => Get(_index + 1, out result);
    bool PeekPrevious(out char result) => Get(_index - 1, out result);
    bool Advance(out char result)
    {
        _index++;
        return Current(out result);
    }
    bool Deadvance(out char result)
    {
        _index--;
        return Current(out result);
    }
    enum NumType
    {
        Int,
        Decimal,
        Exponent,
        Hex,
        Binary
    }
    // false - syntax error
    // true - success
    // if result is null, it means EOF
    public bool Lex(out Token? result)
    {
        result = default;

        while (Advance(out var ch))
        {
            if (char.IsWhiteSpace(ch)) continue;
            if (char.IsLetter(ch))
            {
                var start = _index;
                while (Peek(out ch) && char.IsLetter(ch)) _index++;
                var end = _index + 1;

                result = new Token.Ident(_source.Substring(start, end - start));
                return true;
            }
            if (char.IsDigit(ch) || ch == '.')
            {
                var start = _index;
                var numType = NumType.Int;

                if (ch == '.') numType = NumType.Decimal;
                else if (ch == '0' && Peek(out ch))
                {
                    if (ch == 'x') numType = NumType.Hex;
                    else if (ch == 'b') numType = NumType.Binary;
                }
                if (numType is NumType.Hex or NumType.Binary)
                {
                    _index++;
                    start += 2;
                }

                while (Advance(out ch))
                {
                    ch = char.ToLower(ch);

                    if (ch == '.')
                    {
                        if (numType != NumType.Int) break;
                        numType = NumType.Decimal;

                        continue;
                    }
                    else if (ch == 'e')
                    {
                        if (numType is not NumType.Int and not NumType.Decimal) break;
                        numType = NumType.Exponent;

                        continue;
                    }
                    if (char.IsDigit(ch)) continue;
                    if ((char.IsDigit(ch) || ch is >= 'a' and <= 'f') && numType == NumType.Hex) continue;
                    if (ch is '1' or '0' && numType == NumType.Binary) continue;

                    break;
                }

                var end = _index;
                _index--;
                var src = _source.Substring(start, end - start);

                try
                {
                    switch (numType)
                    {
                        case NumType.Int or NumType.Hex or NumType.Binary:
                            var baseNum = 10;
                            if (numType == NumType.Hex) baseNum = 16;
                            else if (numType == NumType.Binary) baseNum = 2;

                            result = new Token.Number((decimal)Convert.ToInt64(src, baseNum));
                            return true;
                        case NumType.Decimal or NumType.Exponent:
                            result = new Token.Number(decimal.Parse(src, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint));
                            return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            /*
                NOTE TO THE C# DEVS (ESPECIALLY THE DUMBASS ONE TO DESIGN HOW VARIABLES ARE DECLARED):

                if i try to name this `src` i can't because apperently its already defined in this scope so it
                fucking conflicts with the `src` in the if statement up
                ffs this is AFTER THE IF SCOPE ENDED HOW TF CAN THIS MAKE ANY FUCKING SENSE?
                WHY IN THE WORLD IS THIS NOT POSSIBLE?
                ```
                {
                   var src;
                }
                var src; // C#: tHiS iS ErRoR bEcUz I dOnT mAkE seNsE
                ```
            */
            var src1 = _source.AsSpan(_index);

            foreach (var symbol in Symbols)
            {
                if (src1.StartsWith(symbol))
                {
                    _index += symbol.Length - 1;
                    result = new Token.Symbol(symbol);
                    return true;
                }
            }

            return false;
        }

        return true;
    }
}