using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

class Term : IList<Term>
{
    public Loc loc;
    public Tag tag;
    public string name;
    public List<Term> contents = new List<Term>();
    public int intVal;
    public float floatVal;
    public double doubleVal;
    public Term type_;
    public List<Term> params_ = new List<Term>();
    public List<Term> locals = new List<Term>();
    public Term ref_;
    public int serial = -1;

    static int serials;

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

    public static void debug(Term a, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        Console.WriteLine("{0}:{1}:", file, line);
        debug1(a);
    }

    public static void debug1(Term a)
    {
        switch (a.tag)
        {
            case Tag.Fn:
                Console.WriteLine(a);
                foreach (var b in a.locals)
                {
                    Console.Write("  {0}:", b);
                    if (b.type_ != null)
                        Console.Write(" {0}", b.type_.tag);
                    Console.WriteLine();
                }
                foreach (var b in a)
                    debug1(b);
                return;
            case Tag.Block:
                Console.WriteLine("  {0}", a);
                foreach (var b in a)
                    debug1(b);
                return;
            case Tag.Assert:
            case Tag.Return:
            case Tag.Assign:
            case Tag.Debug:
                Console.Write("    {0}", a.tag);
                break;
            default:
                Console.Write("    {0} = {1}", a, a.tag);
                break;
        }
        foreach (var b in a)
            Console.Write(" {0}", b);
        Console.WriteLine();
    }

    public override string ToString()
    {
        if (name != null)
            return name;
        switch (tag)
        {
            case Tag.Int:
                return "#" + intVal;
            case Tag.Float:
                return "#" + floatVal;
            case Tag.Double:
                return "#" + doubleVal;
            case Tag.False:
            case Tag.True:
                return tag.ToString();
        }
        if (serial < 0)
            serial = serials++;
        return string.Format("{0:x}", serial);
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

    public static Term type(Term a)
    {
        switch (a.tag)
        {
            case Tag.Var:
            case Tag.Cast:
                return a.type_;
            case Tag.Ref:
                return type(a.ref_);
            case Tag.Int:
            case Tag.Float:
            case Tag.Double:
                return a;
            case Tag.BitAnd:
            case Tag.BitOr:
            case Tag.BitXor:
            case Tag.BitNot:
            case Tag.Shl:
            case Tag.Shr:
                return new Term(a.loc, Tag.Int);
            case Tag.And:
            case Tag.Or:
            case Tag.Not:
            case Tag.True:
            case Tag.False:
            case Tag.Eq:
            case Tag.Lt:
            case Tag.Le:
                return new Term(a.loc, Tag.Bool);
            case Tag.Neg:
            case Tag.Add:
            case Tag.Sub:
            case Tag.Mul:
            case Tag.Div:
            case Tag.Rem:
            case Tag.Assign:
            case Tag.OpAssign:
                return type(a[0]);
        }
        throw new Exception(a.ToString());
    }

    public static bool eq(Term a, Term b)
    {
        if (a.tag != b.tag)
            return false;
        if (a.Count != b.Count)
            return false;
        for (var i = 0; i < a.Count; i++)
            if (!eq(a[i], b[i]))
                return false;
        return true;
    }
}
