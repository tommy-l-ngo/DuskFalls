using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EncounterSystem : MonoBehaviour
{
    [SerializeField] private Encounter[] enemiesInScene;
    [SerializeField] private int maxNumEnemies;

    private EnemyManager enemyManager;
    // Start is called before the first frame update
    void Start()
    {
        enemyManager = GameObject.FindAnyObjectByType<EnemyManager>();
        UnityEngine.Debug.Log("EnemyManager found: " + enemyManager);
        enemyManager.GenerateEnemiesByEncounter(enemiesInScene, maxNumEnemies);
        UnityEngine.Debug.Log("Enemies generated: " + enemyManager.GetCurrentEnemies().Count);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


[System.Serializable]

public class Encounter 
{
    public EnemyInfo Enemy;
    public int LevelMin;
    public int LevelMax;
}