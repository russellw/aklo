using System;
using System.Collections.Generic;
using System.Text;

static class Etc
{
    public static void err(Loc loc, string msg)
    {
        Console.WriteLine("{0}:{1}: {2}", loc.file, loc.line, msg);
        Environment.Exit(1);
    }
}
