#pragma warning disable IDE0057 // Use range operator
using System;
using System.Collections.Generic;
using System.Text;

static class Parser
{
    const string eof = " ";

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

    static string qtok(string t)
    {
        switch (t)
        {
            case eof:
                return "EOF";
            case "\n":
                return "newline";
        }
        return '\'' + t + '\'';
    }

    public static void parse(string file, string text)
    {
        if (!text.EndsWith('\n'))
            text += '\n';

        int ti = 0;
        int line = 1;
        string tok;

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
            if (c == '\n')
            {
                ti++;
                line++;
                tok = "\n";
                return;
            }
            if (c == ';')
            {
                ti++;
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
                while (isWordPart(c))
                    ti++;
                if (text[ti] == '.')
                    do
                        ti++;
                    while (isWordPart(c));
                switch (text[ti - 1])
                {
                    case 'p':
                    case 'P':
                        if (text[ti] == '-' || text[ti] == '+')
                        {
                            ti++;
                            while (isWordPart(c))
                                ti++;
                        }
                        break;
                }
                tok = substr(text, i, ti);
                return;
            }
            if (isDigit(c))
            {
                while (isWordPart(c))
                    ti++;
                if (text[ti] == '.')
                    do
                        ti++;
                    while (isWordPart(c));
                switch (text[ti - 1])
                {
                    case 'e':
                    case 'E':
                        if (text[ti] == '-' || text[ti] == '+')
                        {
                            ti++;
                            while (isWordPart(c))
                                ti++;
                        }
                        break;
                }
                tok = substr(text, i, ti);
                return;
            }

            //punctuation
            switch (substr(text, i, 3))
            {
                case "<<=":
                case ">>=":
                    ti += 3;
                    tok = substr(text, i, ti);
                    return;
            }
            switch (substr(text, i, 2))
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
                case "^=":
                    ti += 2;
                    tok = substr(text, i, ti);
                    return;
            }
            ti++;
            tok = substr(text, i, ti);
        }

        //parser
        bool eat(string t)
        {
            if (tok == t)
            {
                lex();
                return true;
            }
            return false;
        }

        void expect(string t)
        {
            if (!eat(t))
                err(line, string.Format("{0}: expected {1}", qtok(tok), qtok(t)));
        }

        //expressions
        Term primary()
        {
            var lin = line;
            var loc = new Loc(file, lin);
            var t = tok;
            lex();

            //specific token
            switch (t)
            {
                case "false":
                    return new Term(loc, Tag.False);
            }

            //none of the above
            err(lin, "expected expression");
            return null;
        }

        lex();
    }
}
