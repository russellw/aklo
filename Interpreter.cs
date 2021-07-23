using System;
using System.Collections.Generic;
using System.Text;

static class Interpreter
{
    class Env
    {
        public Env outer;
        public Dictionary<Term, object> m = new Dictionary<Term, object>();

        public Env(Env outer)
        {
            this.outer = outer;
        }
    }

    class Closure
    {
        public Env env;
        public Term f;

        public Closure(Env env, Term f)
        {
            this.env = env;
            this.f = f;
        }
    }

    static object get(Env env, Term a)
    {
        switch (a.tag)
        {
            case Tag.Int:
                return a.intVal;
            case Tag.Float:
                return a.floatVal;
            case Tag.Double:
                return a.doubleVal;
            case Tag.True:
                return true;
            case Tag.False:
                return false;
        }
        while (env != null)
        {
            if (env.m.ContainsKey(a))
            {
                return env.m[a];
            }
            env = env.outer;
        }
        throw new Exception(a.ToString());
    }

    static void set(Env env, Term a, object val)
    {
        while (env != null)
        {
            if (env.m.ContainsKey(a))
            {
                env.m[a] = val;
                return;
            }
            env = env.outer;
        }
        throw new Exception(a.ToString());
    }

    static object eval(Env env, Term a)
    {
        switch (a.tag)
        {
            case Tag.Cast:
                {
                    dynamic x = get(env, a[0]);
                    switch (a.type_.tag)
                    {
                        case Tag.Int:
                            if (x is bool)
                                return x ? 1 : 0;
                            return (int)x;
                        case Tag.Float:
                            if (x is bool)
                                return x ? 1.0f : 0.0f;
                            return (float)x;
                        case Tag.Double:
                            if (x is bool)
                                return x ? 1.0 : 0.0;
                            return (double)x;
                        case Tag.Bool:
                            return x != 0;
                    }
                    throw new Exception(a.type_.ToString());
                }
            case Tag.Neg:
                {
                    dynamic x = get(env, a[0]);
                    return -x;
                }
            case Tag.BitNot:
                {
                    var x = (int)get(env, a[0]);
                    return ~x;
                }
            case Tag.Eq:
                {
                    dynamic x = get(env, a[0]);
                    dynamic y = get(env, a[1]);
                    return x == y;
                }
            case Tag.BitAnd:
                {
                    var x = (int)get(env, a[0]);
                    var y = (int)get(env, a[1]);
                    return x & y;
                }
            case Tag.BitOr:
                {
                    var x = (int)get(env, a[0]);
                    var y = (int)get(env, a[1]);
                    return x | y;
                }
            case Tag.BitXor:
                {
                    var x = (int)get(env, a[0]);
                    var y = (int)get(env, a[1]);
                    return x ^ y;
                }
            case Tag.Shl:
                {
                    var x = (int)get(env, a[0]);
                    var y = (int)get(env, a[1]);
                    return x << y;
                }
            case Tag.Shr:
                {
                    var x = (int)get(env, a[0]);
                    var y = (int)get(env, a[1]);
                    return x >> y;
                }
            case Tag.Lt:
                {
                    dynamic x = get(env, a[0]);
                    dynamic y = get(env, a[1]);
                    return x < y;
                }
            case Tag.Le:
                {
                    dynamic x = get(env, a[0]);
                    dynamic y = get(env, a[1]);
                    return x <= y;
                }
            case Tag.Add:
                {
                    dynamic x = get(env, a[0]);
                    dynamic y = get(env, a[1]);
                    return x + y;
                }
            case Tag.Sub:
                {
                    dynamic x = get(env, a[0]);
                    dynamic y = get(env, a[1]);
                    return x - y;
                }
            case Tag.Mul:
                {
                    dynamic x = get(env, a[0]);
                    dynamic y = get(env, a[1]);
                    return x * y;
                }
            case Tag.Div:
                {
                    dynamic x = get(env, a[0]);
                    dynamic y = get(env, a[1]);
                    return x / y;
                }
            case Tag.Rem:
                {
                    dynamic x = get(env, a[0]);
                    dynamic y = get(env, a[1]);
                    return x % y;
                }
        }
        throw new Exception(a.ToString());
    }

    static object apply(Closure closure, Term[] args)
    {
        var env = new Env(closure.env);
        var f = closure.f;

        //local variables
        foreach (var a in f.locals)
            env.m[a] = null;

        //code
        var block = f[0];
        var ip = 0;
        for (; ; )
        {
            var a = block[ip++];
            switch (a.tag)
            {
                case Tag.Goto:
                    block = a[0];
                    ip = 0;
                    break;
                case Tag.Return:
                    return get(env, a[0]);
                case Tag.Assign:
                    {
                        var x = a[0];
                        var y = get(env, a[1]);
                        set(env, x, y);
                        env.m[a] = y;
                        break;
                    }
                case Tag.Assert:
                    {
                        var x = (bool)get(env, a[0]);
                        if (!x)
                            Etc.err(a.loc, "assert failed");
                        break;
                    }
                case Tag.Debug:
                    {
                        var x = get(env, a[0]);
                        Console.WriteLine("{0}:{1}: {2}", a.loc.file, a.loc.line, x);
                        break;
                    }
                case Tag.If:
                    {
                        var cond = (bool)get(env, a[0]);
                        block = a[cond ? 1 : 2];
                        ip = 0;
                        break;
                    }
                default:
                    env.m[a] = eval(env, a);
                    break;
            }
        }
    }

    public static void run(Term f)
    {
        Term.debug(f);
        var closure = new Closure(null, f);
        var args = new Term[0];
        apply(closure, args);
    }
}
