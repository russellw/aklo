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

    class Loop
    {
        public Term continueTarget;
        public Term breakTarget;

        public Loop(Term continueTarget, Term breakTarget)
        {
            this.continueTarget = continueTarget;
            this.breakTarget = breakTarget;
        }
    }

    static void flatten(Term f)
    {
        var body = f[0];
        f.contents = new List<Term>();
        var block = new Term(f.loc, Tag.Block);

        void go(Term b)
        {
            f.add(block);
            block = b;
        }

        Term term(Loop loop, Term a)
        {
            switch (a.tag)
            {
                case Tag.Goto:
                    f.add(a);
                    go(new Term(a.loc, Tag.Block));
                    break;
                case Tag.Label:
                    {
                        var b = new Term(a.loc, Tag.Block, a.name);
                        f.add(new Term(a.loc, Tag.Goto, b));
                        go(b);
                        break;
                    }
                case Tag.Break:
                    if (loop.breakTarget == null)
                        Etc.err(a.loc, "break without loop");
                    f.add(new Term(a.loc, Tag.Goto, loop.breakTarget));
                    go(new Term(a.loc, Tag.Block, "breakAfter"));
                    break;
                case Tag.Continue:
                    if (loop.continueTarget == null)
                        Etc.err(a.loc, "continue without loop");
                    f.add(new Term(a.loc, Tag.Goto, loop.continueTarget));
                    go(new Term(a.loc, Tag.Block, "continueAfter"));
                    break;
                case Tag.Block:
                    foreach (var b in a)
                        term(loop, b);
                    break;
                case Tag.Assert:
                    a[0] = term(loop, a[0]);
                    f.add(a);
                    break;
                default:
                    throw new Exception(a.ToString());
            }
            return a;
        }

        term(new Loop(null, null), body);
        f.add(block);
    }

    public static void norm(List<Module> program)
    {
        //wrap program in function
        var module = program[0];
        var body = new Term(new Loc(module.file, 1), Tag.Block);
        body.contents = module.publicSection;
        var f = new Term(new Loc(module.file, 1), Tag.Fn);
        f.add(body);

        //convert to normal form
        resolve(new Dictionary<string, Term>(), f);
        flatten(f);
    }
}
