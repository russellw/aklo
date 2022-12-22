package aklo;

public final class Goto extends Term {
  public final String name;

  public Goto(Loc loc, String name) {
    super(loc);
    this.name = name;
  }

  @Override
  public Tag tag() {
    return Tag.GOTO;
  }
}