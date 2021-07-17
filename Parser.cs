using System;
using System.Collections.Generic;
using System.Text;

class Parser
{
    const string eof = " ";

    string file;
    string text;
    int ti;
    int line;
    string tok;

    void err(int line, string msg)
    {
        Console.WriteLine("{0}:{1}: {2}", file, line, msg);
        Environment.Exit(1);
    }

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
    }
}
