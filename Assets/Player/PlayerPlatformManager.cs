using UnityEngine;

public class PlayerPlatformManager : MonoBehaviour
{
    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        gameObject.GetComponent<PlayerPC>().enabled = false;
        gameObject.GetComponent<PlayerAndroid>().enabled = true;
#else
        gameObject.GetComponent<PlayerPC>().enabled = true;;
        gameObject.GetComponent<PlayerAndroid>().enabled = false;;
#endif
    }
}
