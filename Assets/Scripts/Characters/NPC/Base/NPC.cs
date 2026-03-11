using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, IDamageable, INPCMoveable
{
    [field: SerializeField] public float MaxHealth {get; set;} = 100f;
    public float currentHealth { get; set;}
    public Rigidbody RB { get; set; }
    public bool IsFacingRight { get; set;}

    public NPCStateMachine StateMachine {get; set;}
    public NPCIdleState IdleState {get; set;}
    public NPCChaseState ChaseState {get; set;}
    public NPCAttackState AttackState {get; set;}

    #region Idle Variables
    public float RandomMovementRange = 5f;
    public float RandomMovementSpeed = 1f;
    #endregion


    private void Awake() 
    {
        StateMachine = new NPCStateMachine(this, StateMachine);

        IdleState = new NPCIdleState(this, StateMachine);
        ChaseState = new NPCChaseState(this, StateMachine);
        AttackState = new NPCAttackState(this, StateMachine);
    }

    void Start()
    {
        currentHealth = MaxHealth;

        RB = GetComponent<Rigidbody>();

        StateMachine.Initialize(IdleState);
        
    }

    void Update()
    {
        StateMachine.CurrentNPCState.FrameUpdate();
    }

    void FixedUpdate()
    {
        StateMachine.CurrentNPCState.PhysicsUpdate();
    }

    public void Damage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void MoveEnemy(Vector2 velocity)
    {
        RB.velocity = velocity;
        CheckForLeftOrRightFacing(velocity);
    }

    public void CheckForLeftOrRightFacing(Vector2 velocity)
    {
        if (IsFacingRight && velocity.x < 0f)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
        }

        else if (!IsFacingRight && velocity.x > 0f)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            IsFacingRight = !IsFacingRight;
        }
    }

    private void AniamtionTriggerEvent(AnimationTriggerType triggerType)
    {
        StateMachine.CurrentNPCState.AnimationTriggerEvent(triggerType);
    }

    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootstepSound
    }
}
