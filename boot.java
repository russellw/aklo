import java.io.*;
import java.util.*;
import java.util.function.*;

class Main {
  public static void main(String[] args) {
    new Main1().run();
  }
}

@SuppressWarnings("unchecked")
class Etc {
  static int add(Object a, Object b) {
    return (int) a + (int) b;
  }

  static List<Object> range(Object i, Object j) {
    var i1 = (int) i;
    var j1 = (int) j;
    var r = new ArrayList<Object>();
    while (i1 < j1) r.add(i1++);
    return r;
  }

  static int mul(Object a, Object b) {
    return (int) a * (int) b;
  }

  static List<Object> str(Object a) {
    var s = a.toString();
    var r = new ArrayList<Object>();
    for (var i = 0; i < s.length(); i++) r.add((int) s.charAt(i));
    return r;
  }

  static List<Object> append(Object a, Object b) {
    var r = new ArrayList<Object>((List<Object>) a);
    r.add(b);
    return r;
  }

  static List<Object> cat(Object a, Object b) {
    var r = new ArrayList<Object>((List<Object>) a);
    r.addAll((List<Object>) b);
    return r;
  }

  static void eprint(Object a) {
    fprint(System.err, a);
  }

  static void fprint(PrintStream stream, Object a) {
    if (a instanceof List) {
      for (var c : (List<Object>) a) stream.print((char) (int) c);
      return;
    }
    throw new IllegalArgumentException(a.toString());
  }

  static int len(Object a) {
    return ((List<Object>) a).size();
  }

  static int neg(Object a) {
    return -(int) a;
  }

  static Object print(Object a) {
    fprint(System.out, a);
    return null;
  }

  static int sub(Object a, Object b) {
    return (int) a - (int) b;
  }

  static boolean lt(Object a, Object b) {
    return (int) a < (int) b;
  }

  static boolean le(Object a, Object b) {
    return (int) a <= (int) b;
  }

  static boolean eq(Object a, Object b) {
    return a.equals(b);
  }

  static Object subscript(Object a, Object i) {
    return ((List<Object>) a).get((int) i);
  }

  static boolean truth(boolean a) {
    return a;
  }

  static boolean truth(Object a) {
    if (a instanceof Integer) return (int) a != 0;
    if (a instanceof List) return ((List<Object>) a).size() != 0;
    return true;
  }

  static boolean nump(Object a) {
    return a instanceof Integer;
  }

  static boolean symp(Object a) {
    return a instanceof Sym;
  }

  static Object get(Object record, Object key) {
    for (var entry : (List<Object>) record) {
      var entry1 = (List<Object>) entry;
      if (entry1.get(0).equals(key)) return entry1.get(1);
    }
    return 0;
  }

  static boolean listp(Object a) {
    return a instanceof List;
  }
}

final class Sym {
  static final Map<String, Sym> syms = new HashMap<>();
  static final Map<String, Integer> suffixes = new HashMap<>();
  final String stem;
  String name;

  Sym() {
    stem = null;
  }

  Sym(String stem) {
    this.stem = stem;
  }

  static Sym intern(String name) {
    var a = syms.get(name);
    if (a == null) {
      a = new Sym();
      a.name = name;
      syms.put(name, a);
    }
    return a;
  }

  static Sym intern(List<Object> name) {
    var sb = new StringBuilder();
    for (var c : name) sb.append((char) (int) c);
    return intern(name.toString());
  }

  @Override
  public String toString() {
    if (name == null) {
      var i = suffixes.get(stem);
      if (i == null) {
        name = stem;
        i = 1;
      } else name = stem + i++;
      suffixes.put(stem, i);
      name = '#' + name;
    }
    return name;
  }
}