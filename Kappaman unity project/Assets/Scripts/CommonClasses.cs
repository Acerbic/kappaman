using UnityEngine;
using System.Collections;

// stores passability from this cell to the neighbouring ones
public class CellStruct {
  public bool top, right; // true if passage is possible, false if not -- wall.
  public bool reachable; // can this cell be reached by the player at all
  public int deadness;
  public CellStruct(bool _t, bool _r) {top = _t; right = _r; reachable = false; deadness = -1;}
}

public class LabyrinthLayout {
  // for serialization and stuff
  public static LabyrinthLayout empty = new LabyrinthLayout(0, 0);

  // read-only playable field sizes (not including utility row and field)
  public int width { 
    get {return cells.GetLength(0) -1;}}
  public int height { 
    get {return cells.GetLength(1) -1;}}

  public CellStruct[,] cells;

  /**
   * Proper handles negative modulo operands
   */
  private static int MathMod(int a, int b) {return ((a % b) + b) % b;}

  /**
   * accessor to the internal array of cells.
   * if requested indices are greater than array sizes or less than zero,
   * "wrap around" happens (modulo)
   */
  public CellStruct this[int x, int y] {
    get {
      return cells[MathMod(x, cells.GetLength(0)), MathMod(y, cells.GetLength(1))];}
    set {
      cells[MathMod(x, cells.GetLength(0)), MathMod(y, cells.GetLength(1))] = value;
    }
  }

  /**
   * Constructor. All playable cells are created closed for all directions.
   * @param int width     - playable field horizontal dimension (column count). Utility column 0 is not counted.
   * @param int height    - playable field vertical dimension (row count). Utility row 0 is not counted.
   */
  public LabyrinthLayout(int width, int height) {
    // fill array with closed cells
    cells = new CellStruct[width +1, height +1]; // create an array of cells
    for (int j=0; j <= height; j++) {
      for (int i=0; i <= width; i++) {
        // cells[i, j] = new CellStruct(i == 0, j == 0);
        cells[i, j] = new CellStruct(false, false);
      }
    }
  }

  /**
   * Constructor. Generates layout from textual representation
   * @param string[] lines  - lines with '*' marking walls
   */
  public LabyrinthLayout(string[] lines) {
    int size_x = lines[0].Length / 2;
    int size_y = lines.GetLength(0) / 2;

    cells = new CellStruct[size_x, size_y]; // create an array of cells

    // ... and fill it with records, creating objects in labyrinth 
    for (int i = 0; i < size_y; i++) {
      for (int j = 0; j < size_x; j++) {

        // lab coords to text sample labyrinth
        int trow = (size_y - i) * 2 -1;
        int tcol = j * 2;

        // passability flags
        bool top = (lines[trow -1][tcol] != '*');
        bool right = (lines[trow][tcol +1] != '*');

        cells[j, i] = new CellStruct(top, right);
      }
    }
  }


  /**
   * Flips LabyrinthLayout array by diagonal (0,0) to (n, n)
   *
   * @return new LabyrinthLayout - new object, this one is not modified 
   *   null if dimensions of original array are not equal or othe error
   */
  public LabyrinthLayout FlipTurnCellsArray() {
    int size_x = cells.GetLength(0);
    int size_y = cells.GetLength(1);

    if (size_y != size_x) { return null; }

    LabyrinthLayout t = new LabyrinthLayout(size_x -1, size_y -1);
    for (int i = 0; i < size_x; i++) {
      for (int j = 0; j < size_y; j++) {
        // turn+flip single cell
        CellStruct t_cell = new CellStruct(cells[i,j].right, cells[i,j].top);
        t.cells[j,i] = t_cell;
      }
    }

    return t;
  }

  /**
   * Flips LabyrinthLayout in horizontal direction (mirror plane is vertical)
   *
   * @return new LabyrinthLayout - new object, this one is not modified 
   */
  public LabyrinthLayout FlipHorizCellsArray() {
    int size_x = cells.GetLength(0);
    int size_y = cells.GetLength(1);

    LabyrinthLayout t = new LabyrinthLayout(size_x -1, size_y -1); // filled with closed cells

    for (int i = 1; i < size_x; i++) {
      for (int j = 0; j < size_y; j++) {

        int flip_column = size_x - i; // from 1 to size_x
        t.cells[i-1, j].right = cells[flip_column, j].right; 
        t.cells[i, j].right = cells[flip_column-1, j].right; 
        t.cells[i, j].top = cells[flip_column, j].top; 
      }
    }

    return t;
  }

  /**
   * Flips LabyrinthLayout in vertical direction (mirror plane is horizontal)
   * 
   * @return new LabyrinthLayout - new object, this one is not modified 
   */
  public LabyrinthLayout FlipVertCellsArray() {
    int size_x = cells.GetLength(0);
    int size_y = cells.GetLength(1);

    LabyrinthLayout t = new LabyrinthLayout(size_x -1, size_y -1); // filled with closed cells

    for (int i = 0; i < size_x; i++) {
      for (int j = 1; j < size_y; j++) {

        int flip_row = size_y - j; // from 1 to size_y-1
        t.cells[i, j-1].top = cells[i, flip_row].top; 
        t.cells[i, j].top = cells[i, flip_row-1].top; 
        t.cells[i, j].right = cells[i, flip_row].right; 
      }
    }

    return t;
  }
}