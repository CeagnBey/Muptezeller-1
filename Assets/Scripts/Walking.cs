using UnityEngine;

public class Walking : MonoBehaviour
{
    public CharacterController controller;
    public Transform aimTarget;   // kafanın hedefi (MultiAim Target)
    public Transform headBone;    // kafanın gerçek kemik transformu

    public float mouseSensitivity = 200f;
    public float yurumeHizi = 6f;

    public float minPitch = -40f;
    public float maxPitch = 70f;

    float pitch = 0f;   // kafa yukarı-aşağı
    float yaw = 0f;     // kafa sağ-sol

    Vector3 velocity;
    float gravity = -9.81f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --------------------------
        // 1. MOUSE OKUMA
        // --------------------------
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Kafanın yatay dönüşü
        yaw += mouseX;

        // Kafanın dikey dönüşü
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // --------------------------
        // 2. KAFAYI DÖNDÜR (Rig Target)
        // --------------------------
        if (aimTarget != null)
        {
            aimTarget.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // --------------------------
        // 3. GÖVDE YALNIZCA YAW'U TAKİP EDER
        // --------------------------
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // --------------------------
        // 4. YÜRÜME (W → kafanın yönü)
        // --------------------------
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * yurumeHizi * Time.deltaTime);

        // Yerçekimi
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
