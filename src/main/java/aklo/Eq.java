package aklo;

import static org.objectweb.asm.Opcodes.*;

import org.objectweb.asm.MethodVisitor;

public final class Eq extends Term2 {
  public Eq(Loc loc, Term arg0, Term arg1) {
    super(loc, arg0, arg1);
  }

  @Override
  public void emit(MethodVisitor mv) {
    arg0.load(mv);
    arg1.load(mv);
    mv.visitMethodInsn(INVOKEVIRTUAL, "java/lang/String", "equals", "(Ljava/lang/Object;)Z", false);
  }

  @Override
  public Tag tag() {
    return Tag.EQ;
  }
}
