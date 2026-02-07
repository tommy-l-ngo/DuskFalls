 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleVisuals : MonoBehaviour
{

    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI levelText;

    private int currHealth;
    private int maxHealth;
    private int level;

    private Animator anim;
    
    private const string LEVEL_ABB = "Lvl: ";

    private const string IS_ATTACK_PARAM = "IsAttack";
    private const string IS_HIT_PARAM = "IsHit";
    private const string IS_DEATH_PARAM = "IsDeath";


    void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
     
    }


    public void SetStartingValues(int currHealth, int maxHealth, int level)
    {
        this.currHealth = currHealth;
        this.maxHealth = maxHealth;
        this.level = level;
        levelText.text = "Lvl: " + this.level.ToString();
        UpdateHealthBar();
    }

    public void ChangeHealth(int currHealth)
    {
        this.currHealth = currHealth; // if heal;th is 0 _> play death anim -> destroy battle view

        if (currHealth <= 0)
        {
            PlayDeathAnimation();
        }
        UpdateHealthBar();
    }
    public void UpdateHealthBar()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currHealth;
    }

    public void PlayAttackAnimation()
    {
        anim.SetTrigger(IS_ATTACK_PARAM);
        
    }
    public void PlayHitAnimation()
    {
        anim.SetTrigger(IS_HIT_PARAM);
        
    }
    public void PlayDeathAnimation()
    {
        anim.SetTrigger(IS_DEATH_PARAM);
    }
}
