using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
public class BattleSystem : MonoBehaviour
{
 
    private enum BattleState { Start, Selection, Battle, Won, Lost, Run }
    [Header("Battle State")]
    [SerializeField] private BattleState state;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();

    [Header("UI")]
    [SerializeField] private GameObject[] enemySelectionButtons;
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomTextPopUp;
    [SerializeField] private TextMeshProUGUI bottomText;

    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private int currentPlayer;

    private const string ACTION_MESSAGE = "s Action:";
    private const string WIN_MESSAGE = "The Enemies have been vanquished";
    private const string LOST_MESSAGE = "YOU SUCK";
    private const int TURN_DURATION = 2;
    private const int RUN_CHANCE = 50;
    private const string OVERWORLD_SCENE = "Overworld";
    private const string RAN_SUCCESS = "Your paty fleed the Battle";
    private const string RAN_FAIL = "Your paty failed to flee";  


    void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
        DetermineBattleOrder();
        //        AttackAction(allBattlers[0], allBattlers[1]);
    }

    private IEnumerator BattleRoutine()
    { 
        enemySelectionMenu.SetActive(false);
        state = BattleState.Battle;
        bottomTextPopUp.SetActive(true);

        for (int i = 0; i < allBattlers.Count; i++)
        {

            if (state == BattleState.Battle && allBattlers[i].CurrHealth >0)
            {

                switch (allBattlers[i].BattleAction)
                {
                    case BattleEntities.Action.Attack:
                        //do the attack
                        yield return StartCoroutine(AttackRoutine(i));
                        break;
                    case BattleEntities.Action.Run:
                        yield return StartCoroutine(RunRoutine());
                        break;
                    default:
                        Debug.Log("Error - incorrect battle action");
                        break;
                }
            }
        }

        RemoveDeadBattlers();
        if(state == BattleState.Battle) 
        {
            bottomTextPopUp.SetActive(false);
            currentPlayer = 0;
            ShowBattleMenu();
        }
        yield return null;
    }
    private IEnumerator AttackRoutine(int i)
    {
        //players turn attacks selected enemy, wait, then kill enemy
        if (allBattlers[i].IsPlayer == true)
        {
            BattleEntities currAttacker = allBattlers[i];
            if (allBattlers[currAttacker.Target].CurrHealth <=0)
            {
                currAttacker.SetTarget(GetRandomEnemy());
            }

            BattleEntities currTarget = allBattlers[currAttacker.Target];
            AttackAction(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);

            if (currTarget.CurrHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);
                enemyBattlers.Remove(currTarget);
                if (enemyBattlers.Count <= 0)
                {
                    state = BattleState.Won;
                    bottomText.text = WIN_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    SceneManager.LoadScene(OVERWORLD_SCENE);
                }
            }
        }  
        //if no enemy remain, we won battle

        if (i< allBattlers.Count && allBattlers[i].IsPlayer == false)
        {
            BattleEntities currAttacker = allBattlers[i];
            currAttacker.SetTarget(GetRandomPartyMember());
            BattleEntities currTarget = allBattlers[currAttacker.Target];

            AttackAction(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);

            if (currTarget.CurrHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);
                playerBattlers.Remove(currTarget);
            }
            if (playerBattlers.Count <= 0)
            {
                state = BattleState.Lost;
                bottomText.text = LOST_MESSAGE;
                yield return new WaitForSeconds(TURN_DURATION);
                Debug.Log("GAME OVER");
            }
            //enemy turn attacks player, wait, then kill party member, if no one around, we havbe lost
            // target will be random party member, which we will pull from 
        }
    }
    private IEnumerator RunRoutine()
    {
        if(state == BattleState.Battle)
        {
            if(Random.Range(1,101) >= RUN_CHANCE) 
            {
                //WE RAN AWAY, CONFORMED BY TEXY            SharedEditorTextureDictionary STATE TO RUN, CLEAR ALL BATTLERS, LOAD TO OVERWORLS, WAIT A FEW
                bottomText.text = RAN_SUCCESS;
                allBattlers.Clear();
                yield return new WaitForSeconds(TURN_DURATION);
                SceneManager.LoadScene(OVERWORLD_SCENE);
                yield break;            
            }
            else 
            {
                //WE FAIL, CONFIRM BY BOTTOM TEXT, WAIT A FEW
                bottomText.text = RAN_FAIL;
                yield return new WaitForSeconds(TURN_DURATION);
            }
        }
    }
    private void RemoveDeadBattlers() 
    { 
    for (int i = 0; i < allBattlers.Count; i++) 
        {
            if (allBattlers[i].CurrHealth <= 0)
            {
                allBattlers.RemoveAt(i);
            }
        }
    }

    private void CreatePartyEntities()
    {
        //get current party
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetAliveParty();


        //create battle entities for the party
        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentParty[i].MemberName, currentParty[i].CurrHealth, currentParty[i].MaxHealth, currentParty[i].Level, currentParty[i].Strength, currentParty[i].Initiative, true);



            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].MemberBattleVisualPrefab,
                partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();

            tempBattleVisuals.SetStartingValues(currentParty[i].CurrHealth, currentParty[i].MaxHealth, currentParty[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);
        }
    }

    private void CreateEnemyEntities()
    {
        //get enemies
        List<Enemy> currentEnemies = new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();


        //create battle entities for the party
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentEnemies[i].EnemyName, currentEnemies[i].CurrHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Level, currentEnemies[i].Strength, currentEnemies[i].Initiative, false);

            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].EnemyVisualPrefab, enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentEnemies[i].CurrHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);
        }

        // assign them fucked values



    }


    public void ShowBattleMenu()
    {
        //whos action it is
        actionText.text = playerBattlers[currentPlayer].Name + ACTION_MESSAGE;
        battleMenu.SetActive(true);

    }

    public void ShowEnemySelectionMenu()
    {
        battleMenu.SetActive(false);
        SetEnemySelectionButtons();
        enemySelectionMenu.SetActive(true);

    }

    public void SetEnemySelectionButtons()
    {
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            enemySelectionButtons[i].SetActive(false);

        }

        for (int j = 0; j < enemyBattlers.Count; j++)
        {
            enemySelectionButtons[j].SetActive(true);
            enemySelectionButtons[j].GetComponentInChildren<TextMeshProUGUI>().text = enemyBattlers[j].Name;
        }
    }

    public void SelectEnemy(int currentEnemy)
    {
        // sestting current members target ---> tell battle sys the members intention to who to attack ----> increment through paRT
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        currentPlayerEntity.SetTarget(allBattlers.IndexOf(enemyBattlers[currentEnemy]));

        currentPlayerEntity.BattleAction = BattleEntities.Action.Attack;

        currentPlayer++;

        if (currentPlayer >= playerBattlers.Count)
        {
            //start the battle
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();

        }
        // IF --> PLAYERS HAVE ACTION :: START BATTLE

        //ELSE :: SHOW BATTLE MENU FOR NEXT MEMBER
    }


    private void AttackAction(BattleEntities currAttacker, BattleEntities currTarget)
    {
      
        int damage = currAttacker.Strength; //can be algri
        currAttacker.BattleVisuals.PlayAttackAnimation();
        currTarget.CurrHealth -= damage;
        currTarget.BattleVisuals.PlayHitAnimation();
        currTarget.UpdateUI();
        bottomText.text = string.Format("{0} attacks {1} for {2} damage", currAttacker.Name, currTarget.Name, damage);
        SaveHealth();
        //get damage --> play animation -> deal damage -> play hit animation -> assign damage by update UI
        //Debug.Log("=== allBattlers ===");

       // for (int i = 0; i < allBattlers.Count; i++)
       // {
       //     Debug.Log("Index " + i + " : " + (allBattlers[i] == null ? "NULL" : allBattlers[i].Name));
       // }

    }
   
    private int GetRandomPartyMember() 
    {
        //use a temp ,list finds, putting all party members on this ebstien list, then select,, return random party member
        List<int> partyMembers = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == true && allBattlers[i].CurrHealth > 0)
            {
                partyMembers.Add(i);

            }
        }
        return partyMembers[Random.Range(0,partyMembers.Count)];
        }
    private int GetRandomEnemy()
    {
        //use a temp ,list finds, putting all party members on this ebstien list, then select,, return random party member
        List<int> enemies = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == false && allBattlers[i].CurrHealth > 0)
            {
                enemies.Add(i);

            }
        }
        return enemies[Random.Range(0,enemies.Count)];
        }
    private void SaveHealth()
    {
        for(int i = 0; i < playerBattlers.Count; i++) 
        {
            partyManager.SaveHealth(i, playerBattlers[i].CurrHealth);
        }

    }
    private void DetermineBattleOrder()
    { 
        allBattlers.Sort((bi1,bi2) => -bi1.Initiative.CompareTo(bi2.Initiative)); //sorts initiave from assending ordfer
        for (int i = 0; i < playerBattlers.Count; i++)
        {
            partyManager.SaveHealth(i, playerBattlers[i].CurrHealth);
        }

    }

    public void SelectRunAction()
    {
        state = BattleState.Selection;
        // recode to reflect fleeing
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        currentPlayerEntity.BattleAction = BattleEntities.Action.Run;
        battleMenu.SetActive(false);

        currentPlayer++;
        if (currentPlayer >= playerBattlers.Count)
        {
            //start the battle
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();

        }
    }


}



[System.Serializable]
    public class BattleEntities
    {

        public enum Action { Attack, Run };
        public Action BattleAction;
        public string Name;
        public int CurrHealth;
        public int MaxHealth;
        public int Level;
        public int Strength;
        public int Initiative;
        public bool IsPlayer;
        public BattleVisuals BattleVisuals;
        public int Target;

        public void SetEntityValues(string name, int currHealth, int maxHealth, int level, int strength, int initiative, bool isPlayer)
        {
            Name = name;
            CurrHealth = currHealth;
            MaxHealth = maxHealth;
            Level = level;
            Strength = strength;
            Initiative = initiative;
            IsPlayer = isPlayer;

        }

        public void SetTarget(int target)

        {
            Target = target;
        }

        public void UpdateUI()
        {
            BattleVisuals.ChangeHealth(CurrHealth);
        }
    }


