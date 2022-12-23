package aklo;

import java.math.BigInteger;
import java.util.Iterator;
import java.util.function.Function;

public abstract class Term1 extends Term {
  public final Term arg;

  public Term1(Loc loc, Term arg) {
    super(loc);
    this.arg = arg;
  }

  public abstract Term remake(Loc loc, Term arg);

  @Override
  public Term map(Function<Term, Term> f) {
    var a = f.apply(arg);
    return remake(loc, a);
  }

  @Override
  public Type type() {
    return arg.type();
  }

  @Override
  public int size() {
    return 1;
  }

  @Override
  public Term get(int i) {
    assert i == 0;
    return arg;
  }

  public double apply(double a) {
    throw new UnsupportedOperationException(toString());
  }

  public float apply(float a) {
    throw new UnsupportedOperationException(toString());
  }

  public BigInteger apply(BigInteger a) {
    throw new UnsupportedOperationException(toString());
  }

  public BigRational apply(BigRational a) {
    throw new UnsupportedOperationException(toString());
  }

  @Override
  public Term eval() {
    if (arg instanceof ConstDouble) return new ConstDouble(loc, apply(arg.doubleVal()));
    if (arg instanceof ConstFloat) return new ConstFloat(loc, apply(arg.floatVal()));
    if (arg instanceof ConstRational) return new ConstRational(loc, apply(arg.rationalVal()));
    return new ConstInteger(loc, apply(arg.integerVal()));
  }

  @Override
  public final Iterator<Term> iterator() {
    return new Iterator<>() {
      private int i;

      @Override
      public boolean hasNext() {
        return i == 0;
      }

      @Override
      public Term next() {
        assert i == 0;
        i++;
        return arg;
      }
    };
  }
}
