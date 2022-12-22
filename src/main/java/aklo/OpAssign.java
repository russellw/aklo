package aklo;

public final class OpAssign extends Term2 {
  public final Tag op;

  public OpAssign(Loc loc, Tag op, Term arg0, Term arg1) {
    super(loc, arg0, arg1);
    this.op = op;
  }

  @Override
  public Tag tag() {
    return Tag.OP_ASSIGN;
  }
}