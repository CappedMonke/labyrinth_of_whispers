using UnityEngine;

public class BlindCanePC : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;

    private Rigidbody rb;
    private Controls controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new Controls();
        controls.BlindCanePC.Enable();
    }

    void FixedUpdate()
    {
        float rotateInput = controls.BlindCanePC.Rotate.ReadValue<float>();
        HandleRotate(rotateInput);
    }

    private void HandleRotate(float input)
    {
        if (input != 0f)
        {
            transform.Rotate(0f, rotationSpeed * input * Time.fixedDeltaTime, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Wall"))
        {
            return;
        }

        Debug.Log("Entered Vibration Zone");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Wall"))
        {
            return;
        }

        Debug.Log("Exited Vibration Zone");
    }
}
