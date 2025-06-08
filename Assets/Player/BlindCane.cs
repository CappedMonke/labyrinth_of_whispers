using UnityEngine;

public class BlindCane : MonoBehaviour
{
    [SerializeField] [Range(10, 255)] private int vibrationStrenght = 40;
    private const string CANE_DEVICE_NAME = "BlindCane";
    private Rigidbody rb;
    private BluetoothManager bluetoothManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        bluetoothManager = FindFirstObjectByType<BluetoothManager>();
        if (bluetoothManager != null)
        {
            bluetoothManager.OnDataReceived.AddListener(HandleBluetoothData);
            bluetoothManager.ConnectPairedDeviceWithRetry(CANE_DEVICE_NAME);
        }
    }

    private void HandleBluetoothData(string data)
    {
        string[] values = data.Split(',');

        if (values.Length == 3)
        {
            float x = float.Parse(values[0]);
            float y = float.Parse(values[1]);
            float z = float.Parse(values[2]);

            transform.rotation = Quaternion.Euler(0, x, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        bluetoothManager.WriteData(vibrationStrenght.ToString());
    }
    
    void OnTriggerExit(Collider other)
    {
        bluetoothManager.WriteData("0");
    }
}
