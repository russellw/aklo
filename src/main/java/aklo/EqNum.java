package aklo;

import static org.objectweb.asm.Opcodes.INVOKESTATIC;

import java.math.BigInteger;
import java.util.Map;
import org.objectweb.asm.MethodVisitor;

final class EqNum extends Binary {
  EqNum(Loc loc, Term arg0, Term arg1) {
    super(loc, arg0, arg1);
  }

  @Override
  Tag tag() {
    return Tag.EQ_NUM;
  }

  @Override
  void emit(Map<Object, Integer> refs, MethodVisitor mv) {
    Term.load(refs, mv, arg0);
    Term.load(refs, mv, arg1);
    mv.visitMethodInsn(
        INVOKESTATIC,
        "aklo/Etc",
        "eqNum",
        "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;",
        false);
  }

  @Override
  Object apply(double a, double b) {
    return a == b;
  }

  @Override
  Object apply(float a, float b) {
    return a == b;
  }

  @Override
  Object apply(BigInteger a, BigInteger b) {
    return a.equals(b);
  }

  @Override
  Object apply(BigRational a, BigRational b) {
    return a.equals(b);
  }
}
