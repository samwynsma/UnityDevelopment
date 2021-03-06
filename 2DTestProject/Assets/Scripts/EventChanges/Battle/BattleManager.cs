﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


/// <summary>
/// Battle manager : stores the state of the battle
/// </summary>
public class BattleManager : MonoBehaviour {

	public enum BATTLE_STATES
	{
		START = 0,
		DECIDE_TURN = 1,
		DECIDE_ATTACK = 2,
		PERFORM_COMMANDS = 3,
		CHECK_CONDITIONS = 4,
		WIN = 5,
		LOSE = 6
	};
			
	public BATTLE_STATES currentState;

	// classes we use
	LoadBattleScene sceneInit;
	CharacterConversable currentPlayerTurn;
	List<CharacterConversable> battleTurnOrder;
	int currentTurn = 0;
	public bool waitingForTurn = false;
	public bool turnFinished = false;
	public string attackDone;
	public Text displayAttackText;
	public GameObject attackTextPanel;
	public List<EnemyUnit> enemies;
	public List<PlayerUnit> teammates;

	private bool turnsLoaded = false;

	// maybe it should be up to the battle manager to store good guys and bad guys?



	// Use this for initialization
	void Start () 
	{

		// if we have a gameboy game, delete this object
		if (Toolbox.Instance.battleScene != null && Toolbox.Instance.battleScene != "")
		{
			Destroy (this);
		}

		// The battle manager contains a list of scripts as attached to objects
		// INIT will get the list of objects and do some setup
		// DECIDE ON TURN will call the queue and toggle at this point
		// DECIDE ATTACK will simply call the units attack
		// PERFORM COMMANDS will perform the attack's commands
		// Check conditions will just determine if we are done with the round
		// WIN will fire the winning conditions off
		// LOSE will fire the losing conditions off

		// Battle Manager
		// - START / INIT
		// - DECIDE WHOSE TURN IT IS (speed and turn queue? - for now we'll just swap off in a simple turn based queue)
		// - DECIDE ATTACK (random if enemy)
		// - PERFORM COMMANDS
		// - CHECK CONDITIONS
		// - WIN
		// - LOSE

		currentState = BATTLE_STATES.START;
		sceneInit = GameObject.Find ("LoadScene").GetComponent<LoadBattleScene> ();



		Camera.main.orthographicSize = 2;
		Camera.main.transform.position = new Vector3 (-4.30f, -1.50f, -10);


			

	}


	/// <summary>
	/// Update this instance. Controls our state machine. Uses ToolBox
	/// </summary>
	// Update is called once per frame
	void Update () 
	{

		// if we are locked, can't go forward
		if (Toolbox.Instance.isLocked)
			return;

		// lock our toolbox
		Toolbox.Instance.isLocked = true;
		Toolbox toolboxInstance = Toolbox.Instance;
	
		// switch over our battle state
		switch (currentState)
		{


		// load our scene and move our battle forward
		case BATTLE_STATES.START:
			Debug.Log ("We are in start here" + turnsLoaded);
			if (!turnsLoaded)
			{
				turnsLoaded = true;
				sceneInit.LoadTurns ();
				Toolbox.Instance.isLocked = false;
			}
			else
			{
				sceneInit.LoadBattleSceneItems ();
				currentState = BATTLE_STATES.DECIDE_TURN;
				battleTurnOrder = sceneInit.turnOrder;
				Toolbox.Instance.isLocked = false;
			}
				break;


		// decide whose turn it is
		case BATTLE_STATES.DECIDE_TURN:
			
			Debug.Log ("We are deciding turn : ");
			currentState = BATTLE_STATES.DECIDE_ATTACK;

			// pick the turn person
			currentPlayerTurn = battleTurnOrder [currentTurn];

			// update our current turn to the next player in the queue
			// and cycle if we are at the end.
			currentTurn++;
			if (currentTurn >= battleTurnOrder.Count)
				currentTurn = 0;


			turnFinished = false;
			waitingForTurn = false;
			Toolbox.Instance.isLocked = false;


			break;



		// decide which attack we are using on whoever's turn it is
		case BATTLE_STATES.DECIDE_ATTACK:

			// skip this turn if our player has no health
			if (currentPlayerTurn.isPlayerCharacter && currentPlayerTurn.GetComponent<PlayerHealth> ().currentHealth <= 0)
			{
				waitingForTurn = true;
				currentState = BATTLE_STATES.CHECK_CONDITIONS;
				Toolbox.Instance.isLocked = false;
			} 
			else if (!currentPlayerTurn.isPlayerCharacter && currentPlayerTurn.GetComponent<EnemyHealth> ().currentHealth <= 0)
			{
				
				waitingForTurn = true;
				currentState = BATTLE_STATES.CHECK_CONDITIONS;
				Toolbox.Instance.isLocked = false;
			}



			// get our battle manager if we are a good guy.
			// Otherwise, perform an attack if we are a bad guy
			if (!waitingForTurn)
			{
				waitingForTurn = true;
				turnFinished = false;

				Debug.Log ("Whose turn is it? : " + currentPlayerTurn.name + " " + currentPlayerTurn.isPlayerCharacter);

				if (currentPlayerTurn.isPlayerCharacter)
				{
					// get their battle mode and start gaming
					currentPlayerTurn.gameObject.GetComponent<BattleMenu> ().isMyTurn = true;
				}

				// otherwise, if it is not my turn, enemy attack!
				else
				{
					StartCoroutine (currentPlayerTurn.gameObject.GetComponent<EnemyAttack> ().Attack ());
				}
			}

			if (turnFinished)
			{
				currentState = BATTLE_STATES.PERFORM_COMMANDS;
				Toolbox.Instance.isLocked = false;

			}

			break;

		// perform commands of the attack
		case BATTLE_STATES.PERFORM_COMMANDS:
				// let's do a little enum in here
				StartCoroutine (displayAttack ());

				
			break;

		// check the conditions of the battle - has someone won? is any unit
		// to be destroyed?
		case BATTLE_STATES.CHECK_CONDITIONS:

			bool playerStillAlive = false;
			bool enemyStillAlive = false;

			// if we have all of the enemies killed
			// if we have the player characters all dead..
			Debug.Log("ALLIES LENGTH : " + teammates.Count + " ENEMIES LENGTH : " + enemies.Count);

			// check if any of our players are still alive
			// if all are dead, we've lost this battle
			foreach (var player in teammates)
			{
				if (player.playerHealth.currentHealth > 0)
				{
					playerStillAlive = true;
				}
			}

			if (!playerStillAlive)
			{
				currentState = BATTLE_STATES.LOSE;
				Toolbox.Instance.isLocked = false;
				break;
			}

			// check if any of our enemies are still alive - if all are dead
			// then we are done with this battle
			foreach (var player in enemies)
			{
				if (player.enemyHealth.currentHealth > 0)
				{
					enemyStillAlive = true;
				}
			}

			if (!enemyStillAlive)
			{
				currentState = BATTLE_STATES.WIN;
				Toolbox.Instance.isLocked = false;
				break;
			}

			// if we have all of the enemies killed
			// if we have the player characters all dead..
			Debug.Log("ALLIES LENGTH : " + teammates.Count + " ENEMIES LENGTH : " + enemies.Count);

			// first check if the player characters are all dead
			// then check if the enemy characters are all dead
			Debug.Log("we are here in check condition");

			// for each of the enemies in play - are we all dead?
			// for each of the players in play, are we all dead?


			currentState = BATTLE_STATES.DECIDE_TURN;
			Toolbox.Instance.isLocked = false;
			break;

		// if the player side has won, hooray! victory conditions and experience
		case BATTLE_STATES.WIN:
			
			toolboxInstance.sceneAlreadyLoaded = false;

			changeCharacterStates ();

			// load previous scene
			toolboxInstance.positionInLastScene = toolboxInstance.battlePosition;

			// enemy defeated
			GameObject enemyDefeated = GameObject.FindGameObjectWithTag ("Enemy");
			toolboxInstance.enemyDefeated = enemyDefeated.name;
			Destroy (enemyDefeated);

			// return to enemy position from last scene.
			SceneManager.LoadScene("OpeningScene");


			break;

		// if the good side has lost, sad day. Penalties and teleport
		case BATTLE_STATES.LOSE:
			Debug.Log ("we have lost");
			toolboxInstance.sceneAlreadyLoaded = false;

			changeCharacterStates ();





			// spawn at least location?
			// what if the grue is still there?
			// we'll get this in a moment.
			SceneManager.LoadScene("OpeningScene");


			break;

		// if we hit a non-existent case
		default:
			break;

		}


	}



	/// <summary>
	/// Changes the character states.
	/// </summary>
	private void changeCharacterStates()
	{

		// print out sort order
		foreach (var character in battleTurnOrder)
		{
			// set every character to 
			character.GetComponent<Animator>().SetBool("IsFighting", false);
		}
	}


	/// <summary>
	/// Displays the attack in the text panel
	/// </summary>
	/// <returns>The attack.</returns>
	private IEnumerator displayAttack()
	{
		// wait for a couple of seconds and then we'll change the state
		attackTextPanel.SetActive(true);
		displayAttackText.text = currentPlayerTurn.playerName + " " + attackDone;
		yield return new WaitForSeconds (2);

		currentState = BATTLE_STATES.CHECK_CONDITIONS;

		attackTextPanel.SetActive(false);
		Toolbox.Instance.isLocked = false;
	}


}
