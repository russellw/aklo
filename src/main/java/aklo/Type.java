package aklo;

import static org.objectweb.asm.Opcodes.*;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import org.objectweb.asm.ClassWriter;

final class Type extends Named {
  // system types
  static final Type INT = new Type("I");

  // the classes of the program being compiled, excluding system types
  static final Type mainClass = new Type("a");
  static final List<Type> classes = new ArrayList<>(List.of(mainClass));
  static Fn mainFn;

  final List<Var> vars = new ArrayList<>();
  final List<Fn> fns = new ArrayList<>();

  @Override
  public String toString() {
    if (name.length() == 1) return name;
    return 'L' + name + ';';
  }

  Type(String name) {
    super(name);
  }

  private static void lift(Fn f) {
    for (var g : f.fns) lift(g);
    mainClass.fns.add(f);
  }

  static void init(Collection<Fn> modules) {
    mainFn = new Fn("main");
    var args = new Var("args", mainFn.params);
    args.type = "[Ljava/lang/String;";
    mainFn.rtype = "V";
    for (var module : modules) {
      // lift functions to global scope
      lift(module);

      // Module scope variables are static
      mainClass.vars.addAll(module.vars);
      module.vars.clear();

      // modules may contain initialization code
      // so each module is called as a function from main
      mainFn.lastBlock().instructions.add(new Call(module));
    }
    mainFn.lastBlock().instructions.add(new ReturnVoid());
    mainClass.fns.add(mainFn);
  }

  static void writeClasses() throws IOException {
    Named.unique(classes);
    for (var c : classes) c.write();
  }

  private void write() throws IOException {
    var w = new ClassWriter(ClassWriter.COMPUTE_FRAMES);
    w.visit(V17, ACC_PUBLIC, name, null, "java/lang/Object", new String[0]);

    // static variables
    Named.unique(vars);
    for (var x : vars) w.visitField(ACC_STATIC, x.name, x.type, null, null).visitEnd();

    // functions
    Named.unique(fns);
    for (var f : fns) f.write(w);

    // write class file
    w.visitEnd();
    Files.write(Path.of(name + ".class"), w.toByteArray());
  }
}
