using UnityEngine;
using System.Collections;

public class PillController : MonoBehaviour {

	public PlayerController playerController;
	public float rotationSpeed = 1;

	// Use this for initialization
	void Start () {
    if (playerController == null) {
      playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    GetComponent<Rigidbody2D>().AddTorque(rotationSpeed, ForceMode2D.Impulse);
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag("Player")) {
			playerController.PrideParade(true);
			GameObject.Destroy(gameObject);
		}
	}
}
