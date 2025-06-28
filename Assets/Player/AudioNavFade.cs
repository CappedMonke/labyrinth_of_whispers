using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using System.Collections;

public class AudioNavFade : MonoBehaviour
{
    public Transform player;
    public AudioMixer audioMixer;
    public string exposedParam = "VoiceVolume";
    public float fadeDuration = 1f;
    public float checkInterval = 0.5f;

    private NavMeshAgent agent;
    private Coroutine fadeCoroutine;
    private bool isPathValid = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;

        StartCoroutine(CheckPathRoutine());
    }

    IEnumerator CheckPathRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(player.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                if (!isPathValid)
                {
                    isPathValid = true;
                    StartFade(0f); // volume normal
                }
            }
            else
            {
                if (isPathValid)
                {
                    isPathValid = false;
                    StartFade(-40f); // som abafado ou mudo
                }
            }

            yield return wait;
        }
    }

    void StartFade(float targetVolume)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeMixerVolume(targetVolume));
    }

    IEnumerator FadeMixerVolume(float target)
    {
        audioMixer.GetFloat(exposedParam, out float current);
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float newVolume = Mathf.Lerp(current, target, time / fadeDuration);
            audioMixer.SetFloat(exposedParam, newVolume);
            yield return null;
        }

        audioMixer.SetFloat(exposedParam, target);
    }
}
