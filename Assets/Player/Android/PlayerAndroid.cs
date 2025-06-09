using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerAndroid : MonoBehaviour
{
    const float speed = 5f;
    const float backwardMultiplier = 0.5f;
    const float moveThreshold = 0.1f;
    bool isColliding = false;
    bool isVibrating = false;

    Rigidbody rb;
    Controls controls;
    BluetoothManager bluetoothManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new Controls();
        controls.PlayerAndroid.Enable();

        if (Accelerometer.current != null)
        {
            InputSystem.EnableDevice(Accelerometer.current);
        }

        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
        }
    }

    void Start()
    {
        bluetoothManager = FindFirstObjectByType<BluetoothManager>();
        if (bluetoothManager != null)
        {
            bluetoothManager.OnDataReceived.AddListener(HandleBluetoothData);
            bluetoothManager.ConnectPairedDeviceWithRetry("BlindCane");
        }
        else
        {
            Debug.LogError("BluetoothManager not found in the scene.");
        }
    }

    void Update()
    {
        if (isColliding && !isVibrating)
        {
            long[] vibrationPattern = { 0, 100 };
            VibrationManager.Vibrate(vibrationPattern, repeat: 0);
            isVibrating = true;
        }
        else if (!isColliding && isVibrating)
        {
            VibrationManager.Cancel();
            isVibrating = false;
        }
    }

    private void HandleBluetoothData(string data)
    {
        if (data == "reset")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void FixedUpdate()
    {
        Vector3 moveInput = controls.PlayerAndroid.Move.ReadValue<Vector3>();
        HandleMovement(moveInput);

        Quaternion rotateInput = controls.PlayerAndroid.Rotate.ReadValue<Quaternion>();
        HandleRotation(rotateInput);
    }

    void HandleMovement(Vector3 input)
    {
        if (input != Vector3.zero)
        {
            if (input.y > moveThreshold)
            {
                rb.MovePosition(rb.position + input.y * speed * Time.fixedDeltaTime * transform.forward);
            }
            else if (input.y < -moveThreshold)
            {
                rb.MovePosition(rb.position + input.y * backwardMultiplier * speed * Time.fixedDeltaTime * transform.forward);
            }
        }
    }

    void HandleRotation(Quaternion input)
    {
        if (input != Quaternion.identity)
        {
            transform.rotation = Quaternion.Euler(0, -input.eulerAngles.z, 0);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }
}
