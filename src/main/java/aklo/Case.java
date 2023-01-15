package aklo;

import java.util.List;

final class Case extends Nary {
  Case(List<Term> terms) {
    super(terms.get(0).loc, terms);
  }

  @Override
  Tag tag() {
    return Tag.CASE;
  }
}
