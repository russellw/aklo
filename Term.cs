using System;
using System.Collections.Generic;
using System.Text;

class Term
{
    public readonly Loc loc;
    public readonly Tag tag;
    public readonly List<Term> contents = new List<Term>();

    public Term(Loc loc, Tag tag)
    {
        this.loc = loc;
        this.tag = tag;
    }
}
