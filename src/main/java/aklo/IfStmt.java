package aklo;

import java.util.List;

public final class IfStmt extends Terms {
  public final int elseIdx;

  @Override
  public Term remake(Loc loc, Term[] terms) {
    return new IfStmt(loc, terms, elseIdx);
  }

  public IfStmt(Loc loc, List<Term> terms, int elseIdx) {
    super(loc, terms);
    this.elseIdx = elseIdx;
  }

  public IfStmt(Loc loc, Term[] terms, int elseIdx) {
    super(loc, terms);
    this.elseIdx = elseIdx;
  }

  @Override
  public Tag tag() {
    return Tag.IF;
  }
}