using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        gameObject.AddComponent<PlayerAndroid>();
#else
        gameObject.AddComponent<PlayerPC>();
#endif
    }
}
