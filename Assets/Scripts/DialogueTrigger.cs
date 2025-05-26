using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Yarn Settings")]
    public string yarnNodeName = "Start";
    public float pauseDuration = 0f;

    [Header("Interaction Settings")]
    public bool onRepeat = false;
    public bool isKeyTrigger = false;
    public KeyCode interactKey = KeyCode.E;

    [Header("Day Restrictions")]
    public int minDay = 1;
    public int maxDay = 99;

    private bool triggered = false;
    private bool playerInRange = false;
    private PlayerController2D playerController;
    private DialogueRunner dialogueRunner;

    void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();
    }

    void Update()
    {
        if (isKeyTrigger && playerInRange && !triggered && Input.GetKeyDown(interactKey))
        {
            TriggerDialogue();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerController = other.GetComponent<PlayerController2D>();
        if (isKeyTrigger)
        {
            playerInRange = true;
        }
        else
        {
            TriggerDialogue();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (isKeyTrigger)
        {
            playerInRange = false;
        }
    }

    public void TriggerDialogue()
    {
        if (triggered) return;
        if (dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        int currentDay = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;
        if (currentDay < minDay || currentDay > maxDay)
        {
            Debug.Log($"🛑 Dialogue '{yarnNodeName}' skipped: only triggers between days {minDay} and {maxDay} (today = {currentDay})");
            return;
        }

        triggered = true;

        if (playerController != null)
        {
            playerController.enabled = false;

            // Reset animator to idle
            Animator animator = playerController.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }

        dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        dialogueRunner.StartDialogue(yarnNodeName);
    }

    private void OnDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        StartCoroutine(ResumeAfterPause());
    }

    private IEnumerator ResumeAfterPause()
    {
        yield return new WaitForSeconds(pauseDuration);

        if (playerController != null)
            playerController.enabled = true;

        if (onRepeat)
            triggered = false;
    }
}
