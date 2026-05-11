using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Unity.VisualScripting;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviourPunCallbacks
{
    public static Character Instance {get; private set;}
    
    [Header("PlayerObject")]
    public Transform firstPersonAnchor;
    public Transform cameraPivot;
    public CinemachineVirtualCamera playerVCam;
    public CinemachineVirtualCamera vehicleVCam;
    [HideInInspector] public CinemachineBrain cinemachineBrain;

    [Header("UI")]
    public GameObject mainCanvasPrefab;
    public GameObject inventoryUI;
    private bool inventoryOpen = false;
    

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
    public float gravityMultiplier = 1;
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
    public LootingState looting;

    [HideInInspector]
    public float gravityValue = -9.81f;
    [HideInInspector]
    public float normalColliderHeight;
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
    

    private bool IsLocalPlayer => !PhotonNetwork.IsConnected || photonView.IsMine;

    private void Awake()
    {
        if (IsLocalPlayer)
        {
            GetComponent<PlayerInput>().enabled = true;
            this.enabled = true;

            if (inventoryUI != null)
                inventoryUI.SetActive(false);
        }
        else
        {
            GetComponent<PlayerInput>().enabled = false;

            if (inventoryUI != null)
                Destroy(inventoryUI);
        }

        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        groundSensor = GetComponent<GroundSensor>();
        cinemachineBrain = GetComponentInChildren<CinemachineBrain>();

        if (cinemachineBrain != null)
            cinemachineBrain.enabled = false;

        movementSM = new StateMachine();
        standing = new StandingState(this, movementSM);
        jumping = new JumpingState(this, movementSM);
        sprinting = new SprintingState(this, movementSM);
        sprintJumping = new SprintJumpState(this, movementSM);
        combatting = new CombatState(this, movementSM);
        attacking = new AttackingState(this, movementSM);
        driving = new DrivingState(this, movementSM);
        looting = new LootingState(this, movementSM);
        movementSM.Initialize(standing);

        playerSpeed = playerBaseSpeed;
        gravityValue *= gravityMultiplier;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Start()
    {
        if (IsLocalPlayer)
        {
            if (mainCanvasPrefab == null)
            {
                Debug.LogWarning("mainCanvasPrefab is not assigned on Character.");
                return;
            }

            GameObject canvas = Instantiate(mainCanvasPrefab);

            InventoryUI inventoryUIScript = canvas.GetComponentInChildren<InventoryUI>(true);

            if (inventoryUIScript != null)
            {
                inventoryUI = inventoryUIScript.gameObject;
                inventoryUI.SetActive(false);

                Inventory playerInventory = GetComponent<Inventory>();
                if (playerInventory != null)
                    inventoryUIScript.SetInventory(playerInventory);
                else
                    Debug.LogWarning("No Inventory component found on Player.");
            }
            else
            {
                Debug.LogWarning("No InventoryUI component found in MainCanvas prefab.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer)
            return;

        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }

        if (!inventoryOpen)
        {
            movementSM.currentState.HandleInput();
            movementSM.currentState.LogicUpdate();
        }
    }

    private void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(inventoryOpen);
        }

        if (inventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer) return;
        movementSM.currentState.PhysicsUpdate();
    }

    private void LateUpdate() 
    {
        // movementSM.currentState.PhysicsUpdate();
    }

     
}
