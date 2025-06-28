using UnityEngine;

public class MultiSoundOcclusion : MonoBehaviour
{
    public string soundTag = "Sound3D";
    public LayerMask wallLayer;

    // Frequências
    private float clearFreq = 22000f; 
    private float muffledFreq = 500f; 

    void Update()
    {
        GameObject[] sounds = GameObject.FindGameObjectsWithTag(soundTag);

        foreach (GameObject soundObj in sounds)
        {
            AudioSource source = soundObj.GetComponent<AudioSource>();
            AudioLowPassFilter filter = soundObj.GetComponent<AudioLowPassFilter>();

            if (source == null || filter == null) continue;

            Vector3 direction = soundObj.transform.position - transform.position;
            float distance = direction.magnitude;

            if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, distance, wallLayer))
            {
                source.volume = 1f;
                filter.cutoffFrequency = muffledFreq; 
            }
            else
            {
                source.volume = 1f;
                filter.cutoffFrequency = clearFreq; 
            }
        }
    }
}
