using System;
using System.Collections.Generic;
using System.Text;

static class Norm
{
    //resolve identifiers (other than labels)
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
                m[a.name] = a;
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

    //infer types of expressions and local variables
    static List<Tag> numTypes = new List<Tag> { Tag.Bool, Tag.Int, Tag.Float, Tag.Double };

    static int rank(Term a)
    {
        var t = Term.type(a);
        return numTypes.IndexOf(t.tag);
    }

    static Term common(Term t, Term u)
    {
        var ti = rank(t);
        if (ti < 0)
            return null;
        var ui = rank(u);
        if (ui < 0)
            return null;
        return new Term(t.loc, numTypes[Math.Max(ti, ui)]);
    }

    static Term cast(Term a, Term t)
    {
        if (Term.eq(Term.type(a), t))
            return a;
        a = new Term(a.loc, Tag.Cast, a);
        a.type_ = t;
        return a;
    }

    static void infer(Term a)
    {
        foreach (var b in a)
            infer(b);
        switch (a.tag)
        {
            case Tag.Var:
                if (a.type_ == null)
                    a.type_ = Term.type(a[0]);
                a[0] = cast(a[0], a.type_);
                break;
            case Tag.Eq:
                {
                    var t = common(a[0], a[1]);
                    if (t == null)
                        break;
                    for (var i = 0; i < a.Count; i++)
                        a[i] = cast(a[i], t);
                    break;
                }
            case Tag.Lt:
            case Tag.Le:
            case Tag.Add:
            case Tag.Sub:
            case Tag.Mul:
            case Tag.Div:
            case Tag.Rem:
                {
                    var t = common(a[0], a[1]);
                    if (t == null)
                        Etc.err(a.loc, "expected numbers");
                    if (t.tag == Tag.Bool)
                        t = new Term(t.loc, Tag.Int);
                    for (var i = 0; i < a.Count; i++)
                        a[i] = cast(a[i], t);
                    break;
                }
            case Tag.BitAnd:
            case Tag.BitXor:
            case Tag.BitOr:
            case Tag.BitNot:
            case Tag.Shl:
            case Tag.Shr:
                for (var i = 0; i < a.Count; i++)
                    a[i] = cast(a[i], new Term(a.loc, Tag.Int));
                break;
            case Tag.And:
            case Tag.Assert:
            case Tag.Or:
            case Tag.Not:
                for (var i = 0; i < a.Count; i++)
                    a[i] = cast(a[i], new Term(a.loc, Tag.Bool));
                break;
        }
    }

    //flatten functions to lists of basic blocks
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
        //local variables
        Term local(Loc loc, string name)
        {
            var a = new Term(loc, Tag.Var, name);
            f.locals.Add(a);
            return a;
        }

        //blocks
        var body = f[0];
        f.Clear();

        var block = new Term(f.loc, Tag.Block);

        void go(Term b)
        {
            f.Add(block);
            block = b;
        }

        //flatten terms
        Term lval(Loop loop, Term a)
        {
            switch (a.tag)
            {
                case Tag.Ref:
                    return term(loop, a);
            }
            Etc.err(a.loc, "expected lvalue");
            return null;
        }

        Term term(Loop loop, Term a)
        {
            switch (a.tag)
            {
                case Tag.Assign:
                    {
                        var x = lval(loop, a[0]);
                        var y = term(loop, a[1]);
                        block.Add(new Term(a.loc, Tag.Assign, x, y));
                        return y;
                    }
                case Tag.OpAssign:
                    {
                        a = a[0];
                        var x = lval(loop, a[0]);
                        var y = term(loop, a[1]);
                        y = new Term(a.loc, a.tag, x, y);
                        block.Add(y);
                        block.Add(new Term(a.loc, Tag.Assign, x, y));
                        return y;
                    }
                case Tag.Ref:
                    return a.ref_;
                case Tag.Var:
                    if (a.Count > 0)
                    {
                        var x = term(loop, a[0]);
                        a.Clear();
                        block.Add(new Term(a.loc, Tag.Assign, a, x));
                    }
                    f.locals.Add(a);
                    break;
                case Tag.Goto:
                    block.Add(a);
                    go(new Term(a.loc, Tag.Block));
                    break;
                case Tag.Label:
                    {
                        var b = new Term(a.loc, Tag.Block, a.name);
                        block.Add(new Term(a.loc, Tag.Goto, b));
                        go(b);
                        break;
                    }
                case Tag.Break:
                    if (loop.breakTarget == null)
                        Etc.err(a.loc, "break without loop");
                    block.Add(new Term(a.loc, Tag.Goto, loop.breakTarget));
                    go(new Term(a.loc, Tag.Block, "breakAfter"));
                    break;
                case Tag.Continue:
                    if (loop.continueTarget == null)
                        Etc.err(a.loc, "continue without loop");
                    block.Add(new Term(a.loc, Tag.Goto, loop.continueTarget));
                    go(new Term(a.loc, Tag.Block, "continueAfter"));
                    break;
                case Tag.Block:
                    foreach (var b in a)
                        term(loop, b);
                    break;
                case Tag.Assert:
                    a[0] = term(loop, a[0]);
                    block.Add(a);
                    break;
                case Tag.Or:
                    {
                        var falseBlock = new Term(a.loc, Tag.Block, "orFalse");
                        var afterBlock = new Term(a.loc, Tag.Block, "orAfter");
                        var r = local(a.loc, "or");

                        //condition
                        var x = term(loop, a[0]);
                        block.Add(new Term(a.loc, Tag.Assign, r, x));
                        block.Add(new Term(a.loc, Tag.If, x, afterBlock, falseBlock));

                        //false
                        go(falseBlock);
                        var y = term(loop, a[1]);
                        block.Add(new Term(a.loc, Tag.Assign, r, y));
                        block.Add(new Term(a.loc, Tag.Goto, afterBlock));

                        //after
                        go(afterBlock);
                        return r;
                    }
                case Tag.And:
                    {
                        var trueBlock = new Term(a.loc, Tag.Block, "andTrue");
                        var afterBlock = new Term(a.loc, Tag.Block, "andAfter");
                        var r = local(a.loc, "and");

                        //condition
                        var x = term(loop, a[0]);
                        block.Add(new Term(a.loc, Tag.Assign, r, x));
                        block.Add(new Term(a.loc, Tag.If, x, trueBlock, afterBlock));

                        //true
                        go(trueBlock);
                        var y = term(loop, a[1]);
                        block.Add(new Term(a.loc, Tag.Assign, r, y));
                        block.Add(new Term(a.loc, Tag.Goto, afterBlock));

                        //after
                        go(afterBlock);
                        return r;
                    }
                case Tag.Not:
                    {
                        var trueBlock = new Term(a.loc, Tag.Block, "notTrue");
                        var falseBlock = new Term(a.loc, Tag.Block, "notFalse");
                        var afterBlock = new Term(a.loc, Tag.Block, "notAfter");
                        var r = local(a.loc, "not");

                        //condition
                        var x = term(loop, a[0]);
                        block.Add(new Term(a.loc, Tag.If, x, trueBlock, falseBlock));

                        //true
                        go(trueBlock);
                        block.Add(new Term(a.loc, Tag.Assign, r, new Term(a.loc, Tag.False)));
                        block.Add(new Term(a.loc, Tag.Goto, afterBlock));

                        //false
                        go(falseBlock);
                        block.Add(new Term(a.loc, Tag.Assign, r, new Term(a.loc, Tag.True)));
                        block.Add(new Term(a.loc, Tag.Goto, afterBlock));

                        //after
                        go(afterBlock);
                        return r;
                    }
                case Tag.True:
                case Tag.False:
                case Tag.Int:
                case Tag.Float:
                case Tag.Double:
                    break;
                case Tag.Cast:
                case Tag.Eq:
                case Tag.Le:
                case Tag.Lt:
                case Tag.Add:
                case Tag.Sub:
                case Tag.Mul:
                case Tag.Div:
                case Tag.Rem:
                case Tag.Neg:
                case Tag.BitAnd:
                case Tag.BitOr:
                case Tag.BitXor:
                case Tag.BitNot:
                case Tag.Shl:
                case Tag.Shr:
                    for (var i = 0; i < a.Count; i++)
                        a[i] = term(loop, a[i]);
                    block.Add(a);
                    break;
                default:
                    throw new Exception(a.ToString());
            }
            return a;
        }

        term(new Loop(null, null), body);
        block.Add(new Term(f.loc, Tag.Return, new Term(f.loc, Tag.False)));
        f.Add(block);
    }

    //top level
    public static Term norm(List<Module> program)
    {
        //wrap program in function
        var module = program[0];
        var body = new Term(new Loc(module.file, 1), Tag.Block);
        body.contents = module.publicSection;
        var f = new Term(new Loc(module.file, 1), Tag.Fn);
        f.Add(body);

        //convert to normal form
        resolve(new Dictionary<string, Term>(), f);
        infer(f);
        flatten(f);
        return f;
    }
}
