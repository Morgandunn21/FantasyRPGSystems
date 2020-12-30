using Consortya;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : TargetableObject
{
		//The AI for this enemy
	public EnemyAI mEnemyAI { private set; get; }

	[NonSerialized]
	public Action OnStatusChanged;
        
		//Called on spawn
    protected override void Awake()
    {
        base.Awake();
            
        isEnemy = true;
		//Get the AI for this enemy
		mEnemyAI = this.GetComponent<EnemyAI>();
    }

    public override void SetPod(Pod p)
    {
        base.SetPod(p);
        mEnemyAI.SetPod((EnemyPod)p);
    }
}
