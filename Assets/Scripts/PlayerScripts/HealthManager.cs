using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthSystem
{
    public const int MAX_FRAGMENTS = 2;
    public event EventHandler OnDamaged, OnHealed, OnDead;
    private List<Heart> heartList;
    public HealthSystem(int healthAmount)
    {
        heartList = new List<Heart>();
        for (int i = 0; i < healthAmount; i++)
        {
            Heart heart = new Heart(2);
            heartList.Add(heart);
        }

        //heartList[heartList.Count - 1].SetFragments(0);
    }
    public List<Heart> GetHeartList()
    {
        return heartList;
    }
    public void Damage(int damageAmt)
    {
        for(int i=heartList.Count-1;i>=0;i--)//cycle through all hearts
        {
            Heart heart = heartList[i];
            if (damageAmt>heart.GetFragmentAmount())//test heart health versus damage
            {
                damageAmt -= heart.GetFragmentAmount();
                heart.Damage(heart.GetFragmentAmount());
            }
            else
            {
                heart.Damage(damageAmt);
                break;
            }
        }
        if (OnDamaged!=null)
        {
            OnDamaged(this, EventArgs.Empty);
        }
        if (IsDead())
        {
            if (OnDead != null)
            {
                OnDead(this, EventArgs.Empty);
            }
        }
    }
    public void Heal(int healAmt)
    {
        for(int i=0;i < heartList.Count;i++)//cycle through all hearts
        {
            Heart heart = heartList[i];
            int missingFragments = MAX_FRAGMENTS - heart.GetFragmentAmount();
            if (healAmt>missingFragments)
            {
                healAmt -= missingFragments;
                heart.Heal(missingFragments);
            }
            else
            {
                heart.Heal(healAmt);
                    break;
            }
        }
        if (OnHealed != null)
        {
            OnHealed(this, EventArgs.Empty);
        }
    }
    public bool IsDead()
    {
        return heartList[0].GetFragmentAmount() == 0;
    }
    public class Heart
    {
        private int fragments;
        public Heart(int fragments)
        {
            this.fragments = fragments;
        }
        public int GetFragmentAmount()
        {
            return fragments;
        }
        public void SetFragments(int fragments)
        {
            this.fragments = fragments;
        }
        public void Damage(int damageAmt)
        {
            if (damageAmt>=fragments)
            {
                fragments = 0;
            }
            else
            {
                fragments -= damageAmt;
            }
        }
        public void Heal(int healAmt)
        {
            if (fragments+healAmt>MAX_FRAGMENTS)
            {
                fragments = MAX_FRAGMENTS;
            }
            else
            {
                fragments += healAmt;
            }
        }
    }
}
public class HealthManager : MonoBehaviour
{
    public enum UIType { AI, PLAYER, BOSS }
    public UIType UI = UIType.PLAYER;
    public float maxMeter = 100, minMeter = 0;
    public float maxHealth = 100, currentShieldHealth = 0, currentHealth, desperationHealth;
    public int maxPoise = 10, currentPoise = 10;
    public Image HealthFill, DamageFill, BarImage;

    public float showHealthTime = 1, fadeOutTime = .5f, damageShowTime = 1, damageShowSpeed = 1f;
    public bool IsDead = true, shouldSpawnHealth = true, isDesperation, enemyHPVis = false;
    public Color HealthColor, DamageColor, BackColor;
    private Color invisible = new Color(0, 0, 0, 0);
    public int numOfPickups = 3;
    public GameObject currencyPickup, healthPickup;
    
    public Animator effectsAnim, charAnim;
    private float currentMeter = 100, damageShowTimer, healthBarFadeTimer;
    private bool isHealing = false, coroutineStarted = false, healthIsVisible = false, deathCoroutineStarted = false;
    private CharacterObject character;
    public UnityEvent OnDeath;
    public Material dizzyMat;
    public SpriteRenderer rend,effectsRend;
    private AudioManager audioManager;
    //private PlayerRespawner respawner;
    public bool HasShield()
    {
        return currentShieldHealth > 0;
    }
    public bool Dead()
    {
        return HealthVisualManager.healthSystemStatic.IsDead();
    }
    private void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
        character = GetComponent<CharacterObject>();
        //effectsRend = effectsAnim.gameObject.GetComponent<SpriteRenderer>();
        //respawner = GameObject.FindGameObjectWithTag("Respawner").GetComponent<PlayerRespawner>();

        enemyHPVis = GlobalVars.visibleEnemyHealth;
        switch (UI)
        {
            case UIType.PLAYER:
                UpdateMeter();
                SetDefaultMeter();
                lastChance = true;
                OnLastChance += LastChanceEvent;
                break;
            case UIType.AI:
                SetMaxHealth();
                if (enemyHPVis)
                {
                    UpdateHealth();
                    ShowHealth();
                    HideHealth();
                }
                else
                {
                    HideHealth();
                }
                break;
            case UIType.BOSS:
                SetMaxHealth();
                UpdateHealth();
                HideHealth();
                break;
            default:
                break;
        }
    }

    public void SetMaxHealth()
    {
        PoiseReset();
        currentHealth = maxHealth;
        if(rend!=null)
            rend.color = Color.white;
    }

    private void HideHealth()
    {
        BarImage.color = invisible;
        DamageFill.color = invisible;
        HealthFill.color = invisible;

        healthIsVisible = false;
    }

    private void UpdateMeter()
    {
        HealthFill.fillAmount = currentMeter / maxMeter;
    }

    private void UpdateGainMeter()
    {
        DamageFill.fillAmount = currentMeter / maxMeter;
    }
    private void UpdateHealth()
    {
        HealthFill.fillAmount = currentHealth / maxHealth;
    }

    private void UpdateHealthGain()
    {
        DamageFill.fillAmount = currentHealth / maxHealth;
    }

    private void Update()
    {
        switch (UI)
        {
            case UIType.PLAYER:
                if (damageShowTimer < 0)//if the timer is up
                {
                    if (HealthFill.fillAmount < DamageFill.fillAmount && isHealing)//if the bars aren't equal and it's healing
                    {
                        HealthFill.fillAmount += damageShowSpeed * Time.deltaTime;//increase the health bar
                    }
                    else if (HealthFill.fillAmount < DamageFill.fillAmount)//if the health amount is smaller than the damage show
                    {
                        DamageFill.fillAmount -= damageShowSpeed * Time.deltaTime;//decrease the damage bar 
                    }
                    else if (isHealing)//otherwise if the bars are even we're done showing healing so turn the bool off
                        isHealing = false;
                }
                else//DECREASE TIMERS
                {
                    damageShowTimer -= Time.deltaTime;
                    healthBarFadeTimer -= Time.deltaTime;
                }

                if (healthBarFadeTimer < 0)//if we need to start fading health out
                {
                    if (!coroutineStarted && healthIsVisible)
                        StartCoroutine(FadeHealth());
                }
                break;
            case UIType.AI:
                if (enemyHPVis)
                {
                    if (damageShowTimer < 0)//if the timer is up
                    {
                        if (HealthFill.fillAmount < DamageFill.fillAmount && isHealing)//if the bars aren't equal and it's healing
                        {
                            HealthFill.fillAmount += damageShowSpeed * Time.deltaTime;//increase the health bar
                        }
                        else if (HealthFill.fillAmount < DamageFill.fillAmount)//if the health amount is smaller than the damage show
                        {
                            DamageFill.fillAmount -= damageShowSpeed * Time.deltaTime;//decrease the damage bar 
                        }
                        else if (isHealing)//otherwise if the bars are even we're done showing healing so turn the bool off
                            isHealing = false;
                    }
                    else//DECREASE TIMERS
                    {
                        damageShowTimer -= Time.deltaTime;
                        healthBarFadeTimer -= Time.deltaTime;
                    }
                    if (healthBarFadeTimer < 0)//if we need to start fading health out
                    {
                        if (!coroutineStarted && healthIsVisible)
                            StartCoroutine(FadeHealth());
                    }
                }
                break;
            case UIType.BOSS:
                if (damageShowTimer < 0)//if the timer is up
                {
                    if (HealthFill.fillAmount < DamageFill.fillAmount && isHealing)//if the bars aren't equal and it's healing
                    {
                        HealthFill.fillAmount += damageShowSpeed * Time.deltaTime;//increase the health bar
                    }
                    else if (HealthFill.fillAmount < DamageFill.fillAmount)//if the health amount is smaller than the damage show
                    {
                        DamageFill.fillAmount -= damageShowSpeed * Time.deltaTime;//decrease the damage bar 
                    }
                    else if (isHealing)//otherwise if the bars are even we're done showing healing so turn the bool off
                        isHealing = false;
                }
                else//DECREASE TIMERS
                {
                    damageShowTimer -= Time.deltaTime;
                    healthBarFadeTimer -= Time.deltaTime;
                }
                if (healthBarFadeTimer < 0)//if we need to start fading health out
                {
                    if (!coroutineStarted && healthIsVisible)
                        StartCoroutine(FadeHealth());
                }
                break;
            default:
                break;
        }
       

        //if (!this.gameObject.CompareTag("Player"))//if it's not the player, make health bar face camera
        //    BarImage.transform.LookAt(Camera.main.transform);
    }

    private IEnumerator FadeHealth()
    {
        coroutineStarted = true;

        for (float f = fadeOutTime; f > 0; f -= Time.deltaTime)//iterate over time
        {
            Color h = HealthFill.color;//get color
            Color d = DamageFill.color;//gt color
            Color b = BarImage.color;

            h.a = f;//set the alpha to the variable being counted down for
            d.a = f;
            b.a = f;

            HealthFill.color = h;//set that to be our new color
            DamageFill.color = d;
            BarImage.color = b;

            yield return new WaitForEndOfFrame();
        }

        if (healthBarFadeTimer > 0)//if you get hit while it's fading
        {
            ShowHealth();//go back to showing
        }

        if (healthBarFadeTimer <= 0)//as long as that timer isn't running, we've done a successful fade and can set the bool to false
            healthIsVisible = false;

        coroutineStarted = false;
    }


    void SetDefaultMeter()
    {
        currentMeter = 0;
        UpdateMeter();
        UpdateGainMeter();
    }
    public void ChangeMeter(int _val)
    {
        currentMeter += _val;
        currentMeter = Mathf.Clamp(currentMeter, 0f, maxMeter);
        damageShowTimer = damageShowTime;//set the timer back to max when injured happens
        if (_val>0)
        {

            isHealing = true;
            switch (UI)
            {
                case UIType.PLAYER:
                    UpdateGainMeter();
                    break;
                case UIType.AI:
                    break;
                default:
                    break;
            }
        }
        else
        {
            healthBarFadeTimer = showHealthTime;//reset timer for showing health bar here too
            switch (UI)
            {
                case UIType.PLAYER:
                    UpdateMeter();
                    break;
                case UIType.AI:
                    if (enemyHPVis)
                        UpdateHealth();
                    break;
                case UIType.BOSS:
                    UpdateHealth();
                    break;
                default:
                    break;
            }
        }
    }
    public void AddHealth(int amount)
    {
        
        switch (UI)
        {
            case UIType.PLAYER:
                HealthVisualManager.healthSystemStatic.Heal(amount);
                //UpdateFillForHeal();
                break;
            case UIType.AI:
                currentHealth += amount;

                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
                damageShowTimer = damageShowTime;//set the timer back to max when injured happens

                isHealing = true;
                break;
            case UIType.BOSS:
                currentHealth += amount;

                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
                damageShowTimer = damageShowTime;//set the timer back to max when injured happens

                isHealing = true;
                break;
            default:
                break;
        }
        
    }
    private AttackEvent lastHit;
    private float scoreMult = 1;
    public void RemoveHealth(int amount, AttackEvent atk)
    {
        if (atk!=null)
        {
            lastHit = atk;
            scoreMult = atk.killMultiplier;
        }
        switch (UI)
        {
            case UIType.PLAYER:
                if (currentShieldHealth > 0)
                {
                    ShieldDamage(amount);
                }
                else
                {
                    HealthVisualManager.healthSystemStatic.Damage(amount);
                    if (Mission.instance != null)
                        Mission.instance.OnPlayerDamaged(amount);

                    if (Dead())
                    {
                        FinisherDeath();
                    } 
                }
                
                //UpdateFill();
                break;
            case UIType.AI:
                if (currentShieldHealth > 0)
                {
                    ShieldDamage(amount);
                }
                else
                {
                    damageShowTimer = damageShowTime;//set the timer back to max when injured happens
                    healthBarFadeTimer = showHealthTime;//reset timer for showing health bar here too
                    currentHealth -= amount;
                    
                    if (Mission.instance != null)
                        Mission.instance.OnEnemyDamaged(amount);
                }
                if (currentHealth <= desperationHealth&&!isDesperation)
                {
                    DesperationTrigger();
                }
                if (currentHealth <= 0)
                {
                    currentHealth = 0;
                    if (!IsDead)
                        StartCoroutine(DeathEvent(shouldSpawnHealth));
                }
                if (enemyHPVis)
                {
                    UpdateHealth();
                    if (!healthIsVisible)
                    {
                        ShowHealth();
                    }
                    HealthFill.color = Color.Lerp(DamageColor, HealthColor, HealthFill.fillAmount);
                }
                break;
            case UIType.BOSS:
                if (currentShieldHealth > 0)
                {
                    ShieldDamage(amount);
                }
                else
                {
                    damageShowTimer = damageShowTime;//set the timer back to max when injured happens
                    healthBarFadeTimer = showHealthTime;//reset timer for showing health bar here too
                    currentHealth -= amount;
                    if (Mission.instance != null)
                        Mission.instance.OnEnemyDamaged(amount);
                }
                if (currentHealth <= desperationHealth&&!isDesperation)
                {
                    DesperationTrigger();
                }
                if (currentHealth <= 0)
                {
                    currentHealth = 0;
                    if (!IsDead)
                        StartCoroutine(DeathEvent(shouldSpawnHealth));
                }
                UpdateHealth();
                break;
            default:
                break;
        }
        
    }
    private void DesperationTrigger()
    {
        charAnim.SetFloat("aniHealthState", 1);
        isDesperation = true;
        character.OnDesperation();
        PoiseReset();
    }

    public void AddShield(int amount)
    {
        effectsAnim.Play("shieldIntro");
        currentShieldHealth = amount;
        effectsAnim.SetFloat("State", 1);
        effectsRend.color = Color.white;
    }
    public void ShieldDamage(int amount)
    {
        currentShieldHealth -= amount;
        effectsAnim.Play("shieldDamage");
        if (currentShieldHealth<=0)
        {
            effectsAnim.SetFloat("State", 0);
            currentShieldHealth = 0;
            effectsRend.color = Color.clear;
        }
    }
    public void PoiseDamage(int amount)
    {
        currentPoise -= amount;
    }
    public void PoiseReset()
    {
        currentPoise = maxPoise;
    }
    public void ShowHealth()
    {
        BarImage.color = BackColor;
        HealthFill.color = HealthColor;
        DamageFill.color = DamageColor;

        healthIsVisible = true;
    }

    public void FinisherDeath()
    {
        if (!deathCoroutineStarted)
            StartCoroutine(DeathEvent(shouldSpawnHealth));
        //GameEngine.gameEngine.Screenshake();
    }
    public int healthDropRate = 2, effectIndex;
    public bool lastChance = true;
    public event EventHandler OnLastChance;
    public string deathSound = "Enemy/Enemy Explode";
    /// <summary>
    /// waits until the death animation is done and then destroys the character
    /// </summary>
    /// <returns></returns>
    IEnumerator DeathEvent(bool spawnPickup)
    {
        if (spawnPickup)
        {
            for (int i = 0; i < numOfPickups; i++)
            {
                SpawnPickup(currencyPickup);
            }
            int randNum = UnityEngine.Random.Range(0, 10);
            if (randNum<=healthDropRate)
            {
                SpawnPickup(healthPickup);
            }
        }
        rend.color = Color.clear;

        switch (character.controlType)
        {
            case CharacterObject.ControlType.AI:
                DeathStates();

                OnDeath.Invoke();
                character.GlobalPrefab(5);
                character.OnDeath();
                audioManager.PlaySound(deathSound);
                yield return new WaitForFixedUpdate();//get length of death animation        
                if (Mission.instance != null)
                    Mission.instance.OnEnemyKilled(scoreMult);

                EnemySpawn E = GetComponent<EnemySpawn>();
                if (E != null)
                    E.DeSpawn();
                break;
            case CharacterObject.ControlType.BOSS:
                DeathStates();

                OnDeath.Invoke();
                character.GlobalPrefab(5);
                character.OnDeath();
                audioManager.PlaySound(deathSound);
                yield return new WaitForFixedUpdate();//get length of death animation        
                scoreMult *= 100;
                Mission.instance.OnEnemyKilled(scoreMult);

                EnemySpawn B = GetComponent<EnemySpawn>();
                if (B != null)
                    B.DeSpawn();
                break;
            case CharacterObject.ControlType.PLAYER:
                //LAST CHANCE
                if (lastChance)
                {
                    LastChance();
                }
                else
                {
                    DeathStates();
                    yield return new WaitForFixedUpdate();//get length of death animation
                    //ActualDefeat
                    character.QuickChangeForm(4);
                    OnDeath.Invoke();
                    character.OnDeath();
                    //character.StartStateFromScript(36);
                    yield return new WaitForFixedUpdate();//get length of death animation
                    
                    audioManager.PlaySound(deathSound);
                    Mission.instance.EndMission();
                    lastChance = true;
                }
                break;
            default:
                break;
            case CharacterObject.ControlType.OBJECT:
                DeathStates();

                OnDeath.Invoke();
                character.GlobalPrefab(effectIndex);
                character.OnDeath();
                audioManager.PlaySound(deathSound);
                yield return new WaitForFixedUpdate();//get length of death animation        
                EnemySpawn O = GetComponent<EnemySpawn>();
                if (O != null)
                    O.DeSpawn();
                break;
        }
    }

    private void DeathStates()
    {
        deathCoroutineStarted = true;
        IsDead = true;
        charAnim.SetFloat("aniHealthState", 0);
        isDesperation = false;
    }

    void LastChanceEvent(object sender, EventArgs e)
    {
        //Debug.Log("Play Loss event");
    }
    private void LastChance()//transform to human
    {
        character.DOChangeMovelist(4);
        AddHealth(2);
        character.hitStun = 30f;
        character.GlobalPrefab(6);
        OnLastChance?.Invoke(this, EventArgs.Empty);
        lastChance = false;
    }

    private void SpawnPickup(GameObject pickup)
    {
        int randNumX = UnityEngine.Random.Range(-20, 20);
        int randNumY = UnityEngine.Random.Range(15, 35);
        Vector2 offsetDir = new Vector2(randNumX, randNumY);
        GameObject effect = Instantiate(pickup, transform.position, transform.rotation);
        effect.GetComponentInChildren<Rigidbody2D>().AddForce(offsetDir,ForceMode2D.Impulse);
        effect.transform.SetParent(null);
    }
}