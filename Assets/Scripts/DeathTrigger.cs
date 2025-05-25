using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    public float cooldownDuration = 5f;
    private float lastTriggerTime = -Mathf.Infinity;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time - lastTriggerTime >= cooldownDuration)
        {
            lastTriggerTime = Time.time;

            var player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.KillPlayer();
            }
        }
    }
}