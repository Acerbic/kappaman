using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

/**
 * Drives music being played for the game.
 * Respective to different situations in game.
 */
public class MusicBoxController : MonoBehaviour {
  public AudioSource defaultMusic;
  public AudioSource prideMusic;
  public AudioSource victoryMusic;

  public AudioMixer mixer;

  public float[] musicVolumeLevels;
  public Sprite[] musicVolumeIcons;
  private int musicVolumeIndex = 0;
  private Button musicVolumeControlButton;

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
  void GameVictory () {
    victoryMusic.Play();
    defaultMusic.Stop();
    prideMusic.Stop();
  }

  /**
   * Broadcasted game status event - see GameController
   */
  public
  void GameNewLevel () {
    victoryMusic.Stop();
    prideMusic.Stop();
    defaultMusic.Play();

    musicVolumeControlButton = GameObject.Find("Music Volume button").GetComponent<Button>();;
    musicVolumeControlButton.image.overrideSprite = musicVolumeIcons[musicVolumeIndex];
    musicVolumeControlButton.onClick.AddListener(OnMusicVolumeButton);
  }

  /**
   * Broadcasted game status event - see GameController
   */
  public
  void GameKappaPride () {
    defaultMusic.Pause();
    if (!prideMusic.isPlaying) {
      prideMusic.Play();
    }
  }

  /**
   * Broadcasted game status event - see GameController
   */
  public
  void GameKappaDefault () {
    prideMusic.Pause();
    if (!prideMusic.isPlaying) {
      defaultMusic.Play();
    }
  }

  /**
   * OnClick event for "Music Volume button" UI
   */
  public
  void OnMusicVolumeButton () {
    musicVolumeIndex = (musicVolumeIndex + 1) % musicVolumeLevels.Length;
    mixer.SetFloat("Music volume", musicVolumeLevels[musicVolumeIndex]);

    musicVolumeControlButton.image.overrideSprite = musicVolumeIcons[musicVolumeIndex];
  }
}
