using System;
using System.Collections.Generic;
using System.Text;

class Term
{
    public readonly Loc loc;
    public readonly Tag tag;
    public readonly string s;
    public object atom;
    public readonly List<Term> contents = new List<Term>();

    public Term(Loc loc, Tag tag, params Term[] s)
    {
        this.loc = loc;
        this.tag = tag;
        foreach (var a in s)
            contents.Add(a);
    }

    public Term(Loc loc, Tag tag, string s)
    {
        this.loc = loc;
        this.tag = tag;
        this.s = s;
    }

    public Term(Loc loc, int n)
    {
        this.loc = loc;
        this.tag = Tag.Int;
        this.atom = n;
    }

    public Term(Loc loc, float x)
    {
        this.loc = loc;
        this.tag = Tag.Float;
        this.atom = x;
    }

    public Term(Loc loc, double x)
    {
        this.loc = loc;
        this.tag = Tag.Double;
        this.atom = x;
    }

    public void add(Term a)
    {
        contents.Add(a);
    }
}
