using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

/**
 * Controls Sound Effects volume and other tweaks
 */
public class SoundEffectsController : MonoBehaviour {

  public AudioMixer mixer;
  public float[] effectsVolumeLevels;
  public Sprite[] effectsVolumeIcons;
  private int effectsVolumeIndex = 0;
  private Button effectsVolumeControlButton;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  /**
   * Broadcasted game status event - see GameController
   */
  public
  void GameNewLevel () {
    effectsVolumeControlButton = GameObject.Find("Effects Volume button").GetComponent<Button>();;
    effectsVolumeControlButton.image.overrideSprite = effectsVolumeIcons[effectsVolumeIndex];
    effectsVolumeControlButton.onClick.AddListener(OnEffectsVolumeButton);
  }

  /**
   * Broadcasted game status event - see GameController
   */
  public
  void GameEnd () {
    // Disable huff-puff of monsters.
    GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
    foreach (GameObject g in monsters) {
      AudioSource a_s = g.GetComponent<AudioSource>();
      if (a_s) {
        a_s.Stop();
      }
    }    
  }

  /**
   * OnClick event for "Effects Volume button" UI
   */
  public
  void OnEffectsVolumeButton () {
    effectsVolumeIndex = (effectsVolumeIndex + 1) % effectsVolumeLevels.Length;
    mixer.SetFloat("Effects volume", effectsVolumeLevels[effectsVolumeIndex]);

    effectsVolumeControlButton.image.overrideSprite = effectsVolumeIcons[effectsVolumeIndex];
  }
}
