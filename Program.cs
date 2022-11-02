var debug = args.Contains("--debug");

Console.WriteLine("Calculator. Type `exit` to exit the program.");

while (true)
{
    Console.Write(">>> ");

    var input = Console.ReadLine();

    if (input == null) continue;
    if (input == "exit") return;

    var lexer = new Lexer(input);
    var tokens = new List<Token>();
    var error = true;

    if (debug) Console.WriteLine("\nTokens:");

    while (lexer.Lex(out var token))
    {
        if (token != null)
        {
            if (debug) Console.WriteLine($"{token.GetType()}: {token}");
            tokens.Add(token);
        }
        else
        {
            error = false;
            break;
        }
    }

    if (error)
    {
        Console.WriteLine("Syntax error.");
        continue;
    }

    var expression = new Parser(tokens).Expression();

    if (debug) Console.WriteLine($"\nSyntax tree:\n{expression}");

    var eval = expression.Eval();

    if (eval == null)
    {
        Console.WriteLine("Syntax error.");
        continue;
    }

    Console.WriteLine(eval);
}