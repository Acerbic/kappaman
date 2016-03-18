using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class MonsterController : MonoBehaviour {

  public GameController gameController;

  // reference to the player object
  public PlayerController playerController;
  // Game Mechanic : movement speed in units / sec
  public float moveSpeed = 2;

  // how close to the waypoint must monster be to switch to the next one
  private const float WAYPOINT_PROXIMITY_REACTION = 0.05f;
  // path is being calculated right now
  private bool thinking;
  // waypoint on the current path we are moving towards
  private int pathWayPointInd;
  // current path (might be null to indicate full path was executed)
  private Vector3[] pathToPlayer;

  // Game Mechanic : how many points give for eating the monster
  public int monsterPointsWorth = 20;

	// Use this for initialization
	void Start () {
    if (playerController == null) {
      playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }
    gameController = GameController.GetSingletonInstance();

    thinking = false;
    pathWayPointInd = 0;
    pathToPlayer = null;
  }
	
  /**
   * Called to move pathWayPointInd pointer across the path
   * to skip points (a point) that was reached by this gameObject on this
   * tick calculation.
   *
   * Side effects: pathWayPointInd - may increase, pathToPlayer - may turn null
   * 
   * @param float travel_distance   how far can this gameObject move this tick
   *
   * @return float
   *   how much movements distance must object go after reaching and skipping all
   *   waypoints.
   */
  float SkipReachedWaypoint (float travel_distance = WAYPOINT_PROXIMITY_REACTION) {
    if (pathToPlayer == null) {
      return 0;
    }

    Vector3 new_position = transform.position;

    while (pathWayPointInd < pathToPlayer.Length) {
      float distance_to_waypoint = Vector3.Distance(pathToPlayer[pathWayPointInd], new_position);
      if (distance_to_waypoint <= travel_distance) {
        new_position = pathToPlayer[pathWayPointInd];
        pathWayPointInd++;
        travel_distance -= distance_to_waypoint;
      } else {
        break;
      }
    }

    if (pathWayPointInd >= pathToPlayer.Length) {
      pathToPlayer = null;
    }

    transform.position = new_position;
    return travel_distance;
  }

  void FixedUpdate() {

    int old_waypoint_index = pathWayPointInd;
    float travel_rest = SkipReachedWaypoint(Time.fixedDeltaTime * moveSpeed);
    bool switched_waypoints = pathWayPointInd != old_waypoint_index;
    if (pathToPlayer != null) {
      Vector3 direction = (pathToPlayer[pathWayPointInd] - transform.position).normalized;
      transform.position += direction * travel_rest;
    }

    // reached end of path or one of waypoints and no alternative pathing calculation in progress
    if ((pathToPlayer == null || switched_waypoints) && !thinking) {
      //Get a reference to the Seeker component we added earlier
      Seeker seeker = GetComponent<Seeker>();

      Vector3 path_target = playerController.transform.position;
      Vector3 path_from = transform.position;

      // if we are still on path, start re-pathing from the next waypoint
      if (pathToPlayer != null) {
        path_from = pathToPlayer[pathWayPointInd];
      }

      //Start a new path to the path_target, return the result to the OnPathComplete function
      ABPath p = ABPath.Construct(path_from, path_target, null);

      // since wrap around map edge possible, normal heuristics are not working
      p.heuristic = Heuristic.None;

      thinking = true; // do not repath until this pathing calculation finished
      seeker.StartPath(p, OnPathComplete);
    }
  }

	// Update is called once per frame
	void Update () {
	}

  /**
   * Check if a given point is close to a segment, defined
   * by other two points
   * @param Vector3 point - coordinates of a point to check
   * @param Vector3 a     - coordinates of a start of a segment
   * @param Vector3 b     - coordinates of an end of a segment
   *
   * @return bool - true if the point is close enough to the segment
   */
  private bool IsPointOnSegment(Vector3 point, Vector3 a, Vector3 b) {
    Vector3 a_b = b - a; // vector of segment
    Vector3 a_p = point - a; // vector from start to point
    Vector3 a_ps = Vector3.Project(a_p, a_b);
    Vector3 p_ps = a_ps - a_p; // vector from point to the line the segment is part of

    return a_ps.magnitude < a_b.magnitude && p_ps.magnitude < (a_b.magnitude / 3) && Vector3.Dot(a_p, a_b) >= 0;
  }

  /**
   * Callback from Seeker component that will be invoked after
   * path calculation is finished
   * @param Path p - resulting path
   *
   * Side effects: thinking - set to false, pathed - depending on p,
   * pathToPlayer - null or new path array, pathWayPointInd - index of new
   * point on path monster should move
   */
  public void OnPathComplete (Path p) {
    thinking = false;

    if (p.error) {
      Debug.Log ("We have error in path!");
      Debug.Log (p.errorLog);

    } else {
      pathToPlayer = p.vectorPath.ToArray();
      pathWayPointInd = 0;

      // 1 skip first waypoint if it is current position
      SkipReachedWaypoint();
      if (pathToPlayer == null) {

        // TODO handle situation where monster is locked in a closet
        // and repathing on every frame
        return;
      }

      // Debug.Log("Initial adjust skipped to " + pathWayPointInd);

      // 2 check if we already on path between waypoints
      for (int i = pathWayPointInd +1; i < pathToPlayer.Length; i++) {
        // Debug.Log("Check on path from " + (i-1) + " to " + i);

        if (IsPointOnSegment(transform.position, pathToPlayer[i-1], pathToPlayer[i])) {
          pathWayPointInd = i;
          return;
        }
      }
    }
  }

  /**
   * Unity Physics callback on collision
   * @param Collider2D other
   */
  void OnTriggerEnter2D(Collider2D other) {
    if (other.CompareTag("Player")) {
      playerController.HitByMonster(this);
    }
  }

  /**
   * Call this to kill this monster and earn points
   */
  public
  void DieKappa() {
    gameController.EatMonster(monsterPointsWorth);

    GameObject.Destroy(gameObject);
  }
}
