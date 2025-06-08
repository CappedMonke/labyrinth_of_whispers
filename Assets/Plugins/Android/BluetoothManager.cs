using UnityEngine.Android;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BluetoothManager : MonoBehaviour
{
    public UnityEvent<string> OnDataReceived = new();

    private bool isConnected = false;
    private string lastConnectedMac;

    private static AndroidJavaClass bluetoothPlugin;
    private static AndroidJavaObject BluetoothConnector;

    void Awake()
    {
        InitBluetooth();
    }

    // creating an instance of the bluetooth class from the plugin 
    public void InitBluetooth()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        // Check BT and location permissions
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)
            || !Permission.HasUserAuthorizedPermission(Permission.FineLocation)
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADMIN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADVERTISE")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {

            Permission.RequestUserPermissions(new string[] {
                        Permission.CoarseLocation,
                            Permission.FineLocation,
                            "android.permission.BLUETOOTH_ADMIN",
                            "android.permission.BLUETOOTH",
                            "android.permission.BLUETOOTH_SCAN",
                            "android.permission.BLUETOOTH_ADVERTISE",
                             "android.permission.BLUETOOTH_CONNECT"
                    });
        }

        bluetoothPlugin = new AndroidJavaClass("com.example.bluetoothplugin.BluetoothConnector");
        BluetoothConnector = bluetoothPlugin.CallStatic<AndroidJavaObject>("getInstance");
    }

    // Start device scan
    public void StartScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("StartScanDevices");
    }

    // Stop device scan
    public void StopScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("StopScanDevices");
    }

    // This function will be called by Java class to update the scan status,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ScanStatus(string status)
    {
    }

    // This function will be called by Java class whenever a new device is found,
    // and delivers the new devices as a string data="MAC+NAME"
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void NewDeviceFound(string data)
    {
    }

    // Get paired devices from BT settings
    public string[] GetPairedDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return null;

        // This function when called returns an array of PairedDevices as "MAC+Name" for each device found
        string[] data = BluetoothConnector.CallStatic<string[]>("GetPairedDevices"); ;

        // Display the paired devices
        foreach (var d in data)
        {
            Debug.Log("Paired Device: " + d);
        }

        return data;
    }

    // Start BT connect using device MAC address "deviceAdd"
    public void StartConnection(string mac)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        lastConnectedMac = mac;
        BluetoothConnector.CallStatic("StartConnection", mac);
    }

    // Stop BT connetion
    public void StopConnection()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (isConnected)
            BluetoothConnector.CallStatic("StopConnection");
    }

    // This function will be called by Java class to update BT connection status,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ConnectionStatus(string status)
    {
        isConnected = status == "connected";

        if (isConnected)
        {
            Toast("Connected");
        }

        if (status == "disconnected")
        {
            Toast("Disconnected, trying to reconnect...");
            StartCoroutine(RetryConnection(lastConnectedMac));
        }
    }

    // This function will be called by Java class whenever BT data is received,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ReadData(string data)
    {
        // Debug.Log("BT Stream: " + data);
        OnDataReceived?.Invoke(data);
    }

    // Write data to the connected BT device
    public void WriteData(string data)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (isConnected)
            BluetoothConnector.CallStatic("WriteData", data + "\n");
    }

    // This function will be called by Java class to send Log messages,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ReadLog(string data)
    {
        Debug.Log(data);
    }


    // Function to display an Android Toast message
    public void Toast(string data)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("Toast", data);
    }

    private string GetMacAddressFromPairedDevices(string macAddress)
    {
        string[] pairedDevices = GetPairedDevices();

        foreach (string device in pairedDevices)
        {
            // Split device string into MAC and NAME
            string[] parts = device.Split('+');
            if (parts.Length == 2)
            {
                string mac = parts[0];
                string name = parts[1];
                if (name == macAddress)
                {
                    return mac;
                }
            }
        }

        return null;
    }

    public void ConnectPairedDevice(string deviceName)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        Toast("Trying to connect to " + deviceName);

        string mac = GetMacAddressFromPairedDevices(deviceName);
        if (mac != null)
        {
            StartConnection(mac);
        }
    }

    public void ConnectPairedDeviceWithRetry(string deviceName, int maxRetries = -1, float retryDelay = 5.0f)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        Toast("Trying to connect to " + deviceName);

        string mac = GetMacAddressFromPairedDevices(deviceName);
        if (mac != null)
        {
            StartCoroutine(RetryConnection(mac, maxRetries, retryDelay));
        }
    }

    private IEnumerator RetryConnection(string mac, int maxRetries = -1, float retryDelay = 5.0f)
    {
        int attempt = 0;
        while (maxRetries == -1 || attempt < maxRetries)
        {
            Debug.Log($"Attempting to connect to {mac}, attempt {attempt + 1}/{(maxRetries == -1 ? "∞" : maxRetries)}");
            StartConnection(mac);

            yield return new WaitForSeconds(retryDelay);

            if (isConnected)
            {
                Debug.Log($"Successfully connected to {mac} on attempt {attempt + 1}");
                yield break;
            }

            attempt++;
        }

        if (maxRetries != -1)
        {
            Debug.LogError($"Failed to connect to {mac} after {maxRetries} attempts");
        }
    }
}
