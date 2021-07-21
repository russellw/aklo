using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

class Term : IList<Term>
{
    public readonly Loc loc;
    public readonly Tag tag;
    public string name;
    public List<Term> contents = new List<Term>();
    public int intVal;
    public float floatVal;
    public double doubleVal;
    public Term type;
    public List<Term> params_ = new List<Term>();
    public List<Term> locals = new List<Term>();
    public Term ref_;

    public int Count => contents.Count;

    public bool IsReadOnly => throw new NotImplementedException();

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

    public void Add(Term a)
    {
        contents.Add(a);
    }

    public Term this[int i]
    {
        get
        {
            return contents[i];
        }
        set
        {
            contents[i] = value;
        }
    }

    static void str(int indent, Term a, StringBuilder sb)
    {
        for (var i = 0; i < indent; i++)
            sb.Append(' ');
        sb.Append(a.tag);
        sb.Append(' ');
        switch (a.tag)
        {
            case Tag.Int:
                sb.Append(a.intVal);
                break;
            case Tag.Float:
                sb.Append(a.floatVal);
                break;
            case Tag.Double:
                sb.Append(a.doubleVal);
                break;
        }
        if (a.name != null)
            sb.Append(a.name);
        sb.Append('\n');
        foreach (var b in a)
            str(indent + 1, b, sb);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('\n');
        str(0, this, sb);
        return sb.ToString();
    }

    public IEnumerator<Term> GetEnumerator()
    {
        return ((IEnumerable<Term>)contents).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public int IndexOf(Term item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, Term item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        contents.Clear();
    }

    public bool Contains(Term item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(Term[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(Term item)
    {
        throw new NotImplementedException();
    }
}
