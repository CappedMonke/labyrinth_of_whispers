using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public GameObject player;
    public float detectionRange = 5f;
    public float retreatRange = 7f;
    public float moveSpeed = 1f;

    private Vector3 startPosition;
    private bool isFollowing = false;
    private AudioSource tensionAudioOnPlayer;

    void Start()
    {
        startPosition = transform.position;

        if (player != null)
        {
            AudioSource[] sources = player.GetComponents<AudioSource>();
            foreach (AudioSource src in sources)
            {
                if (src.clip != null && src.clip.name == "PursuitLoop")
                {
                    tensionAudioOnPlayer = src;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= detectionRange)
        {
            isFollowing = true;
        }
        else if (distance >= retreatRange)
        {
            isFollowing = false;
        }

        if (isFollowing)
        {
            MoveTowards(player.transform.position);
            if (tensionAudioOnPlayer != null && !tensionAudioOnPlayer.isPlaying)
                tensionAudioOnPlayer.Play();
        }
        else
        {
            MoveTowards(startPosition);
            if (tensionAudioOnPlayer != null && tensionAudioOnPlayer.isPlaying)
                tensionAudioOnPlayer.Stop();
        }
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}