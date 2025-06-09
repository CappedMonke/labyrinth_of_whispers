using UnityEngine;

public class BlindCaneAndroid : MonoBehaviour
{
    [SerializeField] string CANE_DEVICE_NAME = "BlindCane";
    [SerializeField][Range(10, 255)] int vibrationStrenght = 40;

    BluetoothManager bluetoothManager;

    private float initialRotation = 0f;
    private bool isCalibrated = false;

    private void Start()
    {
        bluetoothManager = FindFirstObjectByType<BluetoothManager>();
        if (bluetoothManager != null)
        {
            bluetoothManager.OnDataReceived.AddListener(HandleBluetoothData);
            bluetoothManager.ConnectPairedDeviceWithRetry(CANE_DEVICE_NAME);
        }
        else
        {
            Debug.LogError("BluetoothManager not found in the scene.");
        }
    }

    private void HandleBluetoothData(string data)
    {
        if (data == "calibrate")
        {
            isCalibrated = false;
            return;
        }

        string[] values = data.Split(',');
        if (values.Length == 3)
        {
            float x = float.Parse(values[0]);
            float y = float.Parse(values[1]);
            float z = float.Parse(values[2]);

            float currentRotation = x;
            if (!isCalibrated)
            {
                initialRotation = currentRotation;
                isCalibrated = true;
            }

            float adjustedRotation = currentRotation - initialRotation;
            transform.localRotation = Quaternion.Euler(0, adjustedRotation, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (bluetoothManager != null)
        {
            bluetoothManager.WriteData(vibrationStrenght.ToString());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (bluetoothManager != null)
        {
            bluetoothManager.WriteData("0");
        }
    }
}
