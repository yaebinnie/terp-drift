using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class CarSound : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audio;
    private float[] samples = new float [256];


    [Header("Drift Settings")]
    public float sensitivity = 15f;
    public float maxSpeed = 20f;
    public float driftFactor = 0.95f;
    public float turnSpeed = 3f;
    public float forwardFriction = 0.9f;
    public float sideFriction = 0.2f;

    public Vector3 movementDirection = Vector3.forward;

    private float speed;
    private float inputHorizontal;
    private float inputVertical;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        audio.GetSpectrumData(samples, 0, FFTWindow.Blackman);

        // Calculate the average amplitude of the sound
        float sum = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i];
        }
        float averageAmplitude = sum / samples.Length;

        // Move the Car Object based on the amplitude
        speed = Mathf.Clamp(averageAmplitude * sensitivity, 0, maxSpeed);
        transform.Translate(movementDirection * speed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // Apply forward movement
        Vector3 forwardMovement = transform.forward * speed;
        rb.AddForce(forwardMovement, ForceMode.Acceleration);

        // Drift mechanics using friction control
        Vector3 velocity = rb.linearVelocity;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Forward friction
        localVelocity.z *= forwardFriction;
        
        // Side friction for drifting effect
        localVelocity.x *= sideFriction;
        
        rb.linearVelocity = transform.TransformDirection(localVelocity);

        // Car rotation for drifting
        if (inputHorizontal != 0)
        {
            float turn = inputHorizontal * turnSpeed * Mathf.Sign(localVelocity.z);
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }
}
