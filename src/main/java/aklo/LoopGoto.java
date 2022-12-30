package aklo;

public final class LoopGoto extends Term {
  // break or continue
  public final boolean break1;
  public final String label;

  public LoopGoto(Loc loc, boolean break1, String label) {
    super(loc);
    this.break1 = break1;
    this.label = label;
  }

  @Override
  public Tag tag() {
    return Tag.GOTO;
  }
}