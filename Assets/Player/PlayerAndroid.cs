using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAndroid : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float backwardMultiplier = 0.5f;
    [SerializeField] float moveThreshold = 0.1f;

    Rigidbody rb;
    Controls controls;

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
}
