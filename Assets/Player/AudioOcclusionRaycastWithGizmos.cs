using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class AudioOcclusionRaycastWithGizmos : MonoBehaviour
{
    public float checkInterval = 0.5f;
    public LayerMask wallMask;

    private Transform player;
    private AudioSource audioSource;
    private bool isOccluded = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Please tag your player as 'Player'.");
            enabled = false;
            return;
        }

        InvokeRepeating(nameof(CheckOcclusion), 0f, checkInterval);
    }

    private void CheckOcclusion()
    {
        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, distance, wallMask))
        {
            if (!isOccluded)
            {
                isOccluded = true;
                audioSource.mute = true;
            }
        }
        else
        {
            if (isOccluded)
            {
                isOccluded = false;
                audioSource.mute = false;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = isOccluded ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, player.position);
    }

#endif    
}
