package aklo;

public final class Location {
  public final String file;
  public final int line;

  public Location(String file, int line) {
    this.file = file;
    this.line = line;
  }
}