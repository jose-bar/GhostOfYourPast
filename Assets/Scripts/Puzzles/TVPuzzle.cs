using UnityEngine;

public class TVPuzzle : MonoBehaviour, IResettable
{
    [Header("Sprites")]
    public Sprite tvOffSprite;
    public Sprite tvOnSprite;

    [Header("Timing")]
    public float windowDuration = 1.5f;

    [Header("Also enable these objects while TV is on")]
    public GameObject[] extraObjects;        // drag your light-effect here

    bool windowActive = false;
    float windowEndTime;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    /* -------------------------------------------------- called by REMOTE */
    public void EnableWindow()
    {
        windowActive = true;
        windowEndTime = Time.time + windowDuration;

        if (sr != null && tvOnSprite != null) sr.sprite = tvOnSprite;

        foreach (GameObject go in extraObjects)          // turn extra light on
            if (go != null) go.SetActive(true);

        CancelInvoke(nameof(DisableWindow));
        Invoke(nameof(DisableWindow), windowDuration);
    }

    /* -------------------------------------------------- called by TV itself */
    public void AttemptInteract()
    {
        if (!windowActive) return;               // too late – do nothing

        // succeed, mark puzzle solved
        GameManager.Instance.CompletePuzzle();

        DisableWindow();                         // turn TV & light off instantly
    }

    /* -------------------------------------------------- internal */
    void DisableWindow()
    {
        windowActive = false;

        if (sr != null && tvOffSprite != null) sr.sprite = tvOffSprite;

        foreach (GameObject go in extraObjects)          // turn extra light off
            if (go != null) go.SetActive(false);
    }

    /* -------------------------------------------------- Day reset */
    public void ResetState()
    {
        windowActive = false;
        CancelInvoke(nameof(DisableWindow));

        if (sr != null && tvOffSprite != null) sr.sprite = tvOffSprite;

        foreach (GameObject go in extraObjects)
            if (go != null) go.SetActive(false);
    }

    void OnEnable() { DayResetManager.Instance?.Register(this); }
    void OnDestroy() { DayResetManager.Instance?.Unregister(this); }
}
