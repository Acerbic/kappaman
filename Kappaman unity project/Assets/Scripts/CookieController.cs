using UnityEngine;
using System.Collections;

public class CookieController : MonoBehaviour {
  public PlayerController playerController;
  public int cookiePointsWorth = 1;

  // Use this for initialization
  void Start () {
    if (playerController == null) {
      playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }
  }

	// Update is called once per frame
	void Update () {
	
	}

  void OnTriggerEnter2D(Collider2D other) {
    if (other.CompareTag("Player")) {
      // inform the player
      playerController.CollectCookie();

      // inform the game controller
      GameObject g = GameObject.FindWithTag("GameController");
      GameController gc = g.GetComponent<GameController>();
      gc.EatCookie(cookiePointsWorth);

      // destroy this object
      GameObject.Destroy(gameObject);
    }
  }
}
