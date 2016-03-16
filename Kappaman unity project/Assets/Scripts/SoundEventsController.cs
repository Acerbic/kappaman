using UnityEngine;
using System.Collections;

public class SoundEventsController : MonoBehaviour {

  public AudioClip[] deadSounds;
  public AudioClip[] killSounds;
  public float[] killSoundsVolumes;
  public AudioClip crunchySound;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  public
  void RandomDeathSound () {
    int dead_index = (new System.Random()).Next(0, deadSounds.GetLength(0));
    GetComponent<AudioSource>().PlayOneShot(deadSounds[dead_index], 1.1f);
  }

  public
  void RandomKillSound () {
    int kill_index = (new System.Random()).Next(0, killSounds.GetLength(0));
    GetComponent<AudioSource>().PlayOneShot(killSounds[kill_index], killSoundsVolumes[kill_index]);
    // GetComponent<AudioSource>().PlayOneShot(killSounds[kill_index], 2.3f);
  }

  public
  void OmNomNom () {
    GetComponent<AudioSource>().PlayOneShot(crunchySound, 0.3f);
  }
}
