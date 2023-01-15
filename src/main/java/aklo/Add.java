package aklo;

import static org.objectweb.asm.Opcodes.INVOKESTATIC;

import java.math.BigInteger;
import org.objectweb.asm.MethodVisitor;

final class Add extends Binary {
  Add(Loc loc, Term arg0, Term arg1) {
    super(loc, arg0, arg1);
  }

  @Override
  void emit(MethodVisitor mv) {
    arg0.load(mv);
    arg1.load(mv);
    mv.visitMethodInsn(
        INVOKESTATIC,
        "aklo/Etc",
        "add",
        "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;",
        false);
  }

  @Override
  Object apply(double a, double b) {
    return a + b;
  }

  @Override
  Object apply(float a, float b) {
    return a + b;
  }

  @Override
  Object apply(BigInteger a, BigInteger b) {
    return a.add(b);
  }

  @Override
  Object apply(BigRational a, BigRational b) {
    return a.add(b);
  }

  @Override
  Tag tag() {
    return Tag.ADD;
  }
}
