using System;
using System.IO;

class Program
{
    static string file;
    static string outFile;

    static string optArg(string[] args, ref int i, string arg)
    {
        if (arg != null)
            return arg;
        if (i + 1 == args.Length)
        {
            Console.Error.WriteLine("{0}: expected arg", args[i]);
            Environment.Exit(1);
        }
        return args[++i];
    }

    static void parseArgs(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var s = args[i];

            //file
            if (!s.StartsWith("-"))
            {
                if (file != null)
                {
                    Console.Error.WriteLine("{0}: expected one file", args[i]);
                    Environment.Exit(1);
                }
                file = s;
                continue;
            }

            //option
            while (s.StartsWith("-"))
                s = s.Substring(1);

            //option argument
            string arg = null;
            for (var j = 0; j < s.Length; j++)
                switch (s[j])
                {
                    case ':':
                    case '=':
                        arg = s.Substring(j + 1);
                        s = s.Substring(0, j);
                        goto done;
                }
            done:

            //option
            switch (s)
            {
                case "V":
                case "version":
                    Console.WriteLine("Aklo compiler version 0");
                    Environment.Exit(0);
                    break;
                case "o":
                case "out":
                case "output":
                    outFile = optArg(args, ref i, arg);
                    break;
                case "h":
                case "?":
                case "help":
                    Console.WriteLine("General options:");
                    Console.WriteLine("-h       Show help");
                    Console.WriteLine("-V       Show version");
                    Console.WriteLine();
                    Console.WriteLine("Output options:");
                    Console.WriteLine("-o file  Specify output file");
                    Environment.Exit(0);
                    break;
                default:
                    Console.Error.WriteLine("{0}: unknown option", args[i]);
                    Environment.Exit(1);
                    break;
            }
        }
    }

    static void Main(string[] args)
    {
        parseArgs(args);
        if (file == null)
        {
            Console.Error.WriteLine("Usage: aklo [options] files");
            Environment.Exit(1);
        }
        var text = File.ReadAllText(file);
        var program = Parser.parse(file, text);
        Norm.norm(program);
    }
}
