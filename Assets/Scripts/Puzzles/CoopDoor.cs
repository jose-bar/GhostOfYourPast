using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractableObject))]
public class CooperativeDoor : MonoBehaviour, IResettable
{
    [Header("Push-Together Settings")]
    [Tooltip("Seconds after the first push during which the second push must occur")]
    public float syncWindow = 2f;

    [Tooltip("Door collider that will be turned into a trigger when opened")]
    public Collider2D doorCollider;

    [Tooltip("Sprite shown when the door is open (optional)")]
    public Sprite openedSprite;

    [Header("Events")]
    public UnityEvent OnBothPushed;          // optional: sound, animation …

    // ────────────────── private state ──────────────────
    bool playerPushed = false;
    bool shadowPushed = false;
    float firstPushTime = 0f;
    bool doorOpened = false;

    InteractableObject io;
    SpriteRenderer sr;
    Sprite closedSprite;

    void Awake()
    {
        io = GetComponent<InteractableObject>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (io == null)
            Debug.LogError("CooperativeDoor needs an InteractableObject");

        // cache sprite so we can restore on ResetState
        closedSprite = sr ? sr.sprite : null;

        //  auto-register our listener (so designer does not need to wire events)
        io.OnInteract.AddListener(PlayerPress);
        io.OnShadowInteract.AddListener(ShadowPress);
    }

    void Start()
    {
        DayResetManager.Instance.Register(this);
    }

    // ────────────────── presses ──────────────────
    void PlayerPress() { RegisterPress(true); }
    void ShadowPress() { RegisterPress(false); }

    void RegisterPress(bool fromPlayer)
    {
        if (doorOpened) return;                          // already solved

        // first press of the pair?
        if (!playerPushed && !shadowPushed)
            firstPushTime = Time.time;

        if (fromPlayer) playerPushed = true;
        else shadowPushed = true;

        // second press arrived in time?
        if (playerPushed && shadowPushed &&
            Time.time - firstPushTime <= syncWindow)
        {
            OpenDoor();
        }
        else
        {
            // still waiting – start / keep countdown via Update()
        }
    }

    void Update()
    {
        if (doorOpened) return;

        // reset if time window expired
        if ((playerPushed || shadowPushed) &&
            Time.time - firstPushTime > syncWindow)
        {
            playerPushed = shadowPushed = false;
        }
    }

    // ────────────────── success ──────────────────
    void OpenDoor()
    {
        doorOpened = true;

        // visual
        if (sr != null && openedSprite != null) sr.sprite = openedSprite;

        // allow passage
        if (doorCollider != null) doorCollider.isTrigger = true;

        // mark puzzle solved so bed trigger can advance the day
        GameManager.Instance.CompletePuzzle();

        OnBothPushed.Invoke();          // optional SFX / particles
    }

    // ──────────────────  IResettable  ──────────────────
    public void ResetState()
    {
        playerPushed = shadowPushed = false;
        doorOpened = false;

        if (doorCollider != null) doorCollider.isTrigger = false;
        if (sr != null && closedSprite != null) sr.sprite = closedSprite;
    }

    void OnDestroy() { DayResetManager.Instance.Unregister(this); }
}
