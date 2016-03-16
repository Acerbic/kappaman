using UnityEngine;
using System.Collections;

public class CookieController : MonoBehaviour {
  public PlayerController playerController;
  public SoundEventsController soundPlayer;
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

  void Hide() {
    GetComponent<SpriteRenderer>().enabled = false;
    GetComponent<CircleCollider2D>().enabled = false;
  }

  void OnTriggerEnter2D(Collider2D other) {
    if (other.CompareTag("Player")) {
      playerController.CollectCookie();
      soundPlayer.OmNomNom();

      // Hide();
      // GameObject.Destroy(gameObject, 5);
      GameObject.Destroy(gameObject);

      GameObject g = GameObject.FindWithTag("GameController");
      GameController gc = g.GetComponent<GameController>();
      gc.Score += cookiePointsWorth;
      gc.CookiesCollected++;
    }
  }
}
