using System;
using UnityEngine;

/// <summary>
/// Toolbox Instance: controls our data
/// </summary>
public class Toolbox : Singleton<Toolbox> {
	protected Toolbox () {} // guarantee this will be always a singleton only - can't use the constructor!
 


	public GameObject playerCharacter;
	public Vector2 positionInLastScene;
	public Vector2 battlePosition;

	public string enemyDefeated;
 
	public Language language = new Language();
	public bool sceneAlreadyLoaded = false;
	public int currentSaveSlot;
	public DamageData damageCalculations;

	public string battleScene;


	public bool isLocked = false;
 
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake () 
	{
		// Your initialization code here
		// should rarely have to awaken here..
		this.name = "Toolbox";
		this.tag = "GameManager";
		//damageCalculations = new DamageData (damageDataText);
	}
 

	// (optional) allow runtime registration of global objects
	static public T RegisterComponent<T> () where T: Component {
		return Instance.GetOrAddComponent<T>();
	}



	/// <summary>
	/// Raises the level was loaded event. If a level was loaded, see if we need to
	/// destroy any units or if we need to freeze or move units or camera objects
	/// </summary>
	/// <param name="level">Level.</param>
	void OnLevelWasLoaded(int level)
	{

		isLocked = false;

		if (playerCharacter != null)
		{

			if ((positionInLastScene.Equals (null) || positionInLastScene.Equals (Vector2.zero)) && GameObject.FindGameObjectWithTag ("Respawn"))
			{
				Debug.Log ("WE NEED TO RESET POSITION" + positionInLastScene);

				// find the starting point
				positionInLastScene = GameObject.FindGameObjectWithTag ("Respawn").transform.position;
			} else
			{
				//Debug.Log ("we are here with a position that is not null" + positionInLastScene);
			}

			if (level != 1 && level != 0)
			{
				// make sure our camera follow is set up to our main camera
				Camera.main.GetComponent<CameraFollow> ().target = playerCharacter.transform;


				Debug.Log ("PLAYER IN LAST POSITION : " + positionInLastScene);
				playerCharacter.transform.position = positionInLastScene;
				playerCharacter.GetComponent<PlayerUnit> ().freeze = false;

				// just set a basic 1 health if we are a dead character
				if (playerCharacter.GetComponent<PlayerHealth> ().currentHealth <= 0)
				{
					playerCharacter.GetComponent<PlayerHealth> ().currentHealth = 1;
				}


				// if we have a damage numbers script, remove it
				Destroy (playerCharacter.GetComponent<DamageNumbers> ());

				playerCharacter.GetComponent<Animator> ().SetBool ("IsFighting", false);
				Destroy (playerCharacter.GetComponent<DamageNumbers> ());

				foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
				{
					player.SetActive (false);

					// set the other characters as not showing
					// remove components
					Destroy (player.GetComponent<DamageNumbers> ());

				}

				// this has to happen at the end here so that we don't get into multiple fights ideally
				sceneAlreadyLoaded = false;
				battleScene = "";

			} 
			else if (level == 1)
			{
				if (GameObject.FindGameObjectWithTag ("Enemy") != null)
				{

					// for each enemy and player type tag?
					foreach (Animator enemy in GameObject.FindGameObjectWithTag("Enemy").GetComponentsInChildren<Animator>(true))
					{
						enemy.gameObject.SetActive (true);
						enemy.SetBool ("IsFighting", true);
					}
				}

				foreach (Animator player in GameObject.FindGameObjectWithTag("PlayerCharacter").GetComponentsInChildren<Animator>(true))
				{
					player.gameObject.SetActive (true);
					player.SetBool ("IsFighting", true);
				}

				// this has to happen at the end here so that we don't get into multiple fights ideally
				sceneAlreadyLoaded = true;

			}




			// if an enemy was defeated, destroy it from the scene.
			// we'll have to make this persist later.
			if (enemyDefeated != null)
			{
				Destroy (GameObject.Find (enemyDefeated));
				enemyDefeated = null;
			}

		}



	}
}
 
[System.Serializable]
public class Language {
	public string current;
	public string lastLang;
}
