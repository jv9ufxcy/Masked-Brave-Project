using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
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
    public enum UIType { AI, PLAYER }
    public UIType UI = UIType.PLAYER;
    public float maxMeter = 100, minMeter = 0;
    public float maxHealth = 100, minHealth = 0, currentHealth;
    public Image HealthFill, DamageFill, BarImage;

    public float showHealthTime = 1, fadeOutTime = .5f, damageShowTime = 1, damageShowSpeed = 1f;
    public bool IsDead = false, shouldSpawnHealth=true;
    public Color HealthColor, DamageColor, BackColor;
    private Color invisible = new Color(0, 0, 0, 0);
    public int numOfPickups = 3;
    public GameObject currencyPickup, healthPickup;

    private float currentMeter = 100, damageShowTimer, healthBarFadeTimer;
    private bool isHealing = false, coroutineStarted = false, healthIsVisible = false, deathCoroutineStarted = false;
    private CharacterObject character;
    public Material DizzyMat;
    private AudioManager audioManager;
    //private PlayerRespawner respawner;

    private void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
        character = GetComponent<CharacterObject>();
        //respawner = GameObject.FindGameObjectWithTag("Respawner").GetComponent<PlayerRespawner>();
        

        switch (UI)
        {
            case UIType.PLAYER:
                UpdateFill();
                SetDefaultMeter();
                break;
            case UIType.AI:
                SetMaxHealth();
                break;
            default:
                break;
        }
        

        //if (ui == UIType.enemy)
        //{
        //    HideHealth();
        //}
    }

    public void SetMaxHealth()
    {
        currentHealth = maxHealth;
    }

    private void HideHealth()
    {
        BarImage.color = invisible;
        DamageFill.color = invisible;
        HealthFill.color = invisible;

        healthIsVisible = false;

    }

    private void UpdateFill()
    {
        HealthFill.fillAmount = currentMeter / 100;
    }

    private void UpdateFillForHeal()
    {
        DamageFill.fillAmount = currentMeter / 100f;
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

        //if (ui == UIType.enemy)
        //{
        //    if (healthBarFadeTimer > 0)//if you get hit while it's fading
        //    {
        //        ShowHealth();//go back to showing
        //    }

        //    if (healthBarFadeTimer <= 0)//as long as that timer isn't running, we've done a successful fade and can set the bool to false
        //        healthIsVisible = false;
        //}

        coroutineStarted = false;
    }


    void SetDefaultMeter()
    {
        currentMeter = 0;
        UpdateFill();
        UpdateFillForHeal();
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
                    UpdateFillForHeal();
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
                    UpdateFill();
                    break;
                case UIType.AI:
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
            default:
                break;
        }
        
    }

    public void RemoveHealth(int amount)
    {
        
        switch (UI)
        {
            case UIType.PLAYER:
                HealthVisualManager.healthSystemStatic.Damage(amount);
                //UpdateFill();
                break;
            case UIType.AI:
                damageShowTimer = damageShowTime;//set the timer back to max when injured happens
                healthBarFadeTimer = showHealthTime;//reset timer for showing health bar here too


                currentHealth -= amount;

                if (currentHealth <= minHealth)
                {
                    currentHealth = minHealth;
                    if (!IsDead)
                        StartCoroutine(DeathEvent(shouldSpawnHealth));
                }
                break;
            default:
                break;
        }
        
    }

    private void ShowHealth()
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

    /// <summary>
    /// waits until the death animation is done and then destroys the character
    /// </summary>
    /// <returns></returns>
    IEnumerator DeathEvent(bool spawnPickup)
    {
        deathCoroutineStarted = true;
        IsDead = true;

        if (spawnPickup)
        {
            for (int i = 0; i < numOfPickups; i++)
            {
                SpawnPickup(currencyPickup);
            }
            int randNum = UnityEngine.Random.Range(0, 10);
            if (randNum>=8)
            {
                SpawnPickup(healthPickup);
            }
        }
        audioManager.PlaySound("Death");
        character.GlobalPrefab(1);
        yield return new WaitForSeconds(0f);//get length of death animation        

        switch (character.controlType)
        {
            case CharacterObject.ControlType.AI:
                
                gameObject.SetActive(false);
                break;
            case CharacterObject.ControlType.PLAYER:
                //RESPAWN HERE
                //respawner.RespawnPlayer();
                break;
            default:
                break;
        }

    }

    private void SpawnPickup(GameObject pickup)
    {
        int randNumX = UnityEngine.Random.Range(-20, 20);
        int randNumY = UnityEngine.Random.Range(15, 35);
        Vector2 offsetDir = new Vector2(randNumX, randNumY);
        GameObject effect = Instantiate(pickup, transform.position, transform.rotation);
        effect.GetComponentInChildren<Rigidbody2D>().AddForce(offsetDir,ForceMode2D.Impulse);
    }
}