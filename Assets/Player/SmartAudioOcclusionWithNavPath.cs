using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class SmartAudioOcclusionWithNavPath : MonoBehaviour
{
    public Transform player;
    public float checkInterval = 0.5f;
    public float maxAudibleDistance = 25f;
    public float fadeSpeed = 2f;

    private NavMeshPath path;
    private AudioSource audioSource;
    private float targetVolume = 0f;
    private float defaultVolume;
    private float lastCheckTime = 0f;

    private bool pathFound = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        defaultVolume = audioSource.volume;
        path = new NavMeshPath();
    }

    void Update()
    {
        if (player == null) return;

        if (Time.time >= lastCheckTime + checkInterval)
        {
            lastCheckTime = Time.time;
            UpdateAudioPath();
        }

        // Fade volume
        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
    }

    void UpdateAudioPath()
    {
        if (NavMesh.CalculatePath(transform.position, player.position, NavMesh.AllAreas, path) &&
            path.status == NavMeshPathStatus.PathComplete)
        {
            float totalDistance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                totalDistance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }

            if (totalDistance <= maxAudibleDistance)
            {
                float t = 1f - (totalDistance / maxAudibleDistance);
                targetVolume = defaultVolume * t;
                pathFound = true;
                return;
            }
        }

        // Caminho n�o encontrado ou muito longe
        targetVolume = 0f;
        pathFound = false;
    }

    void OnDrawGizmos()
    {
        if (path == null || path.corners.Length < 2) return;

        Gizmos.color = pathFound ? Color.green : Color.red;
        for (int i = 1; i < path.corners.Length; i++)
        {
            Gizmos.DrawLine(path.corners[i - 1], path.corners[i]);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxAudibleDistance);
    }
}
