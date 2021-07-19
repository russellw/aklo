using System;
using System.Collections.Generic;
using System.Text;

static class Norm
{
    static void resolve(Dictionary<string, Term> m, Term a)
    {
        switch (a.tag)
        {
            case Tag.Block:
                m = new Dictionary<string, Term>(m);
                foreach (var b in a)
                    if (b.tag == Tag.Fn)
                        m.Add(b.name, b);
                break;
            case Tag.Ref:
                if (!m.TryGetValue(a.name, out a.ref_))
                    Etc.err(a.loc, string.Format("'{0}': not found", a.name));
                break;
            case Tag.Var:
                //a fine point: it is actually valid to say x := x
                //the new name of x only applies on the left-hand side
                //so we need to recur before adding the new name
                //instead of the usual order
                resolve(m, a[0]);
                m.Add(a.name, a);
                return;
            case Tag.Fn:
            case Tag.For:
                m = new Dictionary<string, Term>(m);
                foreach (var x in a.params_)
                    resolve(m, x);
                break;
        }
        foreach (var b in a)
            resolve(m, b);
    }

    public static void norm(List<Term> program)
    {
        var m = new Dictionary<string, Term>();
        foreach (var a in program)
            resolve(m, a);
    }
}
