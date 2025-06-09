using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{
    [Header("Audio Settings")]
    private AudioSource walkAudioSource;
    [SerializeField] private List<AudioClip> normalWalkSounds;
    [SerializeField] private List<AudioClip> silentWalkSounds;
    [SerializeField] private float silentWalkThreshold = 0.3f;
    [SerializeField] private Vector2Int stepPause = new(200, 800);
    private bool canPlayStepSound = true;

    void Awake()
    {
        walkAudioSource = GetComponent<AudioSource>();
        if (walkAudioSource == null)
        {
            Debug.LogError("AudioSource component not found on Player.");
        }
    }

    public void PlayWalkSound(float speed, float minSpeed, float maxSpeed)
    {
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        float speedPercentage = (speed - minSpeed) / (maxSpeed - minSpeed);

        if (speedPercentage < silentWalkThreshold)
        {
            TryPlayWalkSound(silentWalkSounds, walkAudioSource, speedPercentage);
        }
        else
        {
            TryPlayWalkSound(normalWalkSounds, walkAudioSource, speedPercentage);
        }
    }

    private void TryPlayWalkSound(List<AudioClip> soundList, AudioSource audioSource, float speedPercentage)
    {
        if (!canPlayStepSound || audioSource.isPlaying || soundList.Count == 0)
        {
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, soundList.Count);
        audioSource.clip = soundList[randomIndex];
        audioSource.Play();
        StartCoroutine(StepPauseCoroutine(speedPercentage));
    }

    private IEnumerator StepPauseCoroutine(float speedPercentage)
    {
        float scaledPause = Mathf.Lerp(stepPause.y, stepPause.x, speedPercentage) / 1000f;
        canPlayStepSound = false;
        yield return new WaitForSeconds(scaledPause);
        canPlayStepSound = true;
    }
}
