using Consortya.Dungeon;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public enum LootType
{
    None,
    Swordsman,
    LootChest,
    Rat,
    Boss,
    WoodenDestructible
}

public abstract class TargetableObject : MonoBehaviour
{
    //Canvas that appears when the enemy is targeted
    public string targetName;

    //[HideInInspector]
    public Pod mPod;

    //[HideInInspector]
    public int ID;

    //Image of the health bar for  this enemy
    public Image mHealthBar;
    //Indicator for if this enemy is targeted
    public GameObject mIndicatorCircle;
    //Item dropped upon the enemy dying
    public GameObject mItemDrop;
    //Transform for
    public Transform mHitTransform;

    //ID declaring the type of enemy this is
    public LootType mLootType;
    //If this enemy has dropped loot
    private bool mHasDroppedLoot;

    protected bool isEnemy = false;

    //The max health of the enemy
    public int mMaxHealth = 50; //TODO: Set back to 100
                                //The current health of the enemy
    public uint CurHealth { get; private set; }

    protected virtual void Awake()
    {
        Debug.Assert(mLootType != LootType.None, "Type not specified");

        SetTargeted(false);
        //The enemy is at full health
        SetHealth(Convert.ToUInt32(mMaxHealth));

        //No loot has been dropped
        mHasDroppedLoot = false;
    }

    /// <summary>
    /// Take a given amount of damage away from health
    /// </summary>
    /// <param name="damage">The amount of damage taken</param>
    /// <returns>If the character is dead</returns>
    public bool TakeDamage(uint damage)
    {
        //The health after taking the damage
        long updatedHealth;
        //If its enough damage to kill
        if (damage >= CurHealth)
        {
            //set health to 0
            updatedHealth = 0;
        }
        else
        {
            //otherwise take the damage away from the current health
            updatedHealth = CurHealth - damage;
        }

        //Output how much damage is dealt
        Debug.Log($"damage {damage}");

        //Set the health to the updated value and return if it is destroyed
        bool destroyed = SetHealth((uint)updatedHealth);
        return destroyed;
    }

    /// <summary>
    /// Sets the current health to a given amount and returns if the enemy is destroyed
    /// </summary>
    /// <param name="health">The amount to set health to</param>
    /// <param name="canDropLoot">If the enemy drops loot on death</param>
    /// <returns>If the enemy was destroyed</returns>
    public bool SetHealth(uint health)
    {
        //Make sure health is non negative
        CurHealth = Math.Max(0, health);
        //Make sure the health is in the correct range
        Debug.Assert(CurHealth >= 0 && CurHealth <= mMaxHealth, "Current health is out of range");
        //Output the current health
        Debug.Log($"CurHealth {CurHealth}");

        if(mHealthBar != null)
        {
            var healthBar = mHealthBar.transform.parent.gameObject;
            if(CurHealth == mMaxHealth)
            {
                healthBar.SetActive(false);
            }
            else if (healthBar.activeSelf == false)
            {
                healthBar.SetActive(true);
            }

            //Fill the health bar based on remaining health
            mHealthBar.fillAmount = PercentHealth();
        }        

        //Determine if the enemy is destroyed
        bool destroyed = CurHealth <= 0;
        //If it is destroyed
        if (destroyed)
        {
            DestroyObject();
        }

        //Return if it was destroyed
        return destroyed;
    }

    public float PercentHealth()
    {
        return (float)CurHealth / mMaxHealth;
    }

    /// <summary>
    /// Drops the lootDrop prefab where the enemy dies
    /// </summary>
    public void DropLoot()
    {
        if (mHasDroppedLoot == false && mItemDrop != null)
        {
            Debug.Log("Dropped Loot");
            Debug.Assert(mHasDroppedLoot == false, "Second loot drop spawned");
            //Drop their loot
            var dropPoint = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            GameObject itemDrop = Instantiate(mItemDrop, dropPoint, Quaternion.identity);
            LootDrop lootDrop = itemDrop.GetComponent<LootDrop>();
            lootDrop.mLootType = mLootType;
            mHasDroppedLoot = true;
        }
    }

    /// <summary>
    /// Set if the enemy is being targeted
    /// </summary>
    /// <param name="indicatorEnabled">If the enemy is targeted</param>
    public void SetTargeted(bool targeted)
    {
        mIndicatorCircle.SetActive(targeted);
    }

    public bool IsEnemy()
    {
        return isEnemy;
    }

    protected virtual void DestroyObject()
    {
        //Output that it has no health
        Debug.Log(string.Format("Enemy Health at 0"));
        //Turn off it's targeting circle
        SetTargeted(false);

        DropLoot();

        //Leave the enemy inactive
        this.gameObject.SetActive(false);
    }

    public virtual void SetPod(Pod p)
    {
        mPod = p;
    }
}
