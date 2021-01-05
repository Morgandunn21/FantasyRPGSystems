using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.AI;


[RequireComponent(typeof(SphereCollider))]
public class EnemyPod : Pod
{
    public LootType[] enemies;
    public Transform[] waypointList;
    public int maxPodRange = 20;
    public Transform rotationTarget;

    System.Action attackableEntered;
    System.Action attackableLeft;

    [HideInInspector]
    public SphereCollider spherCol;

    private Dictionary<LootType, List<Enemy>> enemyDict;

    protected override void Awake()
    {
        base.Awake();

        enemyDict = new Dictionary<LootType, List<Enemy>>();

        spherCol = GetComponent<SphereCollider>();
    }

    protected override void Start()
    {
        GameObject[] enemyPrefabs = PodIDManager.Instance.objectPrefabs;

        foreach(var enemyType in enemies)
        {
            GameObject enemyObject = Instantiate(enemyPrefabs[(int)enemyType], FindSpawnPosition(), Quaternion.identity, this.transform);

            if(rotationTarget != null)
            {
                enemyObject.transform.LookAt(rotationTarget, Vector3.up);
            }

            AddEnemy(enemyObject.GetComponent<EnemyAI>());
        }

        Initialized = true;
    }

    private void EnemyAIKilled(EnemyAI enemyAI)
    {

        //Remove the enemy from the pod
        objectList.Remove(enemyAI.enemy);

        //Get the type of the enemy
        LootType enemyType = enemyAI.enemy.mLootType;

        //Get the list of enemies in the pod of that type
        List<Enemy> enemiesOfType;
        if (!enemyDict.TryGetValue(enemyType, out enemiesOfType))
        {
            Debug.LogError("No List for this Enemy Type");
            return;
        }

        int index = enemiesOfType.IndexOf(enemyAI.enemy);

        //If this is the last enemy in the list of that type
        if (index == enemiesOfType.Count - 1)
        {
            //remove it from the list
            enemiesOfType.RemoveAt(enemiesOfType.Count - 1);
        }
        else
        {
            //otherwise just set its status to false
            enemiesOfType[index] = null;
        }
    }

    public EnemyAI GetNextEnemy(int curEnemyID)
    {
        Debug.Log($"Num enemies: {objectList.Count}");
        Debug.Log($"Current Enemy ID: {curEnemyID}");
        Debug.Assert(objectList.Count >= 0);
        Debug.Assert(curEnemyID < objectList.Count);

        int nextEnemyID = curEnemyID;

        for (int i = 0; i < objectList.Count; i++)
        {
            if (nextEnemyID < (objectList.Count - 1))
            {
                nextEnemyID++;
            }
            else
            {
                nextEnemyID = 0;
            }

            Enemy enemy = ((Enemy)objectList[nextEnemyID]);
            if (enemy != null && enemy.CurHealth > 0)
            {
                return enemy.mEnemyAI;
            }
        }

        return null;
    }

    public bool IsObjectWithinRange(Transform obj)
    {
        bool isInRange = (obj != null && Vector3.Distance(obj.transform.position, this.transform.position) <= maxPodRange);
        return isInRange;
    }

    public void AddEnemy(EnemyAI enemyAI)
    {
        AddObject(enemyAI.enemy);

        SetName(enemyAI.enemy);

        enemyAI.OnDestroyAction = EnemyAIKilled;

        attackableEntered += enemyAI.AttackableEnteredPod;
        attackableLeft += enemyAI.AttackableLeftPod;

        enemyAI.SetPod(this);
    }

    public void RemoveEnemy(EnemyAI enemyAI)
    {
        RemoveObject(enemyAI.enemy);

        enemyAI.OnDestroyAction = null;

        attackableEntered -= enemyAI.AttackableEnteredPod;
        attackableLeft -= enemyAI.AttackableLeftPod;

        List<Enemy> enemiesOfType;

        //if a dictionary entry does exist for this type, remove this enemy
        if (enemyDict.TryGetValue(enemyAI.enemy.mLootType, out enemiesOfType))
        {
            if(enemiesOfType.Contains(enemyAI.enemy))
            {
                enemiesOfType.Remove(enemyAI.enemy);
            }                
        }

        enemyAI.SetPod(null);
    }

    private void SetName(TargetableObject enemy)
    {
        //Get the loot type of the enemy
        LootType enemyType = enemy.mLootType;

        //Get the list of enemies in the pod of that type
        List<Enemy> enemiesOfType;

        //if a dictionary entry does not exist for this type, make one
        if (!enemyDict.TryGetValue(enemyType, out enemiesOfType))
        {
            enemiesOfType = new List<Enemy>();
            enemyDict.Add(enemyType, enemiesOfType);
        }

        //If there is only one enemy, no tag required
        if (enemiesOfType.Count == 0)
        {
            enemiesOfType.Add((Enemy)enemy);
        }
        //else, if this is adding the second enemy, rename the first enemy with a tag
        else if (enemiesOfType.Count == 1)
        {
            enemiesOfType[0].targetName = $"{enemy.targetName} {FormatTag(0)}";
            enemy.targetName = $"{enemy.targetName} {FormatTag(1)}";
            enemiesOfType.Add((Enemy)enemy);
        }
        //otherwise give this enemy a tag based on how many enemies of this type are in the pod
        else
        {
            //Find an empty spot in the list of enemies
            int index;
            for (index = 0; index < enemiesOfType.Count; index++)
            {
                if (enemiesOfType[index] == null)
                {
                    //Fill that slot in the list of enemies of this type
                    enemiesOfType[index] = (Enemy)enemy;
                    break;
                }
            }

            //If the list is full, add this enemy to the end
            if (index >= enemiesOfType.Count)
            {
                enemiesOfType.Add((Enemy)enemy);
            }

            //Name the enemy based on its type and its index
            enemy.targetName = $"{enemy.targetName} {FormatTag(index)}";
        }
    }

    /// <summary>
    /// Takes a string in camel or title case and adds spaces btween the words
    /// </summary>
    /// <param name="text">the string to format</param>
    /// <returns>the formatted string</returns>
    private string FormatName(string text)
    {
        string result;

        //Capitalize the name
        if (text.Length == 0)
        {
            result = "No Name";
        }
        else if (text.Length == 1)
        {
            result = char.ToUpper(text[0]).ToString();
        }
        else
        {
            result = $"{char.ToUpper(text[0])}{text.Substring(1)}";
        }

        //Insert spaces before Capital letters or numbers
        result = Regex.Replace(result, "([a-z](?=[A-Z0-9])|[A-Z](?=[A-Z][a-z]))", "$1 ");

        return result;
    }

    /// <summary>
    /// Calculates the tag to append to the name
    /// </summary>
    /// <param name="i">the index of this enemy</param>
    /// <returns>The tag to append to its name</returns>
    private string FormatTag(int i)
    {
        string result = string.Empty;

        //If more than 26 enemies, call FormatTag again
        if (i / 26 > 0)
        {
            //This results in tags like "AA" after the first 26 enemies
            result = $"{result}{FormatTag((i / 26) - 1)}";

            i -= 26 * (i / 26);
        }

        //Append the last letter to the tag
        result = $"{result}{(char)(65 + i)}";

        //return the result
        return result;
    }

    private Vector3 FindSpawnPosition()
    {
        Debug.Assert(waypointList.Length > 0, "No Waypoints");

        int waypoinIndex = Random.Range(0, waypointList.Length);
        Debug.Assert(waypoinIndex < waypointList.Length, $"Invalid waypoinIndex: {waypoinIndex}");

        GameObject waypoint = waypointList[waypoinIndex].gameObject;
        Vector3 spawnPoint = waypoint.transform.position + Random.insideUnitSphere * waypoint.GetComponent<SphereCollider>().radius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            spawnPoint = hit.position;
        }

        return spawnPoint;
    }
}
