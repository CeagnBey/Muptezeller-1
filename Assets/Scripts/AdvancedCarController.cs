using UnityEngine;
using UnityEngine.UI;

public class CarSystemUI : MonoBehaviour
{
    [Header("Gerekli Objeler")]
    public Transform carSeatPoint;     // Karakterin poposunun geleceği nokta (Empty Object)
    public Transform carExitPoint;     // İnince doğacağı nokta
    public Transform carObject;        // Arabanın ana objesi (Hareket için)
    
    [Header("UI Ayarları")]
    public Image loadFillImage;        
    public GameObject loadCanvasObj;   
    public float holdDuration = 1.5f;  

    [Header("Kamera & Ayarlar")]
    public Transform cameraRig;        
    public float mouseSensitivity = 100f;

    // Değişkenler
    private float currentHoldTime = 0f;
    private bool isInsideCar = false;
    private CharacterController controller;
    private Animator animator;
    
    // Orijinal kamera açısını saklamak için
    private float defaultYRotation;
    
    private float xRotation = 0f;
    private float yRotationInCar = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        
        Cursor.lockState = CursorLockMode.Locked;

        if (loadCanvasObj != null) loadCanvasObj.SetActive(false);
        if (loadFillImage != null) loadFillImage.fillAmount = 0f;
    }

    void Update()
    {
        HandleInteraction(); 
        
        if (isInsideCar)
        {
            HandleCarLook(); 
        }
        else
        {
            HandleFootLook(); 
            HandleMovement(); 
        }
    }

    void HandleInteraction()
    {
        float distance = Vector3.Distance(transform.position, carExitPoint.position);
        bool isClose = distance <= 4.0f; // Mesafe biraz artırıldı

        // BİNME MANTIĞI
        if (!isInsideCar && isClose)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (loadCanvasObj != null) loadCanvasObj.SetActive(true);
                currentHoldTime += Time.deltaTime;
                
                if (loadFillImage != null)
                    loadFillImage.fillAmount = currentHoldTime / holdDuration;

                if (currentHoldTime >= holdDuration)
                {
                    EnterCar();
                    ResetUI();
                }
            }
            else
            {
                ResetUI();
            }
        }
        // İNME MANTIĞI (Tek basış)
        else if (isInsideCar && Input.GetKeyDown(KeyCode.E))
        {
            ExitCar();
        }
        else
        {
            if (!Input.GetKey(KeyCode.E)) ResetUI();
        }
    }

    void EnterCar()
    {
        isInsideCar = true;

        // 1. Fiziği Kapat
        controller.enabled = false;

        // 2. Animator Ayarları (KAYMAYI ÖNLEYEN KISIM)
        if (animator != null)
        {
            animator.SetBool("isinCar", true);
            animator.SetBool("isWalking", false);
            // Root Motion'ı KAPAT. Karakter animasyonla ileri gitmesin, olduğu yerde otursun.
            animator.applyRootMotion = false; 
        }

        // 3. Konumlandırma (KESİN ÇÖZÜM)
        // Karakteri koltuk objesinin "çocuğu" yapıyoruz.
        transform.SetParent(carSeatPoint); 
        
        // Yerel pozisyonu sıfırlıyoruz. Yani SeatPoint neredeyse, karakter tam orada olur.
        transform.localPosition = Vector3.zero; 
        transform.localRotation = Quaternion.identity; // SeatPoint nereye bakıyorsa oraya bak.

        // 4. Kamerayı Sıfırla
        xRotation = 0f;
        yRotationInCar = 0f;
    }

    void ExitCar()
    {
        isInsideCar = false;

        // 1. Parent'tan çık
        transform.SetParent(null);

        // 2. Kapı önüne ışınla
        transform.position = carExitPoint.position;
        // Yere düz bassın diye rotasyonu düzelt
        transform.rotation = Quaternion.Euler(0, carExitPoint.eulerAngles.y, 0);

        // 3. Animator Sıfırla (AYAĞA KALDIRAN KISIM)
        if (animator != null)
        {
            animator.SetBool("isinCar", false);
            // Karakter yere inince tekrar yürüyebilmesi için Root Motion gerekiyorsa aç, gerekmiyorsa kapalı kalsın.
            // Genelde CharacterController kullanıldığı için kapalı kalması daha iyidir.
            animator.applyRootMotion = false; 
            
            // Görseli (Mesh) eski yerine/sıfıra getir (Önemli!)
            animator.transform.localPosition = Vector3.zero;
            animator.transform.localRotation = Quaternion.identity;
        }

        // 4. Fiziği Aç
        controller.enabled = true;
    }

    void ResetUI()
    {
        currentHoldTime = 0f;
        if (loadFillImage != null) loadFillImage.fillAmount = 0f;
        if (loadCanvasObj != null) loadCanvasObj.SetActive(false);
    }
    
    void HandleCarLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -50f, 50f);

        yRotationInCar += mouseX;
        yRotationInCar = Mathf.Clamp(yRotationInCar, -70f, 70f);

        if (cameraRig != null)
            cameraRig.localRotation = Quaternion.Euler(xRotation, yRotationInCar, 0f);
    }

    void HandleFootLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cameraRig != null)
            cameraRig.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * 5f * Time.deltaTime);
        
        if (animator != null) animator.SetBool("isWalking", move.magnitude > 0.1f);
    }
}