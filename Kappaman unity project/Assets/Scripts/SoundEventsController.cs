using UnityEngine;
using System.Collections;

public class SoundEventsController : MonoBehaviour {

  public AudioClip[] deadSounds;
  public AudioClip[] killSounds;
  public float[] killSoundsVolumes;
  public AudioClip crunchySound;

  private AudioSource audioSource;

	// Use this for initialization
	void Start () {
    audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  /**
   * Broadcasted game status event - see GameController
   */
  public
  void GameEnd () {
    int dead_index = (new System.Random()).Next(0, deadSounds.GetLength(0));
    audioSource.PlayOneShot(deadSounds[dead_index], 1.1f);
  }

  /**
   * Broadcasted game status event - see GameController
   */
  public
  void GameEatMonster () {
    int kill_index = (new System.Random()).Next(0, killSounds.GetLength(0));
    audioSource.PlayOneShot(killSounds[kill_index], killSoundsVolumes[kill_index]);
  }

  public
  void GameEatCookie () {
    audioSource.PlayOneShot(crunchySound, 0.3f);
  }
}
