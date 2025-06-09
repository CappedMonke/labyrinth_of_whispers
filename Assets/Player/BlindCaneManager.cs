using UnityEngine;

public class BlindCaneManager : MonoBehaviour
{
    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        gameObject.AddComponent<BlindCaneAndroid>();
#else
        gameObject.AddComponent<BlindCanePC>();
#endif
    }
}
