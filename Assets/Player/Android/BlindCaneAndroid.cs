using System;
using System.Collections;
using UnityEngine;

public class BlindCaneAndroid : MonoBehaviour
{
    [SerializeField] string CANE_DEVICE_NAME = "BlindCane";
    [SerializeField][Range(10, 255)] int vibrationStrength = 30;
    [SerializeField] private uint onConnectionVibrateIntervals = 3;
    [SerializeField][Range(10, 255)] private uint onConnectionVibrationStrength = 50;
    [SerializeField] private float onConnectionVibrateTime = 0.2f;
    [SerializeField] private float onConnectionVibrateBreakTime = 0.05f;

    BluetoothManager bluetoothManager;

    private float initialRotation = 0f;
    private bool isCalibrated = false;

    private void Start()
    {
        bluetoothManager = FindFirstObjectByType<BluetoothManager>();
        if (bluetoothManager != null)
        {
            bluetoothManager.OnDataReceived.AddListener(HandleBluetoothData);
            bluetoothManager.OnDeviceConnected.AddListener(() =>
            {
                StartCoroutine(VibrateOnConnection());
            });
            bluetoothManager.ConnectPairedDeviceWithRetry(CANE_DEVICE_NAME);
        }
        else
        {
            Debug.LogError("BluetoothManager not found in the scene.");
        }
    }

    private IEnumerator VibrateOnConnection()
    {
        yield return new WaitForSeconds(0.1f); // Buffer
        for (uint i = 0; i < onConnectionVibrateIntervals; i++)
        {
            bluetoothManager.WriteData(onConnectionVibrationStrength.ToString());
            yield return new WaitForSeconds(onConnectionVibrateTime);
            bluetoothManager.WriteData("0");
            yield return new WaitForSeconds(onConnectionVibrateBreakTime);
        }
    }

    private void HandleBluetoothData(string data)
    {
        if (data == "button_1_pressed")
        {
            isCalibrated = false;
            Debug.Log("Calibration reset.");
            return;
        }

        if (data.StartsWith("imu_"))
        {
            string[] values = data.Split('_');
            if (values.Length == 4)
            {
                float x = float.Parse(values[1]);
                float y = float.Parse(values[2]);
                float z = float.Parse(values[3]);

                float currentRotation = x;
                if (!isCalibrated)
                {
                    initialRotation = currentRotation - transform.parent.rotation.eulerAngles.y;
                    isCalibrated = true;
                }

                float adjustedRotation = currentRotation - initialRotation;
                transform.rotation = Quaternion.Euler(0, adjustedRotation, 0);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (bluetoothManager != null)
        {
            bluetoothManager.WriteData(vibrationStrength.ToString());
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
