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
        }
        throw new Exception(a.ToString());
    }

    public static void run(List<Term> program)
    {
        var env = new Env(null);
        foreach (var a in program)
            eval(env, a);
    }
}
