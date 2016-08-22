using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

  /**
   * Open all inner walls
   */
  public void OpenInnerWalls () {

    int size_x = cells.GetLength(0);
    int size_y = cells.GetLength(1);

    for (int i = 1; i < size_x -1; i++) {
      int j;
      for (j = 1; j < size_y -1; j++) {
        cells[i, j].right = true;
        cells[i, j].top = true;
      }
      cells[i, j].right = true;
    }

    for (int j = 1; j < size_y -1; j++) {
      cells[size_x -1, j].top = true;
    }
  }

  /**
   * Count a number of walls "touching" a wall joint
   * 
   * @param int  x   coordinate of a Cell bottom left to the joint we check
   * @param int  y   coordinate of a Cell bottom left to the joint we check
   *
   * @return int     number of walls connected to the joint - 
   *                   0 .. 4; 
   *                   -1 for (x,y) being out of bounds
   */
  public int CountWallsConnectedToJoint (int x, int y) {
    int size_x = cells.GetLength(0);
    int size_y = cells.GetLength(1);

    if (x < 0 || x >= size_x || y < 0 || y >= size_y) {return -1;}

    int count = 0;

    if (!cells[x, y].top) {count++;}
    if (!cells[x, y].right) {count++;}

    if ((x < (size_x -1)) && !cells[x+1, y].top) {count++;}
    if ((y < (size_y -1)) && !cells[x, y+1].right) {count++;}

    return count;
    /**
      0,2     1,2     2,2     3,2
                          [*]
      0,1     1,1     2,1     3,1
      
      0,0     1,0     2,0     3,0 size = 4
     */
  }
}

public class Snakewallhead {

  class Wall {
    public int x, y; // coords of a cell this wall belongs to 
    public bool top; // true if the wall is top wall of the cell, false - right wall
    public bool nextCell; // flag to remember if head moves to a cell neighbouring a wall cell
    public Wall (int _x, int _y, bool _t, bool _n) {x = _x; y = _y; top = _t; nextCell = _n;}
  }

  public int x, y;
  private LabyrinthLayout l;

  public Snakewallhead (int _x, int _y, LabyrinthLayout _l) {x = _x; y = _y; l = _l;}

  /**
   * Grow the wall one step
   *
   * @return bool - success of operation, true if wall grown, false if not
   */
  public bool Grow (System.Random rnd) {
    if (l.CountWallsConnectedToJoint(x,y) > 1) {
      // preventing T-juncture
      return false;
    }

    List<Wall> walls = new List<Wall>(3);

    // check all walls from this joint:

    // West
    if (l.CountWallsConnectedToJoint(x-1, y) == 0) {
      walls.Add( new Wall(x, y, true, true) );
    }

    // North
    if (l.CountWallsConnectedToJoint(x, y+1) == 0) {
      walls.Add( new Wall(x, y+1, false, false) );
    }

    // East
    if (l.CountWallsConnectedToJoint(x+1, y) == 0) {
      walls.Add( new Wall(x+1, y, true, false) );
    }

    // South
    if (l.CountWallsConnectedToJoint(x, y-1) == 0) {
      walls.Add( new Wall(x, y, false, true) );
    }

    // choose a path to grow
    if (walls.Count == 0) {return false;}
    Wall growTo = walls[rnd.Next(0, walls.Count)];

    // advance this head to a new position
    if (growTo.top) {
      // growTo.x -= (growTo.nextCell ? 1 : 0);
      l[growTo.x, growTo.y].top = false;
    } else {
      // growTo.y -= (growTo.nextCell ? 1 : 0);
      l[growTo.x, growTo.y].right = false;
    }

    x = growTo.x + (growTo.nextCell && growTo.top ? -1 : 0);
    y = growTo.y + (growTo.nextCell && !growTo.top ? -1 : 0);

    return true;
  }


  /**
   * Automaticall extend this Snakewall to maximum
   * @param System.Random rnd  - system random generator
   */
  public void AutoGrow (System.Random rnd) {

    bool had_growth = true;
    while (had_growth) {
      had_growth = Grow(rnd);
    }
  }
}