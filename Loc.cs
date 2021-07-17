using System;
using System.Collections.Generic;
using System.Text;

struct Loc
{
    public string file;
    public int line;

    public Loc(string file, int line)
    {
        this.file = file;
        this.line = line;
    }
}
