public abstract class Token
{
    public class Ident : Token
    {
        public string Value;

        public Ident(string value) { Value = value; }
        public override string ToString() => Value;
    }
    public class Number : Token
    {
        public decimal Value;

        public Number(decimal value) { Value = value; }
        public override string ToString() => Value.ToString();
    }
    public class Symbol : Token
    {
        public string Value;

        public Symbol(string value) { Value = value; }
        public override string ToString() => Value.ToString();
    }
}