using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections;

/**
 * Controls the game - loads levels, count points, etc..
 *
 * Broadcasts events (method GameObject.BroadcastMessage):
 *
 * GameEatMonster - player destroys a monster
 * GameEatCookie - player collects a cookie
 * GameEnd - game is lost
 * GameNewLevel - new level is loaded (start of the game or going to the next level)
 * GameVictory - game level is finished
 * GameKappaPride - player picked up a power up
 * GameDefault - power up(s) is/are expired
 */

public class GameController : MonoBehaviour {

  // singleton pattern
  private static GameController instanceRef;

  // Game elements
  public PlayerController player;
  public LabyrinthController labyrinth;

  // UI
  private Text gameOverText;
  private Text winnerText;
  private Text restartText;
  private Text continueText;
  private Text scoreText;
  private Text collectedText;
  private Image backdrop;
  private ScrollRect creditsScroll;

  public bool gameOver;
  public bool restart;
  public bool gameLost;

  public int difficultyLevel = 0;

  private bool dirtyUI = true;
  private int screenHeight = 1;

  private int cookiesCollected;
  public int CookiesCollected {
    get {return cookiesCollected;}
    set {
      if (value != cookiesCollected) {dirtyUI = true;}
      cookiesCollected = value;
      if (cookiesCollected >= labyrinth.cookieCount) {
        player.BeAWinner();
        WinGame();
      }
    }
  }

  private int score = -1;
  public int Score {
    get {return score;}
    set {
      if (value != score) {dirtyUI = true;}
      score = value;
    }
  }

  /**
   * Retrieve singleton instance of this component
   * Can be called from Awake() of other scripts
   */
  public static
  GameController GetSingletonInstance() {
    if (GameController.instanceRef) {
      return GameController.instanceRef;
    } else {
      return (GameController) FindObjectOfType(typeof(GameController));
    }
  }

  void Awake () {
    if (instanceRef == null) {
      instanceRef = this;
      DontDestroyOnLoad(gameObject);
      AwakeInit();

    } else {
      Destroy(gameObject);
      // TODO: replace with on scene load event delegates ???
      instanceRef.AwakeInit();
    }
  }

  /**
   * Called to init references to game level objects
   * after game started or level reloaded.
   * This locates relevant Game Objects and saves connections
   */
  void AwakeInit() {
    AdjustOrthoSize();

    if (SceneManager.GetActiveScene().name != "PlayLevel") {
      return;
    }

    // Connecting to game objects
    labyrinth = GameObject.Find("Labyrinth").GetComponent<LabyrinthController>();
    player = GameObject.Find("Player").GetComponent<PlayerController>();

    // Connection to UI
    gameOverText = GameObject.Find("Game over text").GetComponent<Text>();
    winnerText = GameObject.Find("Winner text").GetComponent<Text>();
    restartText = GameObject.Find("restart notification text").GetComponent<Text>();
    continueText = GameObject.Find("Continue notification text").GetComponent<Text>();
    scoreText = GameObject.Find("Points Text").GetComponent<Text>();
    collectedText = GameObject.Find("Collected text").GetComponent<Text>();
    backdrop = GameObject.Find("Backdrop shadow").GetComponent<Image>();
    creditsScroll = GameObject.Find("Scroll View").GetComponent<ScrollRect>();
    creditsScroll.gameObject.SetActive(false);

    // Connection to UI volume control buttons
    // musicVolumeControlButton = GameObject.Find("Music Volume button").GetComponent<Button>();;
    // effectsVolumeControlButton = GameObject.Find("Effects Volume button").GetComponent<Button>();;
    // musicVolumeControlButton.image.overrideSprite = musicVolumeIcons[musicVolumeIndex];
    // musicVolumeControlButton.onClick.AddListener(OnMusicVolumeButton);
    // effectsVolumeControlButton.image.overrideSprite = effectsVolumeIcons[effectsVolumeIndex];
    // effectsVolumeControlButton.onClick.AddListener(OnEffectsVolumeButton);

    // Initializing
    if (score < 0) {
      score = 0;
    }
    cookiesCollected = 0;
    gameOver = false;
    restart = false;
    gameLost = false;
    dirtyUI = true;

    BroadcastMessage("GameNewLevel", null, SendMessageOptions.DontRequireReceiver);
    // musicBox.SwitchNewLevel();
  }

	// Use this for initialization
	void Start () {
	}
	
  void AdjustOrthoSize () {
    screenHeight = Screen.height;

    // ortho_size = 0.5 * height / (PPU * PPUScale) = 0.5 * height / (128 * scale)
    float new_ortho_size = (float)screenHeight / 128;
    Camera c = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    c.orthographicSize = new_ortho_size;
  }

	// Update is called once per frame
	void Update () {
    // Handle resizing
    if (screenHeight != Screen.height) {
      AdjustOrthoSize();
    }

    // Handle timeless (absolute time stuff)
    // Handle endgame credit scrolling
    if (creditsScroll.gameObject.activeInHierarchy && creditsScroll.normalizedPosition.y > 0) {
      creditsScroll.normalizedPosition += (new Vector2(0, -0.05f) * Time.unscaledDeltaTime);
    }

    if (dirtyUI) {
      scoreText.text = "Points: " + score + "!";
      collectedText.text = "Collected: " + cookiesCollected + "/" + labyrinth.cookieCount;
      dirtyUI = false;
    }

		if (restart) {
			if (Input.GetKeyDown("space")) {
        if (gameLost) {
          difficultyLevel = 0;
          score = 0;

        } else {
          difficultyLevel++;
        }
        Time.timeScale = 1;
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
		}
	}

  /**
   * Called when player destroys a monster
   *
   * @param float points - amount of points value for the monster
   */
  public
  void EatMonster (int points) {
    Score += points;
    BroadcastMessage("GameEatMonster", null, SendMessageOptions.DontRequireReceiver);
  }

  public
  void EatCookie (int points) {
    Score += points;
    CookiesCollected++;
    BroadcastMessage("GameEatCookie", null, SendMessageOptions.DontRequireReceiver);
  }

  /**
   * Show game over screen. 
   */
  public
  void EndGame () {
    gameOverText.enabled = true;
    backdrop.enabled = true;

    if (gameOver)  { Debug.Log("EndGame called more than needed! ");}
    gameOver = true;
    gameLost = true;
    Time.timeScale = 0; // "pause" ...

    // TODO add a pause between game over and restart availability
    restartText.enabled = true;
    restart = true;
    creditsScroll.gameObject.SetActive(true);
    creditsScroll.normalizedPosition = new Vector2(0, 1);

    BroadcastMessage("GameEnd", null, SendMessageOptions.DontRequireReceiver);
  }

  /**
   * Call this when game is won
   */
  public
  void WinGame () {
    winnerText.enabled = true;
    gameLost = false;
    backdrop.enabled = true;
    gameOver = true;
    Time.timeScale = 0; // "pause" ...

    // TODO add a pause between game over and restart availability
    continueText.enabled = true;
    restart = true;

    BroadcastMessage("GameVictory", null, SendMessageOptions.DontRequireReceiver);
  }

  public
  void GoParade () {
    BroadcastMessage("GameKappaPride", null, SendMessageOptions.DontRequireReceiver);
  }

  public
  void StopParade () {
    BroadcastMessage("GameKappaDefault", null, SendMessageOptions.DontRequireReceiver);
  }
}