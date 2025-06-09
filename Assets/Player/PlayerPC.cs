using UnityEngine;

public class PlayerPC : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    float speedMultiplier = 1f;
    [SerializeField] float rotationSpeed = 100f;

    Rigidbody rb;
    Controls controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new Controls();
        controls.PlayerPC.Enable();
    }

    void Update()
    {
        float newSpeedMultiplier = controls.PlayerPC.AdjustSpeed.ReadValue<float>();
        if (newSpeedMultiplier != 0f)
        {
            speedMultiplier = newSpeedMultiplier switch
            {
                1f => 0.25f,
                2f => 0.5f,
                3f => 0.75f,
                4f => 1f,
                _ => 1f,
            };
        }
    }

    void FixedUpdate()
    {
        float moveInput = controls.PlayerPC.Move.ReadValue<float>();
        HandleMovement(moveInput);

        float rotateInput = controls.PlayerPC.Rotate.ReadValue<float>();
        HandleRotation(rotateInput);
    }

    void HandleMovement(float input)
    {
        if (input != 0f)
        {
            rb.MovePosition(rb.position + input * speed * speedMultiplier * Time.fixedDeltaTime * transform.forward);
        }
    }

    void HandleRotation(float input)
    {
        if (input != 0f)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationSpeed * input * Time.fixedDeltaTime, 0));
        }
    }
}
