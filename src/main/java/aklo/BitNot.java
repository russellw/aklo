package aklo;

import static org.objectweb.asm.Opcodes.INVOKESTATIC;

import java.math.BigInteger;
import java.util.Map;
import org.objectweb.asm.MethodVisitor;

final class BitNot extends Unary {
  BitNot(Loc loc, Term arg) {
    super(loc, arg);
  }

  @Override
  void emit(Map<Object, Integer> refs, MethodVisitor mv) {
    Term.load(refs, mv, arg);
    mv.visitMethodInsn(
        INVOKESTATIC, "aklo/Etc", "bitNot", "(Ljava/lang/Object;)Ljava/lang/Object;", false);
  }

  @Override
  BigInteger apply(BigInteger a) {
    return a.not();
  }

  @Override
  Tag tag() {
    return Tag.BIT_NOT;
  }
}
