using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }
    public GameObject blackScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ToggleBlackScreen()
    {
        if (blackScreen != null)
        {
            blackScreen.SetActive(!blackScreen.activeSelf);
        }
    }
}
