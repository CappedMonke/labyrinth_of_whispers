using UnityEngine;

public class BlindCanePlatformManager : MonoBehaviour
{
    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        GetComponent<BlindCanePC>().enabled = false;
        GetComponent<BlindCaneAndroid>().enabled = true;
#else
        GetComponent<BlindCanePC>().enabled = true;
        GetComponent<BlindCaneAndroid>().enabled = false;
#endif
    }
}
