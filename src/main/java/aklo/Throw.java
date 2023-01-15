package aklo;

import static org.objectweb.asm.Opcodes.*;

import java.util.Map;
import org.objectweb.asm.MethodVisitor;

final class Throw extends Unary {
  @Override
  boolean isTerminator() {
    return true;
  }

  Throw(Loc loc, Term arg) {
    super(loc, arg);
  }

  @Override
  void emit(Map<Object, Integer> refs, MethodVisitor mv) {
    mv.visitTypeInsn(NEW, "java/lang/RuntimeException");
    mv.visitInsn(DUP);
    Term.load(refs, mv, arg);
    mv.visitMethodInsn(
        INVOKESTATIC, "aklo/Etc", "decode", "(Ljava/lang/Object;)Ljava/lang/String;", false);
    mv.visitMethodInsn(
        INVOKESPECIAL, "java/lang/RuntimeException", "<init>", "(Ljava/lang/String;)V", false);
    mv.visitInsn(ATHROW);
  }

  @Override
  Tag tag() {
    return Tag.THROW;
  }
}
