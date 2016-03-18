using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour {

	// public Text debug;
	public float movementSpeed = 1f; // labyrinth cells per second = unit/sec

	public LabyrinthController labyrinth;

	private Vector3 currentMovement; // store current movement vector
	private Vector3 cachedMovement; // store movement vector after planned future turn

	public Sprite spriteKappa;
	public Sprite spriteKappaPride;

	public AudioSource audioSourceEvents;
	public AudioSource audioSourcePersistent;

	public bool pride;
	public float prideTime = 3.0f;

	private float timeParadeOver = 0;

	// Use this for initialization
	void Start () {
		currentMovement = Vector3.zero;
		cachedMovement = Vector3.zero;
		pride = false;

		transform.position =  new Vector3(
			labyrinth.playerSpawnX,
			labyrinth.playerSpawnY,
			transform.position.z);
	}
	
	void Update_Sound () {
		if (currentMovement == Vector3.zero) {
			// disable top-top movement sound
			audioSourcePersistent.Pause();
			return;
		}

		GameObject g = GameObject.FindWithTag("GameController");

		if (!audioSourcePersistent.isPlaying && !g.GetComponent<GameController>().gameOver) {
			audioSourcePersistent.Play();
		}
	}

	void FixedUpdate () {
		if (pride && Time.time > timeParadeOver) {
			PrideParade(false);
		}

	}

	// Update is called once per frame
	void Update () {

		ProccessMovementInputs();
		Update_Sound();

		if (currentMovement == Vector3.zero) {
			return;
		}

		// proceed with movement
		float timeToMove = Time.deltaTime;
		float timeToCellCenter = GetSecondsToCellCenter();

		if (timeToCellCenter > timeToMove) {
			// simply move as much as possible inside the cell
			transform.position = transform.position + currentMovement * timeToMove * movementSpeed;

		} else {
			// when timeToCellCenter is 0, we are already in a center of this cell
			if (timeToCellCenter > 0) {
				AdjustToNextCellCenter();
			}

			// we are crossing cell center - time to decide if we want to turn, etc.
			timeToMove -= timeToCellCenter;

			// check if cached command applicable
			if (cachedMovement != Vector3.zero && IsMovementPossible(cachedMovement)) {
				// turn!
				currentMovement = cachedMovement;
				cachedMovement = Vector3.zero;
			}

			// move forward (after turn or simply forward to the cell exit)
			if (IsMovementPossible(currentMovement)) {
				transform.position = transform.position + currentMovement * timeToMove * movementSpeed;
			} else {
				currentMovement = Vector3.zero;
			}
		}
	}

	/**
	 * Get Input Axis and set currentMovement and cachedMovement accordingly
	 */
	void ProccessMovementInputs() {
		float horizontalMovement = Input.GetAxis("Horizontal");
		float verticalMovement = Input.GetAxis("Vertical");

		// normalize movements to [-1, 0, 1]
		horizontalMovement = (Mathf.Abs(horizontalMovement) > 0) ? Mathf.Sign(horizontalMovement) : 0;
		verticalMovement   = (Mathf.Abs(verticalMovement) > 0) ? Mathf.Sign(verticalMovement) : 0;

		// Flipping sprite!
		if (horizontalMovement != 0) {
			SpriteRenderer sr = GetComponent<SpriteRenderer>();
			sr.flipX = horizontalMovement < 0;
		}

		// get sameAxis / turn components of movement command
		Vector3 newDirection = new Vector3(horizontalMovement, verticalMovement, 0);
		Vector3 turnDirection = GetTurnMovement(newDirection);
		Vector3 alignedDirection = GetAlignedMovement(newDirection);

		// if there is a new command of moving along same axis - replace with new command
		if (alignedDirection != Vector3.zero) {
			currentMovement = alignedDirection;
			cachedMovement = turnDirection; // turn can be possibly overriden with zero only if directional command present
		}

		// if turn command is not zero - always remember it
		if (turnDirection != Vector3.zero) {
			cachedMovement = turnDirection;
		}
	}

	/**
	 * Checks if movement is possible from current position in a direction
	 * vector specified. Call this only when you are in a center of a labyrinth
	 * cell.
	 * 
	 * @param Vector3 newDirection - direction of movement suggested. 
	 *    Only one of x or y can be non zero. I.e. if x component non-zero then
	 *    y must be equal to zero and vice versa.
	 * @return bool
	 *   - true if movement in suggested direction is possible from current position
	 */
	bool IsMovementPossible(Vector3 newDirection) {
		Vector2 coords = GetCurrentCellCoords();

		int position_x = Mathf.RoundToInt(coords.x);
		int position_y = Mathf.RoundToInt(coords.y);
		int target_x = position_x + Mathf.RoundToInt(newDirection.x);
		int target_y = position_y + Mathf.RoundToInt(newDirection.y);

		LabyrinthController lc = labyrinth.GetComponent<LabyrinthController>();
		return lc.CanMoveFromCell(position_x, position_y, target_x, target_y);
	}

	/**
	 * Calculates coodinates of a cell we reside in in the labyrinth
	 *
	 * @return 
	 * 		Vector2.zero - error / outside of labyrinth.
	 * 		(1 .. labyrinth.width, 1 .. labyrinth.height)
	 */
	Vector2 GetCurrentCellCoords() {
		int x = Mathf.RoundToInt(transform.position.x);
		int y = Mathf.RoundToInt(transform.position.y);

		LabyrinthController lc = labyrinth.GetComponent<LabyrinthController>();
		
		// x is from 1 to lc.width (inclusive)
		// y is from 1 to lc.height (inclusive)
		if (y < 1 || x < 1 || x > (lc.width) || y > (lc.height)) {
			return Vector2.zero;
		} else {
			return new Vector2(x, y);
		}
	}

	/**
	 * Compare current movement with user input to check for possible turn command
	 * @param Vector3 newDirection - normalized to [-1, 0, 1] values for components X and Y
	 *
	 * @return Vector3
	 *   - if current movement is zero, returns first non-zero newDirection components, x or y axis
	 *   - if newDirection is zero or error in current movement, return Vector3.zero
	 */
	Vector3 GetTurnMovement(Vector3 newDirection) {
		if (currentMovement.x == 0 && newDirection.x != 0) {
			return new Vector3(newDirection.x, 0, 0);
		}

		if (currentMovement.y == 0) {
			return new Vector3(0, newDirection.y, 0);
		}

		return Vector3.zero;
	}

	/**
	 * Compare newDirection to the current movement to extract aligned movement component
	 * @param Vector3 newDirection - normalized to [-1, 0, 1] values for components X and Y
	 *
	 * @return Vector3
	 *   - if current movement is zero, returns first non-zero newDirection components, x or y axis
	 */
	Vector3 GetAlignedMovement(Vector3 newDirection) {
		if (currentMovement.x != 0) {
			return new Vector3(newDirection.x, 0, 0);
		}

		if (currentMovement.y != 0) {
			return new Vector3(0, newDirection.y, 0);
		}

		// here our currentMovement is zero (x, y)
		if (newDirection.x != 0) {
			return new Vector3(newDirection.x, 0, 0);

		} else {
			return new Vector3(0, newDirection.y, 0);
		}
	}

	/**
	 * How many seconds will it take with current movement direction to get to the next 
	 * cell center.
	 *
	 * @return float - 0 if already in a center of cell or no movement direction.
	 */
	float GetSecondsToCellCenter() {

		float direction = 0;
		float position = 0;

		// get position on movement axis and direction of movement
		if (currentMovement.x != 0) {
			direction = currentMovement.x;
			position = transform.position.x;
		} else if (currentMovement.y != 0 ) {
			direction = currentMovement.y;
			position = transform.position.y;
		}

		// return if no movement or already in center of a cell
		if (direction == 0) {
			return 0;
		}

		// errors in presentation of integers like 3.99999999f
		if (Mathf.Abs(Mathf.Round(position) - position) < 0.0001f) {
			return 0;
		}

		float distance = 0;

		// get distance to the next cell
		if (direction > 0) {
			distance = Mathf.Ceil(position) - position;
		} else {
			distance = position - Mathf.Floor(position);
		}

		// time = distance / velocity
		return distance / movementSpeed;
	}

	/**
	 * Along currentMovement direction snap to the next cell center
	 * this.currentMovement must have components with exact values
	 * of -1, 0 or +1
	 */
	void AdjustToNextCellCenter() {

		float direction = 0;
		float position = 0;
		bool isXMovement = false;

		// get position on movement axis and direction of movement
		if (currentMovement.x != 0) {
			direction = currentMovement.x;
			position = transform.position.x;
			isXMovement = true;
		} else if (currentMovement.y != 0 ) {
			direction = currentMovement.y;
			position = transform.position.y;
		} else {
			return;
		}

		float new_position = (direction > 0) ? Mathf.Ceil(position) : Mathf.Floor(position);
		transform.position = isXMovement ? 
			new Vector3(new_position, transform.position.y, transform.position.z) :
			new Vector3(transform.position.x, new_position, transform.position.z);
	}

	/**
	 * Call this when bitten by monster
	 */
	public
	void HitByMonster(MonsterController monster) {
		if (pride) {
			monster.DieKappa();

		} else {
			GameController gc = GameObject.FindWithTag("GameController").GetComponent<GameController>();
			if (!gc.gameOver) {
				gc.EndGame();
				audioSourcePersistent.Stop();
			}
		}
	}

	/**
	 * Called when cookie is being hit
	 */
	public
	void CollectCookie() {

	}

	/**
	 * Called to stop/start KappaPrideMan
	 * @param bool boy_next_door  - new state. True for enabling KappaPrideMan
	 */
	public void PrideParade (bool boy_next_door) {
		if (boy_next_door != pride) {
			pride = boy_next_door;
			SpriteRenderer sr = GetComponent<SpriteRenderer>();
			sr.sprite = boy_next_door ? spriteKappaPride : spriteKappa;
		}	

		GameController gc = GameObject.FindWithTag("GameController").GetComponent<GameController>();

		if (boy_next_door) {
			gc.GoParade();
			// set timer for end of KappaPrideMan
			if (timeParadeOver < Time.time) {
				timeParadeOver = Time.time;
			}
			timeParadeOver += prideTime;

		} else {
			gc.StopParade();
		}
	}

	public
	void BeAWinner () {
		audioSourcePersistent.Stop();
	}
}