using System;
using System.Collections.Generic;
using System.Text;

class Module
{
    public string file;
    public List<Term> publicSection = new List<Term>();
    public List<Term> privateSection = new List<Term>();

    public Module(string file)
    {
        this.file = file;
    }
}
