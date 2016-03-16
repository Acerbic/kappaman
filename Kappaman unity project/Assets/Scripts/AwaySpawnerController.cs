using UnityEngine;
using System.Collections;

/**
 * Spawns monster at distance from player
 */
public class AwaySpawnerController : MonoBehaviour {

  public MonsterController monster_template;
  public LabyrinthController labyrinth;
  public PlayerController player;

  public MonsterController[] monsters = null;
  public int monstersNumber = 1;
  public int minimalDistanceToPlayer = 4;

	// Use this for initialization
	void Start () {
	  monsters = new MonsterController[monstersNumber];
	}
	
  // 
  MonsterController SpawnMonster () {

    // 1. Create a monster
    GameObject monsters = GameObject.Find("Labyrinth/Monsters");
    GameObject monster = Instantiate(monster_template.gameObject, new Vector3(0, 0, 0), Quaternion.identity) 
      as GameObject;

    monster.SetActive(true);
    monster.transform.parent = monsters.transform;

    // 2. Position the monster
    Vector3 pos = Vector3.zero;
    int tries = 10000;

    while (pos == Vector3.zero && tries > 0) {

      int i = labyrinth.rnd.Next(1, labyrinth.width);
      int j = labyrinth.rnd.Next(1, labyrinth.height);

      // check reachable tile
      if (labyrinth.layout[i, j].reachable &&

          (System.Math.Abs(i - player.transform.position.x) + 
           System.Math.Abs(j - player.transform.position.y)
           > minimalDistanceToPlayer)
         ) {

        monster.transform.position = new Vector3(i, j, 0);
        return monster.GetComponent<MonsterController>();
      }

      tries--;
    }

    throw new System.Exception("FFS can't spawn a monster");
  }

	// Update is called once per frame
	void Update () {
    for (int i = 0; i < monstersNumber; i ++) {
      if (monsters[i] == null) {
        monsters[i] = SpawnMonster();
        // monstersNumber = 0;
      }
    }
	}
}
