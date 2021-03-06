﻿using UnityEngine;
using System.Collections;


/// <summary>
/// Enemy unit class that holds everything about an enemy unit (all the subclasses)
/// </summary>
public class EnemyUnit : CharacterConversable {

	// an enemy unit has health, attack, and character items
	public EnemyHealth enemyHealth;
	public EnemyAttack enemyAttack;
	public EnemyCharacter enemyCharacter;


	// PUBLIC VARIABLES FOR SETTING INFORMATION ABOUT AN ENEMY
	public int enemyCurrentHealth;
	public int enemyMaxHealth;
	public string enemyName;
	public int enemyArmor;
	public int enemySpellResistance;
	public int enemyExperience;
	public int enemyGold;
	public int baseDamage;
	public int weaponDamage;




	// some characteristics about the enemy to start
	void Start()
	{
		enemyHealth = this.GetOrAddComponent<EnemyHealth> ();
		enemyAttack = this.GetOrAddComponent<EnemyAttack> ();
		enemyCharacter = this.GetOrAddComponent<EnemyCharacter> ();
		enemyAttack.attackDamage = baseDamage + weaponDamage;

		enemyHealth.currentHealth = enemyCurrentHealth;
		enemyHealth.maxHealth = enemyMaxHealth;
		enemyCharacter.gameObjectPlayerName = enemyName;
	}

}
