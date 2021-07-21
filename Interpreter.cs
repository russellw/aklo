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

    static object get(Env env, Term key)
    {
        while (env != null)
        {
            if (env.m.ContainsKey(key))
            {
                return env.m[key];
            }
            env = env.outer;
        }
        throw new Exception(key.ToString());
    }

    static void set(Env env, Term key, object val)
    {
        while (env != null)
        {
            if (env.m.ContainsKey(key))
            {
                env.m[key] = val;
                return;
            }
            env = env.outer;
        }
        throw new Exception(key.ToString());
    }

    static object eval(Env env, Term a)
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
            case Tag.Eq:
                {
                    dynamic x = eval(env, a[0]);
                    dynamic y = eval(env, a[1]);
                    return x == y;
                }
        }
        throw new Exception(a.ToString());
    }

    static object apply(Closure closure, Term[] args)
    {
        var env = new Env(closure.env);
        var f = closure.f;
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
                    return eval(env, a[0]);
                case Tag.Assert:
                    {
                        var x = (bool)eval(env, a[0]);
                        if (!x)
                            Etc.err(a.loc, "asset failed");
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
