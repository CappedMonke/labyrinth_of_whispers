using UnityEngine;

public class Cane : MonoBehaviour
{
    private const string CANE_DEVICE_NAME = "BlindCane";
    private BluetoothManager bluetoothManager;

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
}
