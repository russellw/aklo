package aklo;

import java.util.ArrayList;
import java.util.List;

public final class Block {
  public final Loc loc;
  public final List<Term> insns = new ArrayList<>();

  public Block(Loc loc) {
    this.loc = loc;
  }
}
