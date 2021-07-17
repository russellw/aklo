using System;
using System.Collections.Generic;
using System.Text;

class Term
{
    public Loc loc;
    public readonly Tag tag;
    public List<Term> contents;

    public Term(Tag tag)
    {
        this.tag = tag;
    }
}
