using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSpell : ScriptableObject
{
    [Header("Damage Range")]
    //Minimum damage the spell can deal
    public uint DmgMin;
    //Maximum damage the spell can deal
    public uint DmgMax;
    //Damage the spell does to target's stamina
    public uint DmgStamina;

    [Header("Mana Cost")]
    //Amount of Mana required to cast the spell
    public uint MPCost;

    [Header("Spell Range")]
    //Maximum range of this spell
    public uint Range;

    [Header("Casting Animation")]
    //Animation played on the player model when casting the spell
    public string AnimationName;

    [Header("Spell Attach Point")]
    //Where on the player model this spell is spawned from
    public PlayerAttachPoint mAttachPoint;

    [Header("Spell Timing")]
    //How long it takes to cast the spell
    public float CastDuration;
    //How long does the spell effect last
    public float SpellDuration;
    //How long to wait for cooldown
    public float Cooldown;

    [Header("Spell Particle Effect")]
    //Prefab for attack particle effect
    public GameObject ParticlePrefab;

    [Header("Spell Status Effect")]
    //Holds information for the status effect this spell applies
    public StatusEffectSO statusEffect;

    [NonSerialized]
    public bool onCooldown;

    //Cached when the spell is used
    protected GameObject userGO;
    protected Player player;
    protected PlayerControl playerControl;
    //tracks how long this spell has been on cooldown
    private float curCooldown = 0f;

    /// <summary>
    /// Determines if a spell is targeted
    /// </summary>
    public abstract bool RequiresTarget();
    /// <summary>
    /// Calculates the total cooldown of a spell, including casting time and spell duration
    /// </summary>
    /// <returns>Total cooldown in seconds</returns>
    protected abstract float CalculateCooldown();

    /// <summary>
    /// Applies the spell's status effect to a given target
    /// </summary>
    /// <param name="attachTrans">Where the status effect particles attatch to</param>
    /// <param name="target">The target recieving the effect</param>
    protected virtual void ApplySpell(Transform attachTrans, TargetableObject target)
    {
        //Only call ApplyEffect if this spell has a status effect
        if (statusEffect != null)
        {
            statusEffect.ApplyEffect(target);
        }
    }

        
    public bool Use(GameObject userObj, Transform attachTrans, TargetableObject target)
    {
        //Is the spell cooling down?
        if (onCooldown == true)
        {
            return false;
        }

        userGO = userObj;

        //Determine whether it is for the player or the guest
        player = userObj.GetComponent<Player>();
        playerControl = userObj.GetComponent<PlayerControl>();

        //Only the player does the damage portion
        if (player != null && playerControl != null)
        {
            if (ApplyMPCost() == false)
            {
                return false;
            }

            //AOE spells typically have a null enemy
            if (target != null)
            {
                //Look at the enemy
                userObj.transform.LookAt(new Vector3(target.transform.position.x, userObj.transform.position.y, target.transform.position.z));
            }

            //Deactivate other spells for the cast duration
            playerControl.mAttackControl.SetCooldowns(CastDuration, this);
            //Start Cooldown
            HandleCooldown(CalculateCooldown());
        }

        //apply the spell to the target
        ApplySpell(attachTrans, target);

        return true;
    }

    /// <summary>
    /// Subtracts the spells mana cost from the user
    /// </summary>
    /// <returns>If the user had enough mana to cast the spell</returns>
    protected bool ApplyMPCost()
    {
        //If the player has been initialized, return if they have enough mana
        if (player != null)
        {
            return player.SubtractStat(Player.StatType.MAGIC, MPCost);
        }
        //Otherwise return false
        return false;
    }

    /// <summary>
    /// Deals a given amount of damage to a given target
    /// </summary>
    /// <param name="target">The target hit by the spell</param>
    /// <param name="damage">The amount of damage dealt</param>
    protected bool DealDamage(TargetableObject target, uint damage)
    {
        //Store if the target survived the damage
        bool killed = target.TakeDamage(damage);

        return killed;
    }

    /// <summary>
    /// Handles starting and updating the spells cooldown
    /// </summary>
    /// <param name="cooldown">The total cooldown time in seconds</param>
    public void HandleCooldown(float cooldown)
    {
        //If this spell is already on cooldown
        if (onCooldown)
        {
            //if the new cooldown is longer than whats left of the current cooldown
            if (cooldown > curCooldown)
            {
                //set the current cooldown to the new cooldown
                curCooldown = cooldown;
            }
            //return
            return;
        }
        else
        {
            //otherwise run the cooldown coroutine
            Timing.RunCoroutine(_Cooldown(cooldown));
        }
    }

    /// <summary>
    /// Coroutine for continuously updating the spells cooldown and cooldown UI
    /// </summary>
    /// <param name="cooldown"></param>
    /// <returns></returns>
    private IEnumerator<float> _Cooldown(float cooldown)
    {
        Debug.Assert(cooldown > 0f, "Invalid Cooldown");

        //Set this spell on cooldown
        onCooldown = true;

        //set the current cooldown to the original cooldown
        curCooldown = cooldown;

        //while there is time left on the cooldown
        while (curCooldown > 0)
        {
            //if curCooldown has been updated to be larger than the original cooldown
            if (curCooldown > cooldown)
            {
                //update the original cooldown
                cooldown = curCooldown;
            }

            //Decrease the current cooldown
            yield return Timing.WaitForOneFrame;
            curCooldown -= Time.deltaTime;
        }
        //take the spell off cooldown
        onCooldown = false;
    }
}
