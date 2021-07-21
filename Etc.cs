using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

static class Etc
{
    public static void err(Loc loc, string msg)
    {
        Console.Error.WriteLine("{0}:{1}: {2}", loc.file, loc.line, msg);
        Environment.Exit(1);
    }

    public static void debug(object a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        Console.WriteLine("{0}:{1}: {2}", file, line, a);
    }
}
