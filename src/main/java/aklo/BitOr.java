package aklo;

import static org.objectweb.asm.Opcodes.INVOKESTATIC;

import java.math.BigInteger;
import java.util.Map;

import org.objectweb.asm.MethodVisitor;

final class BitOr extends Binary {
  BitOr(Loc loc, Term arg0, Term arg1) {
    super(loc, arg0, arg1);
  }

  @Override
  void emit(Map<Object, Integer> refs, MethodVisitor mv) {
    arg0.load(, mv);
    arg1.load(, mv);
    mv.visitMethodInsn(
        INVOKESTATIC,
        "aklo/Etc",
        "bitOr",
        "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;",
        false);
  }

  @Override
  Object apply(BigInteger a, BigInteger b) {
    return a.or(b);
  }

  @Override
  Tag tag() {
    return Tag.BIT_OR;
  }
}
