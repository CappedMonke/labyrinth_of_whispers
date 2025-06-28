using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class SoundZone : MonoBehaviour
{
    public AudioMixer audioMixer;
    public string exposedParam = "VoiceVolume";
    public float fadeDuration = 1.5f;

    private Coroutine fadeCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entrou — som aumentando");
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeMixerVolume(11f, -40f));
        }
    }


    private IEnumerator FadeMixerVolume(float from, float to)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float current = Mathf.Lerp(from, to, time / fadeDuration);
            audioMixer.SetFloat(exposedParam, current);
            yield return null;
        }

        audioMixer.SetFloat(exposedParam, to);
    }
}
