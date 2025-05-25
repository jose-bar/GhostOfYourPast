using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Yarn Settings")]
    public string yarnNodeName = "Start"; // Default node
    public float pauseDuration = 0f;      // Optional delay after dialogue

    private bool triggered = false;
    private PlayerController2D playerController;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        playerController = other.GetComponent<PlayerController2D>();

        if (playerController != null)
            playerController.enabled = false; // Pause movement

        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
        {
            runner.onDialogueComplete.AddListener(OnDialogueComplete);
            runner.StartDialogue(yarnNodeName);
        }
    }

    private void OnDialogueComplete()
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
            runner.onDialogueComplete.RemoveListener(OnDialogueComplete);

        StartCoroutine(ResumeAfterPause());
    }

    private IEnumerator ResumeAfterPause()
    {
        yield return new WaitForSeconds(pauseDuration);
        if (playerController != null)
            playerController.enabled = true;
    }
}
