using UnityEngine;

public class CalendarDisplay : MonoBehaviour, IResettable
{
    public Sprite[] daySprites;        // index 0 = Day-1, etc.
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        DayResetManager.Instance.Register(this);
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        int d = GameManager.Instance.currentDay;
        if (d <= 0 || d > daySprites.Length) return;
        spriteRenderer.sprite = daySprites[d - 1];
    }

    public void ResetState() => UpdateSprite();
}
