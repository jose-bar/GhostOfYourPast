// using UnityEngine;

// [RequireComponent(typeof(Collider2D))]
// public class ShadowTouchFail : MonoBehaviour
// {
//     [Tooltip("Seconds after a new day starts before touching the shadow kills you")]
//     public float gracePeriod = 1.5f;

//     void OnTriggerEnter2D(Collider2D col)
//     {
//         if (!col.CompareTag("Player")) return;
//         if (Time.time < GameManager.Instance.dayStartRealtime + gracePeriod) return;

//         GameManager.Instance.EndDayFailure();            // <- restart, no advance
//     }
// }