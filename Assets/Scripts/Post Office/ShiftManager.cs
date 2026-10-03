using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;

// The punch clock. Press E on it to start a shift; packages only spawn while a shift is running.
// The end time is a room property on the shared Photon clock, so every player sees the same countdown.
// Needs a collider on the Interactable layer so the Interactor finds it.
public class ShiftManager : MonoBehaviourPunCallbacks, IInteractable
{
    public static ShiftManager Instance { get; private set; }

    // Fired on every client when a shift starts or ends. Hook pay, cartel jobs, etc. in here later.
    public static event Action ShiftStarted;
    public static event Action ShiftEnded;

    [SerializeField] private float shiftLengthMinutes = 5f;
    [SerializeField] private bool startOnLoad;         // master starts the first shift automatically
    [SerializeField] private TMP_Text clockLabel;       // world text on the clock, or a HUD text
    [SerializeField] private WorldInteractionPrompt worldPrompt;

    private const string ShiftEndKey = "shift.end";

    private double shiftEnd; // on the Photon clock; 0 means no shift has started
    private bool wasActive;
    private bool promptShown;

    // With no ShiftManager in the scene, packages spawn all the time (handy for testing)
    public static bool IsShiftActive => Instance == null || Instance.IsActive;

    public bool IsActive => shiftEnd > Now;
    public float SecondsLeft => Mathf.Max(0f, (float)(shiftEnd - Now));

    private static double Now => Net.Online ? PhotonNetwork.Time : Time.timeAsDouble;

    public string InteractionPrompt => IsActive ? $"Shift ends in {FormatTime(SecondsLeft)}" : "[E] Clock in";

    void Awake() => Instance = this;

    void Start()
    {
        if (Net.TryGetRoomProperty(ShiftEndKey, out double end))
            shiftEnd = end;

        if (startOnLoad && Net.IsAuthority && !IsActive)
            StartShift();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool Interact(Interactor interactor)
    {
        if (IsActive) return false;

        StartShift();
        return true;
    }

    public void StartShift()
    {
        double end = Now + shiftLengthMinutes * 60f;

        if (Net.Online) Net.SetRoomProperty(ShiftEndKey, end); // everyone (including us) applies it in OnRoomPropertiesUpdate
        else shiftEnd = end;
    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed.TryGetValue(ShiftEndKey, out object value) && value is double end)
            shiftEnd = end;
    }

    void Update()
    {
        bool active = IsActive;
        if (active != wasActive)
        {
            wasActive = active;
            if (active)
            {
                Debug.Log($"Shift started: {FormatTime(SecondsLeft)} on the clock.");
                ShiftStarted?.Invoke();
            }
            else
            {
                Debug.Log("Shift over.");
                ShiftEnded?.Invoke();
            }
        }

        if (clockLabel != null)
            clockLabel.text = active ? FormatTime(SecondsLeft) : "Off shift";

        // Keep the countdown in the prompt ticking while the player is standing at the clock
        if (promptShown)
            ShowPrompt();
    }

    // Uses the world prompt if one is assigned, otherwise the on-screen prompt from MainCanvas
    public void ShowPrompt()
    {
        promptShown = true;

        if (worldPrompt != null)
            worldPrompt.Show(InteractionPrompt);
        else if (InteractionPromptUI.Instance != null)
            InteractionPromptUI.Instance.Show(InteractionPrompt);
    }

    public void HidePrompt()
    {
        promptShown = false;

        if (worldPrompt != null)
            worldPrompt.Hide();
        else if (InteractionPromptUI.Instance != null)
            InteractionPromptUI.Instance.Hide();
    }

    static string FormatTime(float seconds)
    {
        int s = Mathf.CeilToInt(seconds);
        return $"{s / 60}:{s % 60:00}";
    }
}
