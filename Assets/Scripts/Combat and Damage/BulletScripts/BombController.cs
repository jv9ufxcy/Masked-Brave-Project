using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    public CharacterObject character;
    private AudioManager audioManager;
    [SerializeField]private string contactSound = "Player/Bombardier Form/Grenade Land", explosionSound = "Player/Bombardier Form/Genade Explo";
    private void Awake()
    {
        audioManager = AudioManager.instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (hitbox == null)
            hitbox = GetComponentInChildren<BulletHitbox>();
        hitbox.character = character;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!PauseManager.IsGamePaused && !DialogueManager.instance.isDialogueActive)
        {
            if (GameEngine.hitStop <= 0)
            {
                if (active)
                {
                    currentStateTime++;
                    hitbox.projectileIndex = currentState;
                    hitbox.atkIndex = currentAttackIndex;
                    UpdateStateAttacks();
                    if (currentStateTime > GameEngine.coreData.characterStates[currentState].length)
                    {
                        EndState();
                    }
                }
            }
        }
    }
    public void StartState()
    {
        active = true;
        audioManager.PlaySound(contactSound);
    }
    public void EndState()
    {
        active = false;
        currentStateTime = 0;
        Destroy(gameObject);
    }
    private void PlayExplosionSound()
    {
        audioManager.PlaySound(explosionSound);
    }
    [Header("CurrentState")]
    bool active;
    public int currentState;
    public float currentStateTime;

    [Header("CurrentAttack")]
    public float hitActive;
    public int currentAttackIndex;
    public BulletHitbox hitbox;
    void UpdateStateAttacks()
    {
        int _cur = 0;
        foreach (AttackEvent _atk in GameEngine.coreData.characterStates[currentState].attacks)
        {
            if (currentStateTime == _atk.start)
            {
                hitbox.RestoreGetHitBools();
                hitActive = _atk.length;
                hitbox.transform.localScale = _atk.hitBoxScale;
                hitbox.transform.localPosition = _atk.hitBoxPos;
                currentAttackIndex = _cur;
                PlayExplosionSound();
            }
            if (currentStateTime == _atk.start + _atk.length)
            {
                hitActive = 0;
            }
            _cur++;
        }
    }

}
