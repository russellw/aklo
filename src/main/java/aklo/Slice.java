package aklo;

import static org.objectweb.asm.Opcodes.INVOKESTATIC;

import java.util.Map;
import org.objectweb.asm.MethodVisitor;

final class Slice extends Ternary {
  Slice(Loc loc, Object s, Object i, Object j) {
    super(loc, s, i, j);
  }

  @Override
  void emit(Map<Object, Integer> refs, MethodVisitor mv) {
    load(refs, mv, arg0);
    load(refs, mv, arg1);
    load(refs, mv, arg2);
    mv.visitMethodInsn(
        INVOKESTATIC,
        "aklo/Etc",
        "slice",
        "(Ljava/lang/Object;Ljava/lang/Object;Ljava/lang/Object;)Ljava/util/List;",
        false);
  }

  @Override
  String type() {
    return "Ljava/util/List;";
  }
}
