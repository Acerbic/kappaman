using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class LabyrinthController : MonoBehaviour {

  public int width; // width of the labyrinth - columns count not including unplayable col 0
  public int height; // height of the labyrinth - rows count not including unplayable row 0

  // 2D array of cell records
  // rows are counted from bottom to top -- 0 is the bottom row(not playable!),  1 is the lowest on screen
  //  first coord is horizontal (x axis), second is vertical (y axis), aligned to the transform coordinate system
  public LabyrinthLayout layout;

  // player spawning coordinates for this labyrinth. Should be (1 .. width-1, 1 .. height-1)
  public int playerSpawnX = 1;
  public int playerSpawnY = 1;
  public int randomSeed = 0;
 
  public int cookieCount = 0;
  public int pillCount = 0;

  public int cookieToPillRatio = 10;

  public System.Random rnd;

  private LabyrinthLayout[] sectionSize6;
  private LabyrinthLayout[] sectionSize5;
  private LabyrinthLayout[] sectionSize4;
  private LabyrinthLayout[] sectionSize3;

  private LabyrinthGraph lg;

  public SpriteRenderer[] hWallsCollection;
  public float[] hWallsChances;

  public SpriteRenderer[] floorsCollection;
  public float[] floorsChances;

  public SpriteRenderer ceiling;
  public AwaySpawnerController spawner;

  void Awake () {
    // calling it here, because monster array allocation
    // happens in Start() call of the spawner
  	SetDifficulty();
  }

	// Use this for initialization
	void Start () {

    // BAD SEED: diff = 3, seed = 673964776

		if (randomSeed == 0) {
			randomSeed = (new System.Random()).Next();
		}
		rnd = new System.Random(randomSeed);

		// GameController gc = GameController.GetSingletonInstance();

		// GenerateLabyrinthLoops(20, 20);
		// GenerateLabyrinthRandom(20, 20);

		// GenerateLabyrinthWorms(width, height);
		GenerateLabyrinthCrosses(width, height);

		FillLabyrinthBlanks();
		CloseBorders();

    MapLabyrinthPathings();
		InstantiateLabyrinthObjects();
	}

	// Update is called once per frame
	void Update () {
	}

  void SetDifficulty () {
    GameController gc = GameController.GetSingletonInstance();
    if (gc) {
      spawner.monstersNumber = Mathf.CeilToInt((float)gc.difficultyLevel / 2);
      width = 5 + gc.difficultyLevel * 5;
      height = 5 + gc.difficultyLevel * 3;
      cookieToPillRatio = 10 + 2 * gc.difficultyLevel;
    }
  }


	/**
	 * Check if passage is possible in labyrinth. Target must be ajacend to position (4 directions).
	 *
	 * @param int position_x		- cell coordinate (1 .. width) of a cell from which to move
	 * @param int position_y		- cell coordinate (1 .. height) of a cell from which to move
	 * @param int target_x  		- cell coordinate (1 .. width) of a targe cell
	 * @param int target_y  		- cell coordinate (1 .. height) of a targe cell
	 */
	public bool CanMoveFromCell(int position_x, int position_y, int target_x, int target_y) {
		try {
			bool checkTop = false;

			checkTop = (position_x == target_x);

			if (checkTop) {
				return target_y < position_y ?
					layout.cells[target_x, target_y].top :
					layout.cells[target_x, position_y].top;

			} else {
				return target_x > position_x ?
					layout.cells[position_x, position_y].right :
					layout.cells[target_x, position_y].right;
			}
		} catch (System.IndexOutOfRangeException e) {
				Debug.Log("IOOR exception: pos = (" + position_x + ", " + position_y + "); target = (" + target_x + ", " + target_y + ")");
			throw e;
		}
	}

	// /**
	//  * Carve a single loop in cells array
	//  */
	// private LabyrinthLayout MakeALoopInLabyrinth(LabyrinthLayout l, int left, int bottom, int right, int top) {
	// 	for (int i = left; i < right; i++) {
	// 		l.cells[i, bottom].right = true;
	// 		l.cells[i, top].right = true;
	// 	}

	// 	for (int i = bottom; i < top; i++) {
	// 		l.cells[left, i].top = true;
	// 		l.cells[right, i].top = true;
	// 	}

	// 	return l;
	// }

	// public void GenerateLabyrinthLoops(int width, int height) {
	// 	// number of loops
	// 	int top_dimention = System.Math.Max(width, height);

	// 	int loops = rnd.Next(top_dimention / 2, top_dimention * 3 / 4);

	// 	Debug.Log ("Making " + loops + " loops!");

	// 	// player position
	// 	playerSpawnX = rnd.Next(1, width);
	// 	playerSpawnY = rnd.Next(1, height);
	// 	Debug.Log ("Spawning at (" + playerSpawnX + ", " + playerSpawnY + ")");

	// 	layout = new LabyrinthLayout(width, height);

	// 	while (loops > 0) {
	// 		int left, right, bottom, top;

	// 		left = rnd.Next(1, width -1);
	// 		right = rnd.Next(left +1, width);
	// 		bottom = rnd.Next(1, height -1);
	// 		top = rnd.Next(bottom +1, height);

	// 		Debug.Log ("looping (" + left + ", " + right + ", " + bottom + ", " + top + ", " + ")");

	// 		MakeALoopInLabyrinth(layout, left, bottom, right, top);
	// 		loops--;
	// 	}
	// }

	// /**
	//  * Run single worm through the maze, carving its way
	//  *
	//  * @param int x      			starting position x
	//  * @param int y      			starting position y
	//  * @param int length 			how long worm path to make
	//  * @param System.Random r predefined random generator for levels consistency
	//  */
	// private void WormCarve(int x, int y, int length) {
	// 	int direction = rnd.Next(0, 4);

	// 	while (length > 0) {
	// 		// choose if moving or turning?
	// 		if (rnd.Next(0, 4) <= 0) {
	// 			// turning
	// 			direction += rnd.Next(0, 2) * 2 - 1; // direction += -1 or 1
	// 			direction = direction % 4;
	// 		}

	// 		// moving to direction
	// 		switch (direction) {
	// 			case 0:
	// 				layout.cells[x, y].right = true;
	// 				x += 1;
	// 				if (x > width) {
	// 					x = 1;
	// 					layout.cells[0, y].right = true;
	// 				}
	// 				break;

	// 			case 1:
	// 				layout.cells[x, y].top = true;
	// 				y += 1;
	// 				if (y > height) {
	// 					y = 1;
	// 					layout.cells[x, 0].top = true;
	// 				}
	// 				break;

	// 			case 2:
	// 				x -= 1;
	// 				layout.cells[x, y].right = true;
	// 				if (x <= 0) {
	// 					x = width-1;
	// 					layout.cells[x, y].right = true;
	// 				}
	// 				break;

	// 			case 3:
	// 				y -= 1;
	// 				layout.cells[x, y].top = true;
	// 				if (y <= 0) {
	// 					y = height-1;
	// 					layout.cells[x, y].top = true;
	// 				}
	// 				break;
	// 		}

	// 		length--;
	// 	}
	// }

	// /**
	//  * Generate labyrinth with given dimensions.
	//  * Actual playable area will be 1 less row and 1 less column due
	//  * to col 0 and row 0 reserved for utility
	//  */
	// public void GenerateLabyrinthWorms(int width, int height) {

	// 	int top_dimention = System.Math.Max(width, height);
	// 	int cells_count = (width-1) * (height-1);

	// 	int worms_count = rnd.Next(cells_count / top_dimention / 3, cells_count / top_dimention / 2);

	// 	// Debug.Log("Sending " + worms_count + " worms");

	// 	layout = new LabyrinthLayout(width, height); 
	// 	// player position
	// 	playerSpawnX = rnd.Next(1, width);
	// 	playerSpawnY = rnd.Next(1, height);

	// 	while (worms_count > 0) {

	// 		WormCarve(playerSpawnX, playerSpawnY, top_dimention * 3);

	// 		worms_count--;
	// 	}
	// }

	private void Crucify (int x, int y) {
		for (int i=1; i < width; i++) {
			layout[i, y].right = true;
		}
		for (int i=1; i < height; i++) {
			layout[x, i].top = true;
		}
	}

	/**
	 * Labyrinth generation with crosses.
	 */
	public void GenerateLabyrinthCrosses (int width, int height) {

		layout = new LabyrinthLayout(width, height); 

    // player position
    if (width >= 7) {
      playerSpawnX = rnd.Next(4, width-2);
    } else {
      playerSpawnX = rnd.Next(1, width+1);
    }
    if (height >= 7) {
      playerSpawnY = rnd.Next(4, height-2);
    } else {
      playerSpawnY = rnd.Next(1, height+1);
    }

		Crucify(playerSpawnX, playerSpawnY);

		// int cross_count = top_dimention / 7 -1;

		// while (cross_count > 0) {
		// 	Crucify(rnd.Next(1, width), rnd.Next(1, height));
		// 	cross_count--;
		// }
	}


  private int GetNodeConnectionsCount(GraphNode n) {
    int connections_count = 0;
    n.GetConnections((GraphNode node) => {connections_count++;});
    return connections_count;
  }

  private int CalculateDeadEndDepth(GraphNode n) {
    int depth = -1;

    GraphNode last = null;
    int last_connections_count = 1;

    int current_connections_count;
    GraphNode next_node;

    while (n != null && last_connections_count <= 2) {
      depth++;

      // count connections from this
      current_connections_count = 0;
      next_node = null;

      n.GetConnections((GraphNode node) => {
        current_connections_count++;
        if (node != last) {
          next_node = node;
        }
      });

      last = n;
      n = next_node;
      last_connections_count = current_connections_count;
    }
    return depth;
  }


	/**
	 * Create a AStar node graph and pathing trough generated labyrinth.
   * Detect dead ends. 
	 */
	private void MapLabyrinthPathings () {
    lg = AstarPath.active.astarData.AddGraph(typeof(LabyrinthGraph)) as LabyrinthGraph;
    lg.layout = layout;
    AstarPath.active.Scan();

		Vector3 starting_position = new Vector3(playerSpawnX, playerSpawnY, 0);
		GraphNode start_node = (GraphNode)lg.GetNearest(starting_position);
		List<GraphNode> reachable = PathUtilities.GetReachableNodes(start_node);

		foreach (GraphNode n in reachable) {
			Vector3 position = (Vector3) n.position;

			// FIXME: position (x, y) --> [i, j] make adjustment for scale and translation of LabyrinthGraph
			int i = Mathf.RoundToInt(position.x);
			int j = Mathf.RoundToInt(position.y);

			layout[i, j].reachable = true;

      // detect dead ends
      if (GetNodeConnectionsCount(n) == 1) {
        layout[i, j].deadness = CalculateDeadEndDepth(n);
      }
		}
	}


	/**
	 * Pick a number based on weightened probability.
	 * @param float[] - array of weights to correspoinding elements.
	 *                  If you want to choose one element out of N, you send
	 *                  an array of length N.
	 *                  
	 * @returns int - index of an element selected
	 */
	private	int ChooseRandomWeighted (float[] probs) {

	  float total = 0;

	  foreach (float elem in probs) {
	      total += elem;
	  }

	  float randomPoint = Random.value * total;

	  for (int i= 0; i < probs.Length; i++) {
	      if (randomPoint < probs[i]) {
	          return i;
	      }
	      else {
	          randomPoint -= probs[i];
	      }
	  }
	  return probs.Length - 1;
	}

	/**
	 * Create a GameObject for a wall
	 * @param bool    horizontal    	- is wall horizontal (several variants random)
	 * @param Vector3 position      	- where to place the wall
	 * @param int     sorting_order 	- index of SortingOrder for the wall (proper overlapping)
	 */
	private GameObject InstantiateRandomWall(bool horizontal, Vector3 position, int sorting_order) {
		GameObject walls = GameObject.Find("Labyrinth/Walls");
		GameObject o;
		GameObject template;

		if (horizontal) {
			int index = ChooseRandomWeighted(hWallsChances);

			// fix for transparent hole in "crumbled wall" texture showing empty space when
			// in the lowest row. FIXME, position = local coordinates, not game grid indices
			if (position.y == 0) {
				index = 0;
			}

			SpriteRenderer sr = hWallsCollection[index];
			template = sr.gameObject;

		} else {
			template = GameObject.Find("Labyrinth/Walls Collection/wallv");
		}

		o = Instantiate(template, position, Quaternion.identity) as GameObject;
		o.GetComponent<SpriteRenderer>().sortingOrder = sorting_order;
		o.SetActive(true);
		o.transform.parent = walls.transform;
		return o;
	}

  /**
   * [InstantiateLabyrinthObjects description]
   */
	public void InstantiateLabyrinthObjects() {
		GameObject cookie_template = GameObject.Find("Labyrinth/Items Collection/cookie");
		GameObject pill_template = GameObject.Find("Labyrinth/Items Collection/pride pill");

		GameObject o; // temp object for instantiation
		GameObject tiles = GameObject.Find("Labyrinth/Floor Tiles");
		GameObject items = GameObject.Find("Labyrinth/Items");

		int walls_layer_index = 0;

		// Add Labyrinth walls
		for (int i = height; i >= 0 ; i--) {

			// create top walls (horizontal)
			for (int j = 0; j <= width; j++) {
				if (!layout.cells[j, i].top && j > 0) {
					InstantiateRandomWall(true, new Vector3(j, i, 0), walls_layer_index);
					walls_layer_index++;
				}
			}

			// create right walls (vertical)
			for (int j = 0; j <= width; j++) {
				if (!layout.cells[j, i].right & i > 0) {
					InstantiateRandomWall(false, new Vector3(j + 0.5f, i + 0.5f, 0), walls_layer_index);
					walls_layer_index++;
				}
			}
		}

		// Add Labyrinth children: tiles, items, ...
		for (int i = 0; i <= height; i++) {
			for (int j = 0; j <= width; j++) {

				// create tiles except for zero column and row
				if (i > 0 && j > 0) {
					SpriteRenderer sr;
					if (layout[j, i].reachable) {
						int index = ChooseRandomWeighted(floorsChances);
						sr = floorsCollection[index];
					} else {
						sr = ceiling;
					}
					o = Instantiate(sr.gameObject, new Vector3(j, i, 0), Quaternion.identity) as GameObject;
					o.SetActive(true);
					o.transform.parent = tiles.transform;
				}

				// put items - cookies and pills
				if (layout[j, i].reachable && !(j == playerSpawnX && i == playerSpawnY)) {
					Vector3 position = new Vector3(j, i, 0);
					if (layout[j, i].deadness >= 4 || rnd.Next(1, cookieToPillRatio) == 1) {
						// drop a pill
						o = Instantiate(pill_template, position, Quaternion.identity) as GameObject;
						pillCount++;

					} else {
						// put a cookie
						o = Instantiate(cookie_template, position, Quaternion.identity) as GameObject;
						cookieCount++;
					}
					o.SetActive(true);
					o.transform.parent = items.transform;
				}
			}
		}
	}

	/**
	 * Returns a number of presets we have in a library for
	 * any given size. 
	 * XXX: right now we have only 1 of each sizes from 3 to 6
	 * 
	 * @param int size - size of a square preset (playable space)
	 */
	private int GetLabPresetsCount(int size) {
		if (size >= 3 && size <= 6) { return 8; }
		return 0;
	}

	/**
	 * Pick a preset from the library of presets by size and index
	 *  
	 * @param int size  - 3 to 6
	 * @param int index - 0 to GetLabPresetsCount(size)-1
	 */
	private LabyrinthLayout ReadLabPreset(int size, int index) {
		switch (size) {
			case 3: return sectionSize3[index];
			case 4: return sectionSize4[index];
			case 5: return sectionSize5[index];
			case 6: return sectionSize6[index];
		}

		Debug.Log("ReadLabPreset error on " + size + " " + index);
		return null;
	}

	/**
	 * Find a preset section of appropriate size and flip it randomly
	 * @param int size - length of a side of a square block of tiles
	 *    we are looking to fill with a preset.
	 *
	 * @return LabyrinthLayout - selected preset that will be less or 
	 *   equal to requested size
	 */
	public LabyrinthLayout GenerateSection(int size) {
		int actual_size = size;
		// check presets number by sizes.
		int count_presets = 0;
		while ((actual_size > 0) && (count_presets == 0)) {
			count_presets = GetLabPresetsCount(actual_size);
			actual_size--;
		}
		actual_size++;

		if (count_presets == 0) {
			Debug.Log("No presets for size " + size);
			return null;
		}
 
 		int ind = rnd.Next(0, count_presets);
		// get random preset of given size
		LabyrinthLayout preset = ReadLabPreset(actual_size, ind);
		if (preset == null) {
			Debug.Log("GenerateSection failed for size " + size + " and ind = " + ind);
		}

		return preset;
	}

	/**
	 * Adds a labyrinth section to the labyrinth on specified coords
	 * @param CellStruct[,] array   section to be glued in
	 * @param int left          		corner left coord of where to glue section in
	 * @param int bottom        		corner bottom coord of where to glue section in
	 */
	public void GlueInSection(LabyrinthLayout l, int left, int bottom) {
		int size_x = l.cells.GetLength(0);
		int size_y = l.cells.GetLength(1);

		for (int i=0; i<size_x; i++) {
			for (int j=0; j<size_y; j++) {
				int target_x = left + i -1;
				int target_y = bottom + j -1;

				if (target_x > width || target_y > height)
					continue;

				if (j > 0) {
					layout.cells[target_x, target_y].right = l.cells[i, j].right;
				}

				if (i > 0) {
					layout.cells[target_x, target_y].top = l.cells[i, j].top;
				}
			}
		}
	}

	/**
	 * check right-top sides of rectangle with coordinats and sizes provided
	 * to see if all the walls (external and between cells) are closed
	 */
	private bool CheckBorderClosed(int left, int bottom, int size_x, int size_y) {
		// check horizontal walls
		for (int i = bottom; i <= bottom + size_y; i++) {
			if (layout.cells[left + size_x -1, i -1].top) return false;
		}

		// check vertical walls
		for (int i = left; i <= left + size_x; i++) {
			if (layout.cells[i -1, bottom + size_y -1].right) return false;
		}

		return true;
	}

	/**
	 * check right-top sides of rectangle with coordinats and sizes provided
	 * to see if all the walls (external and between cells) are open
	 */
	private bool CheckBorderOpen(int left, int bottom, int size_x, int size_y) {
		// check horizontal walls
		for (int i = bottom; i < bottom + size_y; i++) {
			if (!layout.cells[left + size_x -1, i].top) return false;
		}

		// check vertical walls
		for (int i = left; i < left + size_x; i++) {
			if (!layout.cells[i, bottom + size_y -1].right) return false;
		}

		return true;
	}

	/**
	 * From a single layout generate 8 by flipping and rotating it
	 * @param LabyrinthLayout preset - original layout
	 *
	 * @return LabyrinthLayout[] - array of 8 layouts, including the original in [0]
	 */
	private LabyrinthLayout[] GeneratePresetFlips(LabyrinthLayout preset) {
		LabyrinthLayout[] result = new LabyrinthLayout[8];

		result[0] = preset;
		result[1] = preset.FlipHorizCellsArray();
		result[2] = preset.FlipVertCellsArray();
		result[3] = result[2].FlipHorizCellsArray();

		preset = preset.FlipTurnCellsArray();

		result[4] = preset;
		result[5] = preset.FlipHorizCellsArray();
		result[6] = preset.FlipVertCellsArray();
		result[7] = result[6].FlipHorizCellsArray();

		return result;
	}

	/**
	 * Prepare library of presets with sizes 3 to 6
	 */
	private void GenerateSectionPresets() {
		string []s3 = {
			" *** ***" ,
			". .*.*. " ,
			" * * ***" ,
			".*.*. .*" ,
			" *** * *" ,
			".*. .*. " ,
			" * *****" ,
			". . . . " 
		};

		string []s4 = {
			".*******.*" ,
			".......*.*" ,
			".*****.*.*" ,
			".*...*...." ,
			".*.*.*.*.*" ,
			".*.*.*.*.*" ,
			".*.*.*.*.*" ,
			".*.*...*.*" ,
			".*.*.***.*" ,
			".........." 
		};

		string []s5 = {
			".*.*****.*.*" ,
			".........*.*" ,
			".*******.*.*" ,
			".*.....*.*.*" ,
			".*.***.*.*.*" ,
			".*...*.*.*.." ,
			".***.*.*.*.*" ,
			".*.*.*...*.*" ,
			".*.*.*****.*" ,
			".*.*.......*" ,
			".*.*.*****.*" ,
			"............" 
		};

		string []s6 = {
			".***.***.*****" ,
			".......*.....*" ,
			".*****.*****.*" ,
			".*.......*...*" ,
			".*.*****.*.***" ,
			"...*.....*.*.." ,
			".***.*.***.*.*" ,
			".....*.......*" ,
			".***.*.***.***" ,
			".*.*.*...*...." ,
			".*.*.*.*.*****" ,
			".*...*.*.....*" ,
			".*****.*****.*" ,
			".............." 
		};

		sectionSize3 = GeneratePresetFlips(new LabyrinthLayout(s3));
		sectionSize4 = GeneratePresetFlips(new LabyrinthLayout(s4));
		sectionSize5 = GeneratePresetFlips(new LabyrinthLayout(s5));
		sectionSize6 = GeneratePresetFlips(new LabyrinthLayout(s6));
	}

	/**
	 * Check all blank square spaces in labyrinth (no walls at all or all walls)
	 * and fill them with patterns from pattern library
	 */
	private void FillLabyrinthBlanks() {
		GenerateSectionPresets();
		// int sections_count = 0;

		// find a blank
		for (int left = 1; left <= width-2; left++) {
			for (int bottom = 1; bottom <= height-2; bottom++) {
				// looking for a blank with left-bottom corner (left, bottom)

				bool empty_space = layout.cells[left-1, bottom].right && layout.cells[left, bottom-1].top;
				bool closed_space = !(layout.cells[left-1, bottom].right || layout.cells[left, bottom-1].top);
				int max_size_square = System.Math.Min(width - left +1, height - bottom +1);

				int size = 1;
				while ((empty_space || closed_space) && size <= max_size_square) {
					empty_space = empty_space && CheckBorderOpen(left, bottom, size, size);
					closed_space = closed_space && CheckBorderClosed(left, bottom, size, size);

					if (! (empty_space || closed_space)) break;
					size++;
				}
				size--;

				if (size >= 3) {
					// fill blank

					// Debug.Log("Found blank size " + size + " at (" + left + ", " + bottom + ")");
					LabyrinthLayout section = GenerateSection(size);
					GlueInSection(section, left, bottom);

					// sections_count++;
				}
			}
		}

		// Debug.Log("sections to add max = " + sections_count);
		// XXX: destroy Section Presets?
	}

	/**
	 * Edit the labyrinth so all external passages are closed
	 * (no wrapping around)
	 */
	private void CloseBorders() {
		for (int i = 1; i <= width; i++) {
			layout.cells[i, 0].top = false;
			layout.cells[i, height].top = false;
		}

		for (int j = 1; j <= height; j++) {
			layout.cells[0, j].right = false;
			layout.cells[width, j].right = false;
		}
	}
}