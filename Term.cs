using System;
using System.Collections.Generic;
using System.Text;

class Term
{
    public readonly Loc loc;
    public readonly Tag tag;
    public readonly string name;
    public readonly List<Term> contents = new List<Term>();
    public int intVal;
    public float floatVal;
    public double doubleVal;
    public Term type;
    public List<Term> params1;

    public Term(Loc loc, Tag tag, params Term[] s)
    {
        this.loc = loc;
        this.tag = tag;
        foreach (var a in s)
            contents.Add(a);
    }

    public Term(Loc loc, Tag tag, string name)
    {
        this.loc = loc;
        this.tag = tag;
        this.name = name;
    }

    public Term(Loc loc, int intVal)
    {
        this.loc = loc;
        this.tag = Tag.Int;
        this.intVal = intVal;
    }

    public Term(Loc loc, float floatVal)
    {
        this.loc = loc;
        this.tag = Tag.Float;
        this.floatVal = floatVal;
    }

    public Term(Loc loc, double doubleVal)
    {
        this.loc = loc;
        this.tag = Tag.Double;
        this.doubleVal = doubleVal;
    }

    public void add(Term a)
    {
        contents.Add(a);
    }
}
