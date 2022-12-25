package aklo;

public final class ContinueBreak extends Term {
  public final boolean break1;
  public final String label;

  public ContinueBreak(Loc loc, boolean break1, String label) {
    super(loc);
    this.break1 = break1;
    this.label = label;
  }

  @Override
  public Tag tag() {
    return Tag.GOTO;
  }
}