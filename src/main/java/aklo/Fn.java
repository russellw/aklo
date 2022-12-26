package aklo;

import java.util.*;
import java.util.function.Consumer;

public class Fn extends Term {
  public final String name;
  public final List<Var> params = new ArrayList<>();
  public Type rtype;
  public final List<Var> vars = new ArrayList<>();
  public final List<Term> body = new ArrayList<>();
  public final List<Block> blocks = new ArrayList<>();

  public Fn(Loc loc, String name) {
    super(loc);
    this.name = name;
  }

  public final void walkFns(Consumer<Fn> f) {
    f.accept(this);
    for (var a : body)
      a.walk(
          b -> {
            if (b instanceof Fn b1) b1.walkFns(f);
          });
  }

  @Override
  public Tag tag() {
    return Tag.FN;
  }

  // convert to basic blocks
  private static final class Env {
    final Env outer;
    final Map<String, Term> locals = new HashMap<>();

    Env(Env outer) {
      this.outer = outer;
    }

    Term get(String name) {
      for (var env = this; env != null; env = env.outer) {
        var a = env.locals.get(name);
        if (a != null) return a;
      }
      return null;
    }
  }

  private record Loop(Fn.Loop outer, String label, Block continueTarget, Block breakTarget) {
    // static because loop could be null
    static Loop get(Loop loop, String label) {
      if (label == null) return loop;
      for (; loop != null; loop = loop.outer) if (label.equals(loop.label)) break;
      return loop;
    }
  }

  private Var var1(Loc loc) {
    var x = new Var(loc);
    vars.add(x);
    return x;
  }

  private void addBlock(Block block) {
    blocks.add(block);
  }

  private Block lastBlock() {
    return blocks.get(blocks.size() - 1);
  }

  private void insn(Term a) {
    lastBlock().insns.add(a);
  }

  private void assign(Term a, Term b, Block fail) {
    switch (a.tag()) {
      case INTERN -> {}
    }
  }

  private Term term(Loop loop, Term a) {
    switch (a.tag()) {
      case POST_INC -> {
        var a1 = (PostInc) a;
        var r = var1(a.loc);

        insn(new Assign(a.loc, r, term(loop, a.get(0))));

        return r;
      }
      case OR -> {
        var r = var1(a.loc);
        var falseBlock = new Block(a.loc);
        var afterBlock = new Block(a.loc);

        // condition
        insn(new Assign(a.loc, r, term(loop, a.get(0))));
        insn(new If(a.loc, r, afterBlock, falseBlock));

        // false
        addBlock(falseBlock);
        insn(new Assign(a.loc, r, term(loop, a.get(1))));
        insn(new Goto(a.loc, afterBlock));

        // after
        addBlock(afterBlock);
        return r;
      }
      case AND -> {
        var r = var1(a.loc);
        var trueBlock = new Block(a.loc);
        var afterBlock = new Block(a.loc);

        // condition
        insn(new Assign(a.loc, r, term(loop, a.get(0))));
        insn(new If(a.loc, r, trueBlock, afterBlock));

        // true
        addBlock(trueBlock);
        insn(new Assign(a.loc, r, term(loop, a.get(1))));
        insn(new Goto(a.loc, afterBlock));

        // after
        addBlock(afterBlock);
        return r;
      }
      case NOT -> {
        var r = var1(a.loc);
        var trueBlock = new Block(a.loc);
        var falseBlock = new Block(a.loc);
        var afterBlock = new Block(a.loc);

        // condition
        insn(new If(a.loc, term(loop, a.get(0)), trueBlock, falseBlock));

        // true
        addBlock(trueBlock);
        insn(new Assign(a.loc, r, new False(a.loc)));
        insn(new Goto(a.loc, afterBlock));

        // false
        addBlock(falseBlock);
        insn(new Assign(a.loc, r, new True(a.loc)));
        insn(new Goto(a.loc, afterBlock));

        // after
        addBlock(afterBlock);
        return r;
      }
      case IF -> {
        var a1 = (IfStmt) a;
        var trueBlock = new Block(a.loc);
        var falseBlock = new Block(a.loc);
        var afterBlock = new Block(a.loc);

        // condition
        insn(new If(a.loc, term(loop, a1.get(0)), trueBlock, falseBlock));

        // true
        addBlock(trueBlock);
        for (var i = 1; i < a1.elseIdx; i++) term(loop, a1.get(i));
        insn(new Goto(a.loc, afterBlock));

        // false
        addBlock(falseBlock);
        for (var i = a1.elseIdx; i < a1.size(); i++) term(loop, a1.get(i));
        insn(new Goto(a.loc, afterBlock));

        // after
        addBlock(afterBlock);
      }
      case WHILE -> {
        var a1 = (While) a;
        var bodyBlock = new Block(a.loc);
        var condBlock = new Block(a.loc);
        var afterBlock = new Block(a.loc);
        loop = new Loop(loop, a1.label, condBlock, afterBlock);

        // before
        insn(new Goto(a.loc, a1.doWhile ? bodyBlock : condBlock));

        // body
        addBlock(bodyBlock);
        for (var i = 1; i < a1.size(); i++) term(loop, a1.get(i));
        insn(new Goto(a.loc, condBlock));

        // condition
        addBlock(condBlock);
        insn(new If(a.loc, term(loop, a1.get(0)), bodyBlock, afterBlock));

        // after
        addBlock(afterBlock);
      }
      case GOTO -> {
        var a1 = (LoopGoto) a;
        var label = a1.label;
        loop = Loop.get(loop, label);
        if (loop == null) {
          if (label == null) {
            var s = a1.break1 ? "break" : "continue";
            throw new CompileError(a.loc, s + " without loop");
          }
          throw new CompileError(a.loc, label + " not found");
        }
        insn(new Goto(a.loc, a1.break1 ? loop.breakTarget : loop.continueTarget));
        addBlock(new Block(a.loc));
      }
      case RETURN -> {
        a.set(0, term(loop, a.get(0)));
        insn(a);
        addBlock(new Block(a.loc));
      }
      default -> {
        if (a.isEmpty()) return a;
        for (var i = 0; i < a.size(); i++) a.set(i, term(loop, a.get(i)));
        insn(a);
      }
    }
    return a;
  }

  private void toBlocks(Env outer) {
    // environment of local variables and functions
    var env = new Env(outer);
    for (var x : vars) env.locals.put(x.name, x);
    for (var a : body)
      a.walk(
          b -> {
            if (b instanceof Fn b1) env.locals.put(b1.name, b);
          });

    // convert nested functions to basic blocks
    for (var a : body)
      a.walk(
          b -> {
            if (b instanceof Fn b1) b1.toBlocks(env);
          });

    // convert this function to basic blocks
    addBlock(new Block(loc));
    for (var a : body) term(null, a);

    // default return 0
    if (!lastBlock().hasTerminator()) {
      var loc1 = body.isEmpty() ? loc : body.get(body.size() - 1).loc;
      insn(new Return(loc1, new ConstInteger(loc1, 0)));
    }
  }

  public void toBlocks() {
    toBlocks(null);
  }
}
