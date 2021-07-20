using System;
using System.Collections.Generic;
using System.Text;

static class Parser
{
    const string eof = "";

    static char at(string s, int i)
    {
        if (i < s.Length)
            return s[i];
        return '\0';
    }

    static bool parseInt(string s, int base_, out int r)
    {
        r = 0;
        if (s.Length == 0)
            return false;
        foreach (var c in s)
        {
            var d = digit(c);
            if (d >= base_)
                return false;
            r = r * base_ + d;
        }
        return true;
    }

    static bool isDigit(char c)
    {
        return '0' <= c && c <= '9';
    }

    static bool isAlpha(char c)
    {
        if ('a' <= c && c <= 'z')
            return true;
        if ('A' <= c && c <= 'Z')
            return true;
        return false;
    }

    static bool isWordStart(char c)
    {
        return isAlpha(c) || c == '_';
    }

    static bool isWordPart(char c)
    {
        return isWordStart(c) || isDigit(c);
    }

    static string substr(string s, int i, int j)
    {
        j = Math.Min(j, s.Length);
        return s.Substring(i, j - i);
    }

    static int digit(char c)
    {
        if ('0' <= c && c <= '9')
            return c - '0';
        if ('a' <= c && c <= 'z')
            return c - 'a' + 10;
        if ('A' <= c && c <= 'Z')
            return c - 'A' + 10;
        return 99;
    }

    static bool isId(string s)
    {
        if (!isWordStart(s[0]))
            return false;
        switch (s)
        {
            case "true":
            case "false":
                return false;
        }
        return true;
    }

    static string utf8(int c)
    {
        var sb = new StringBuilder();
        if (c < 0x80)
        {
            sb.Append((char)c);
            return sb.ToString();
        }
        if (c < 0x800)
        {
            sb.Append((char)(0xa0 | c >> 6));
            sb.Append((char)(0x80 | (c & 0x3f)));
            return sb.ToString();
        }
        if (c < 0x10000)
        {
            sb.Append((char)(0xe0 | c >> 12));
            sb.Append((char)(0x80 | (c >> 6 & 0x3f)));
            sb.Append((char)(0x80 | (c & 0x3f)));
            return sb.ToString();
        }
        sb.Append((char)(0xf0 | c >> 18));
        sb.Append((char)(0x80 | (c >> 12 & 0x3f)));
        sb.Append((char)(0x80 | (c >> 6 & 0x3f)));
        sb.Append((char)(0x80 | (c & 0x3f)));
        return sb.ToString();
    }

    static string unquote(string s)
    {
        var i = 1;
        var sb = new StringBuilder();
        while (i < s.Length - 1)
        {
            var c = s[i++];
            if (c != '\\')
            {
                sb.Append(c);
                continue;
            }
            c = s[i++];
            switch (c)
            {
                case 'a':
                    sb.Append('\a');
                    break;
                case 'b':
                    sb.Append('\b');
                    break;
                case 'f':
                    sb.Append('\f');
                    break;
                case 'n':
                    sb.Append('\n');
                    break;
                case 'r':
                    sb.Append('\r');
                    break;
                case 't':
                    sb.Append('\t');
                    break;
                case 'v':
                    sb.Append('\v');
                    break;
                case 'e':
                    sb.Append('\x1b');
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                    {
                        i--;
                        var j = i + 3;
                        var n = 0;
                        while (digit(s[i]) < 8 && i < j)
                            n = n * 8 + digit(s[i++]);
                        sb.Append((char)n);
                        break;
                    }
                case 'x':
                    {
                        var j = i + 2;
                        var n = 0;
                        while (digit(s[i]) < 16 && i < j)
                            n = n * 16 + digit(s[i++]);
                        sb.Append((char)n);
                        break;
                    }
                case 'u':
                case 'U':
                    {
                        var j = i + 4;
                        if (c == 'U')
                            j += 4;
                        var n = 0;
                        while (digit(s[i]) < 16 && i < j)
                            n = n * 16 + digit(s[i++]);
                        sb.Append(utf8(n));
                        break;
                    }
                default:
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    static string qtok(string s)
    {
        switch (s)
        {
            case eof:
                return "EOF";
            case "\n":
                return "newline";
        }
        return '\'' + s + '\'';
    }

    public static List<Module> parse(string file, string text)
    {
        if (!text.EndsWith("\n"))
            text += '\n';

        int ti = 0;
        int line = 1;
        string tok = "\n";

        void err(int line, string msg)
        {
            Console.WriteLine("{0}:{1}: {2}", file, line, msg);
            Environment.Exit(1);
        }

        //tokenizer
        void lex()
        {
            var lin = line;

            //eof
            if (ti >= text.Length)
            {
                tok = eof;
                return;
            }

            var c = text[ti];

            //newline
            if (c == '\n' || c == ';')
            {
                ti++;
                if (c == '\n')
                    line++;
                if (tok == "\n")
                    lex();
                else
                    tok = "\n";
                return;
            }

            //space
            if (c <= ' ')
            {
                ti++;
                lex();
                return;
            }

            //comment
            if (text.Substring(ti, 2) == "//")
            {
                while (text[ti] != '\n')
                    ti++;
                lex();
                return;
            }
            if (text.Substring(ti, 2) == "/*")
            {
                ti += 2;
                for (; ; )
                {
                    if (ti + 1 >= text.Length)
                        err(lin, "unclosed comment");
                    if (text.Substring(ti, 2) == "*/")
                    {
                        ti += 2;
                        lex();
                        return;
                    }
                    if (text[ti] == '\n')
                        line++;
                    ti++;
                }
            }

            var i = ti;

            //word
            if (isWordStart(c))
            {
                while (isWordPart(text[ti]))
                    ti++;
                tok = substr(text, i, ti);
                return;
            }

            //string or character 
            if (c == '"' || c == '\'')
            {
                ti++;
                while (text[ti] != c)
                {
                    if (text[ti] == '\\')
                        ti++;
                    if (text[ti] == '\n')
                        err(lin, "unclosed quote");
                    ti++;
                }
                ti++;
                tok = substr(text, i, ti);
                return;
            }

            //number
            if (c == '0' && (text[ti + 1] == 'x' || text[ti + 1] == 'X'))
            {
                while (isWordPart(text[ti]))
                    ti++;
                if (text[ti] == '.')
                    do
                        ti++;
                    while (isWordPart(text[ti]));
                switch (text[ti - 1])
                {
                    case 'p':
                    case 'P':
                        if (text[ti] == '-' || text[ti] == '+')
                        {
                            ti++;
                            while (isWordPart(text[ti]))
                                ti++;
                        }
                        break;
                }
                tok = substr(text, i, ti);
                return;
            }
            if (isDigit(c) || (c == '.' && isDigit(text[ti + 1])))
            {
                while (isWordPart(text[ti]))
                    ti++;
                if (text[ti] == '.')
                    do
                        ti++;
                    while (isWordPart(text[ti]));
                switch (text[ti - 1])
                {
                    case 'e':
                    case 'E':
                        if (text[ti] == '-' || text[ti] == '+')
                        {
                            ti++;
                            while (isWordPart(text[ti]))
                                ti++;
                        }
                        break;
                }
                tok = substr(text, i, ti);
                return;
            }

            //punctuation
            switch (substr(text, i, i + 3))
            {
                case "<<=":
                case ">>=":
                    ti += 3;
                    tok = substr(text, i, ti);
                    return;
            }
            switch (substr(text, i, i + 2))
            {
                case "==":
                case "!=":
                case "<=":
                case ">=":
                case "+=":
                case "++":
                case "--":
                case "*=":
                case "-=":
                case "/=":
                case "%=":
                case "<<":
                case ">>":
                case "@=":
                case "&&":
                case "||":
                case "&=":
                case "|=":
                case ":=":
                case "^=":
                    ti += 2;
                    tok = substr(text, i, ti);
                    return;
            }
            ti++;
            tok = substr(text, i, ti);
        }

        //parser
        bool eat(string s)
        {
            if (tok == s)
            {
                lex();
                return true;
            }
            return false;
        }

        void expect(string s)
        {
            if (!eat(s))
                err(line, string.Format("{0}: expected {1}", qtok(tok), qtok(s)));
        }

        string id()
        {
            var s = tok;
            if (!isId(s))
                err(line, string.Format("{0}: expected identifier", qtok(s)));
            lex();
            return s;
        }

        //types
        Term type()
        {
            var loc = new Loc(file, line);
            var s = tok;
            lex();
            switch (s)
            {
                case "int":
                    return new Term(loc, Tag.Int);
                case "char":
                    return new Term(loc, Tag.Char);
                case "float":
                    return new Term(loc, Tag.Float);
                case "double":
                    return new Term(loc, Tag.Double);
                case "void":
                    return new Term(loc, Tag.Void);
                case "bool":
                    return new Term(loc, Tag.Bool);
            }
            err(loc.line, string.Format("{0}: expected type", qtok(s)));
            return null;
        }

        Term param()
        {
            var loc = new Loc(file, line);
            var a = new Term(loc, Tag.Var, id());
            eat(":");
            a.type = type();
            return a;
        }

        List<Term> params_()
        {
            expect("(");
            var r = new List<Term>();
            if (tok != ")")
                do
                    r.Add(param());
                while (eat(","));
            expect(")");
            return r;
        }

        //expressions
        void args(Term a, string rbracket)
        {
            if (tok != rbracket)
                do
                    a.add(expr());
                while (eat(","));
            expect(rbracket);
        }

        Term primary()
        {
            var loc = new Loc(file, line);
            var s = tok;
            lex();

            //specific token
            switch (s)
            {
                case "false":
                    return new Term(loc, Tag.False);
                case "true":
                    return new Term(loc, Tag.True);
                case "(":
                    {
                        var a = expr();
                        expect(")");
                        return a;
                    }
                case "[":
                    {
                        var a = new Term(loc, Tag.List);
                        args(a, "]");
                        return a;
                    }
            }

            //identifier
            if (isId(s))
                return new Term(loc, Tag.Ref, s);

            //character
            if (s[0] == '\'')
            {
                s = unquote(s);
                if (s.Length != 1)
                    err(loc.line, "expected one character");
                return new Term(loc, s[0]);
            }

            //string
            if (s[0] == '"')
            {
                s = unquote(s);
                var a = new Term(loc, Tag.List);
                foreach (var c in s)
                    a.contents.Add(new Term(loc, c));
                return a;
            }

            //number
            if (isDigit(at(s, 0)) || at(s, 0) == '.' && isDigit(at(s, 1)))
            {
                var t = s.Replace("_", "");
                int n;
                switch (substr(t, 0, 2))
                {
                    case "0x":
                    case "0X":
                        if (parseInt(substr(t, 2, t.Length), 16, out n))
                            return new Term(loc, n);
                        break;
                    case "0b":
                    case "0B":
                        if (parseInt(substr(t, 2, t.Length), 2, out n))
                            return new Term(loc, n);
                        break;
                    case "0o":
                    case "0O":
                        if (parseInt(substr(t, 2, t.Length), 8, out n))
                            return new Term(loc, n);
                        break;
                }
                if (parseInt(t, 10, out n))
                    return new Term(loc, n);
                if (t.EndsWith("f") || t.EndsWith("F"))
                {
                    t = substr(t, 0, t.Length - 1);
                    if (float.TryParse(t, out var r))
                        return new Term(loc, r);
                    err(loc.line, string.Format("{0}: expected float", qtok(s)));
                }
                if (double.TryParse(t, out var r1))
                    return new Term(loc, r1);
                err(loc.line, string.Format("{0}: expected number", qtok(s)));
            }

            //none of the above
            err(loc.line, string.Format("{0}: expected expression", qtok(s)));
            return null;
        }

        Term postfix()
        {
            var a = primary();
            for (; ; )
            {
                var loc = new Loc(file, line);
                switch (tok)
                {
                    case "++":
                        lex();
                        return new Term(loc, Tag.PostInc, a);
                    case "--":
                        lex();
                        return new Term(loc, Tag.PostDec, a);
                    case "[":
                        lex();
                        a = new Term(loc, Tag.Subscript, a, expr());
                        expect("]");
                        break;
                    case "(":
                        lex();
                        a = new Term(loc, Tag.Call, a);
                        args(a, ")");
                        break;
                    default:
                        return a;
                }
            }
        }

        Term prefix()
        {
            var loc = new Loc(file, line);
            switch (tok)
            {
                case "++":
                    lex();
                    return new Term(loc, Tag.Inc, prefix());
                case "--":
                    lex();
                    return new Term(loc, Tag.Dec, prefix());
                case "!":
                    lex();
                    return new Term(loc, Tag.Not, prefix());
                case "-":
                    lex();
                    return new Term(loc, Tag.Neg, prefix());
                case "~":
                    lex();
                    return new Term(loc, Tag.BitNot, prefix());
            }
            return postfix();
        }

        // operator precedence parser
        int prec0 = 99;
        Dictionary<string, Op> ops = new Dictionary<string, Op>();

        void op(string name, int left = 1)
        {
            ops.Add(name, new Op(prec0, left));
        }

        // multiplicative
        prec0--;
        op("*");
        op("/");
        op("%");

        // additive
        prec0--;
        op("+");
        op("-");
        op("@");

        // shift
        prec0--;
        op("<<");
        op(">>");

        // relational
        prec0--;
        op("<");
        op(">");
        op("<=");
        op(">=");

        // equality
        prec0--;
        op("!=");
        op("==");

        // bitwise and
        prec0--;
        op("&");

        // bitwise xor
        prec0--;
        op("^");

        // bitwise or
        prec0--;
        op("|");

        // and
        prec0--;
        op("&&");

        // or
        prec0--;
        op("||");

        // conditional
        prec0--;
        op("?", 0);

        // assignment
        prec0--;
        op("=", 0);
        op("*=", 0);
        op("/=", 0);
        op("%=", 0);
        op("+=", 0);
        op("-=", 0);
        op("<<=", 0);
        op(">>=", 0);
        op("&=", 0);
        op("^=", 0);
        op("|=", 0);
        op(":=", 0);

        Term infix(int prec)
        {
            var a = prefix();
            for (; ; )
            {
                var s = tok;
                if (!ops.TryGetValue(s, out var o))
                    return a;
                if (o.prec < prec)
                    return a;
                var loc = new Loc(file, line);
                lex();
                var b = infix(o.prec + o.left);
                switch (s)
                {
                    // multiplicative
                    case "*":
                        a = new Term(loc, Tag.Mul, a, b);
                        break;
                    case "/":
                        a = new Term(loc, Tag.Div, a, b);
                        break;
                    case "%":
                        a = new Term(loc, Tag.Rem, a, b);
                        break;

                    // additive
                    case "+":
                        a = new Term(loc, Tag.Add, a, b);
                        break;
                    case "-":
                        a = new Term(loc, Tag.Sub, a, b);
                        break;

                    // shift
                    case "<<":
                        a = new Term(loc, Tag.Shl, a, b);
                        break;
                    case ">>":
                        a = new Term(loc, Tag.Shr, a, b);
                        break;

                    // relational
                    case "<":
                        a = new Term(loc, Tag.Lt, a, b);
                        break;
                    case ">":
                        a = new Term(loc, Tag.Lt, b, a);
                        break;
                    case "<=":
                        a = new Term(loc, Tag.Le, a, b);
                        break;
                    case ">=":
                        a = new Term(loc, Tag.Le, b, a);
                        break;

                    // equality
                    case "==":
                        a = new Term(loc, Tag.Eq, a, b);
                        break;
                    case "!=":
                        a = new Term(loc, Tag.Not, new Term(loc, Tag.Eq, a, b));
                        break;

                    // bitwise and
                    case "&":
                        a = new Term(loc, Tag.BitAnd, a, b);
                        break;

                    // bitwise xor
                    case "^":
                        a = new Term(loc, Tag.BitXor, a, b);
                        break;

                    // bitwise or
                    case "|":
                        a = new Term(loc, Tag.BitOr, a, b);
                        break;

                    // and
                    case "&&":
                        a = new Term(loc, Tag.And, a, b);
                        break;

                    // or
                    case "||":
                        a = new Term(loc, Tag.Or, a, b);
                        break;

                    // conditional
                    case "?":
                        expect(":");
                        a = new Term(loc, Tag.IfExpr, a, b, infix(o.prec + o.left));
                        break;

                    // assignment
                    case "=":
                        a = new Term(loc, Tag.Assign, a, b);
                        break;
                    case "*=":
                        a = new Term(loc, Tag.MulAssign, a, b);
                        break;
                    case "/=":
                        a = new Term(loc, Tag.DivAssign, a, b);
                        break;
                    case "%=":
                        a = new Term(loc, Tag.RemAssign, a, b);
                        break;
                    case "+=":
                        a = new Term(loc, Tag.AddAssign, a, b);
                        break;
                    case "-=":
                        a = new Term(loc, Tag.SubAssign, a, b);
                        break;
                    case "<<=":
                        a = new Term(loc, Tag.ShlAssign, a, b);
                        break;
                    case ">>=":
                        a = new Term(loc, Tag.ShrAssign, a, b);
                        break;
                    case "&=":
                        a = new Term(loc, Tag.BitAndAssign, a, b);
                        break;
                    case "^=":
                        a = new Term(loc, Tag.BitXorAssign, a, b);
                        break;
                    case "|=":
                        a = new Term(loc, Tag.BitOrAssign, a, b);
                        break;
                    case ":=":
                        if (a.tag != Tag.Ref)
                            Etc.err(a.loc, "expected identifier");
                        a = new Term(loc, Tag.Var, a.name);
                        a.add(b);
                        break;

                    // compiler bug
                    default:
                        throw new ArgumentException(s);
                }
            }
        }

        Term expr()
        {
            return infix(0);
        }

        //statements
        Term parseIf()
        {
            var loc = new Loc(file, line);
            lex();
            var a = new Term(loc, Tag.If, expr());
            expect("\n");
            a.add(stmts());
            switch (tok)
            {
                case "elif":
                    a.add(parseIf());
                    break;
                case "else":
                    lex();
                    expect("\n");
                    a.add(stmts());
                    break;
            }
            return a;
        }

        Term stmt()
        {
            var loc = new Loc(file, line);
            Term a;
            switch (tok)
            {
                case "fn":
                    lex();
                    a = new Term(loc, Tag.Fn, id());
                    a.params_ = params_();
                    eat(":");
                    a.type = tok == "\n" ? new Term(loc, Tag.Void) : type();
                    expect("\n");
                    a.add(stmts());
                    expect("end");
                    break;
                case "break":
                    lex();
                    a = new Term(loc, Tag.Break);
                    break;
                case "continue":
                    lex();
                    a = new Term(loc, Tag.Continue);
                    break;
                case "assert":
                    lex();
                    a = new Term(loc, Tag.Assert, expr());
                    break;
                case "debug":
                    lex();
                    a = new Term(loc, Tag.Debug, expr());
                    break;
                case "goto":
                    lex();
                    a = new Term(loc, Tag.Goto, id());
                    break;
                case ":":
                    lex();
                    a = new Term(loc, Tag.Label, id());
                    break;
                case "if":
                    a = parseIf();
                    expect("end");
                    break;
                case "while":
                    lex();
                    a = new Term(loc, Tag.While, expr());
                    expect("\n");
                    a.add(stmts());
                    expect("end");
                    break;
                case "dowhile":
                    lex();
                    a = new Term(loc, Tag.DoWhile, expr());
                    expect("\n");
                    a.add(stmts());
                    expect("end");
                    break;
                case "for":
                    lex();
                    a = new Term(loc, Tag.For, expr());
                    expect("in");
                    a.add(expr());
                    expect("\n");
                    a.add(stmts());
                    expect("end");
                    break;
                case "return":
                    lex();
                    a = new Term(loc, Tag.Return);
                    if (tok != "\n")
                        a.add(expr());
                    break;
                default:
                    a = expr();
                    break;
            }
            expect("\n");
            return a;
        }

        Term stmts()
        {
            var loc = new Loc(file, line);
            var a = new Term(loc, Tag.Block);
            for (; ; )
            {
                switch (tok)
                {
                    case "end":
                    case "else":
                    case "elif":
                        return a;
                    default:
                        a.add(stmt());
                        break;
                }
            }
        }

        lex();
        var module = new Module();
        while (tok != eof)
        {
            if (eat("private"))
            {
                eat(":");
                expect("\n");
                break;
            }
            module.publicSection.Add(stmt());
        }
        while (tok != eof)
            module.privateSection.Add(stmt());
        var program = new List<Module>();
        program.Add(module);
        return program;
    }

    class Op
    {
        public readonly int prec;
        public readonly int left;

        public Op(int prec, int left)
        {
            this.prec = prec;
            this.left = left;
        }
    }
}
