using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviourPunCallbacks
{
    public static Character Instance {get; private set;}
    
    [Header("PlayerObject")]
    public Transform orientation;
    public Transform cameraPivot;
    public Canvas inventoryCanvas;
    public event Action<Character> OnCharacterInitialized;

    [Header("PlayerStats")]
    public float strength;
    public float health;
    public float defense;
    public float vitality;
    public float intellect;
    public float wisdom;
    public float dexterity;
    public float charisma;
    public float speed;

    

    [Header("Controls")]
    public float playerSpeed;
    public float playerBaseSpeed = 5.0f;
    public float crouchSpeed = 2.0f;
    public float sprintSpeed = 7.0f;
    public float jumpHeight = 0.8f;
    public float gravityMultiplier = 2;
    public float rotationSpeed = 5f;
    public float crouchColliderHeight = 1.35f;
    public float lookSensitivity = 0.1f;
    public float pitchMin = -85f;
    public float pitchMax = 85f;
    public float yaw;
    public float pitch;
    


    [Header("Animation Smoothing")]
    [Range(0,1)]
    public float speedDampTime = 0.1f;
    [Range(0,1)]
    public float velocityDampTime = 0.9f;
    [Range(0,1)]
    public float rotationDampTime = 0.2f;
    [Range(0,1)]
    public float airControl = 0.5f;

    public StateMachine movementSM;
    public StandingState standing;
    public JumpingState jumping;
    public SprintingState sprinting;
    public SprintJumpState sprintJumping;
    public CombatState combatting;
    public AttackingState attacking;
    public DrivingState driving;

    [HideInInspector]
    public float gravityValue = -9.81f;
    [HideInInspector]
    public float normalColliderHeight;
    [HideInInspector]
    public CharacterController controller;
    [HideInInspector]
    public PlayerInput playerInput;
    [HideInInspector]
    public Transform cameraTransform;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public Vector3 playerVelocity;
    [HideInInspector]
    public GroundSensor groundSensor;

    public Vector3 launchHorizontalVelocity;
    

    private void Awake() 
    {

        if (photonView.IsMine)
        {
            GetComponent<PlayerInput>().enabled = true;
            this.enabled = true;
        }
        else
        {
            GetComponent<PlayerInput>().enabled = false;
        }

        
        cameraTransform = GetComponentInChildren<Camera>().transform;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        // cameraTransform = Camera.main.transform;

        cameraPivot = GetComponentInChildren<Camera>().transform;
        orientation = this.transform;

        groundSensor = GetComponent<GroundSensor>();
        
        

        movementSM = new StateMachine();
        standing = new StandingState(this, movementSM);
        jumping = new JumpingState(this, movementSM);
        sprinting = new SprintingState(this, movementSM);
        sprintJumping = new SprintJumpState(this, movementSM);
        combatting = new CombatState(this, movementSM);
        attacking = new AttackingState(this, movementSM);
        driving = new DrivingState(this, movementSM);
        movementSM.Initialize(standing);

        playerSpeed = playerBaseSpeed;

        // normalColliderHeight = controller.height;
        gravityValue *= gravityMultiplier;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }


    void Start()
    {
        
        
        
    }

    // Update is called once per frame
    void Update()
    {      
        movementSM.currentState.HandleInput();
        movementSM.currentState.LogicUpdate();
    }

    void FixedUpdate()
    {
        movementSM.currentState.PhysicsUpdate();
    }

     
}
