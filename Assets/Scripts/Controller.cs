using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Controller : MonoBehaviour
{
    // --- DIŞ REFERANSLAR ---
    [Header("--- ARABA REFERANSLARI ---")]
    [Tooltip("Arabanın ana objesi üzerindeki Car scripti.")]
    public Car targetCar; 
    public Transform carSeatPoint; // Oturma noktası (Araba Child)
    public Transform carExitPoint; // İniş noktası (Araba Child)

    [Header("--- IK AYARLARI (DİREKSİYON) ---")]
    [Tooltip("Direksiyon üzerindeki sol elin tutma noktası.")]
    public Transform leftHandTarget;
    [Tooltip("Direksiyon üzerindeki sağ elin tutma noktası.")]
    public Transform rightHandTarget;
    
    // --- AYARLAR ---
    [Header("--- YAYA HAREKET AYARLARI ---")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("--- KAMERA AYARLARI ---")]
    public Transform cameraRig; 
    [Tooltip("Arabanın içindeki sürücü kamerasının konumu.")] // 👈 YENİ REFERANS EKLENDİ
    public Transform carCameraPoint; 
    public float mouseSensitivity = 100f;
    public float lookXLimitYaya = 85f;
    public float lookXLimitAraba = 45f;
    public float lookYLimitAraba = 90f;
    
    [Header("--- POZİSYON DÜZELTME AYARLARI ---")]
    [Tooltip("Karakterin CharacterController (CC) merkezinden aşağı kaydırılacağı oran. 0.5f (yarısı) normalde yeterlidir.")]
    public float enterVerticalOffset = 0.5f; // CC'nin merkezini koltuğa hizalamak için

    [Tooltip("Karakter CC yüksekliğinin ne kadar yukarısına ışınlansın? (0.8f veya 1.0f deneyin)")]
    public float exitSafetyMultiplier = 0.8f; 

    [Header("--- ETKİLEŞİM & UI ---")]
    public float interactionDistance = 5.0f;
    public float holdDuration = 1.0f; 
    public GameObject loadCanvasObj; 
    public Image loadFillImage; 

    // --- ÖZEL DEĞİŞKENLER ---
    private CharacterController characterController;
    private Animator animator;
    private Vector3 velocity;
    
    private bool isInsideCar = false;
    private bool isProcessing = false;
    private float currentHoldTime = 0f;
    private bool isInteractionActive = false; 
    
    private float xRotation = 0f;
    private float yRotationInCar = 0f; 
    
    // Kamerayı geri bağlamak için başlangıç pozisyonlarını kaydeder 👈 YENİ
    private Transform initialCameraParent; 
    private Vector3 initialCameraLocalPosition;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); 
        
        // carCameraPoint referans kontrolü Start'a eklendi
        if (targetCar == null || carSeatPoint == null || carExitPoint == null || cameraRig == null || carCameraPoint == null)
        {
             Debug.LogError("Controller: Eksik referanslar var! Lütfen tüm referansları bağlayın (carCameraPoint dahil).");
             this.enabled = false;
             return;
        }

        // Başlangıç kamera ayarlarını kaydet 👈 YENİ
        initialCameraParent = cameraRig.parent;
        initialCameraLocalPosition = cameraRig.localPosition;

        // IK hedefleri kontrolü (aynı kaldı)
        if (leftHandTarget == null || rightHandTarget == null)
        {
             Debug.LogWarning("Controller: IK Hedefleri (leftHandTarget/rightHandTarget) bağlanmamış. El kenetleme animasyonu çalışmayabilir.");
        }

        targetCar.enabled = false; 
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ResetUI();

        if (cameraRig != null)
        {
            xRotation = cameraRig.localEulerAngles.x;
            if (xRotation > 180) xRotation -= 360;
        }
    }

    void Update()
    {
        if (isProcessing) return; 

        if (isInsideCar)
        {
            HandleCarSteering(); 
            HandleCarLook();
            HandleExitInteraction();
        }
        else
        {
            HandleMovement();
            HandleCharacterLook();
            HandleEntryInteraction();
        }
    }

    // --- YENİ FONKSİYON: ARABA DİREKSİYON KONTROLÜ (A/D TUŞLARI) ---
    void HandleCarSteering()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); 

        if (targetCar != null)
        {
            targetCar.steeringInput = horizontalInput;
        }
    }

    // --- YAYA HAREKET VE BAKMAK (MEVCUT İÇERİK) ---
    void HandleMovement()
    {
        // ... (İçerik aynı kaldı)
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0) 
            velocity.y = -2f; 

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
        Vector3 move = transform.right * x + transform.forward * z;

        characterController.Move(move * currentSpeed * Time.deltaTime);

        if (animator != null)
        {
            bool isMoving = move.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if(animator != null) animator.SetTrigger("Jump");
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleCharacterLook()
    {
        // ... (İçerik aynı kaldı)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookXLimitYaya, lookXLimitYaya);

        if (cameraRig != null)
            cameraRig.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCarLook()
    {
        // ... (İçerik aynı kaldı)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookXLimitAraba, lookXLimitAraba); 

        yRotationInCar += mouseX;
        yRotationInCar = Mathf.Clamp(yRotationInCar, -lookYLimitAraba, lookYLimitAraba); 

        if(cameraRig != null)
        {
            cameraRig.localRotation = Quaternion.Euler(xRotation, yRotationInCar, 0f);
        }
    }
    // ------------------------------------------

    // --- ETKİLEŞİM MANTIĞI (MEVCUT İÇERİK) ---
    void HandleEntryInteraction()
    {
        float distance = Vector3.Distance(transform.position, targetCar.transform.position); 
        bool isCloseEnough = distance <= interactionDistance;

        if (isCloseEnough && Input.GetKey(KeyCode.E))
        {
            if (!isInteractionActive) 
            {
                isInteractionActive = true;
                if (loadCanvasObj != null) loadCanvasObj.SetActive(true);
            }
            
            currentHoldTime += Time.deltaTime;

            if (loadFillImage != null)
                loadFillImage.fillAmount = currentHoldTime / holdDuration;

            if (currentHoldTime >= holdDuration)
            {
                EnterCar();
                isInteractionActive = false; 
                currentHoldTime = 0f;
                ResetUI(); 
            }
        }
        else 
        {
            if (isInteractionActive) 
            {
                StopInteraction();
            }
        }
    }
    
    void HandleExitInteraction()
    {
        // ... (İçerik aynı kaldı)
        if (Input.GetKey(KeyCode.E))
        {
            if (!isInteractionActive)
            {
                isInteractionActive = true;
                if (loadCanvasObj != null) loadCanvasObj.SetActive(true);
            }

            currentHoldTime += Time.deltaTime;
            
            if (loadFillImage != null)
                loadFillImage.fillAmount = currentHoldTime / holdDuration;

            if (currentHoldTime >= holdDuration)
            {
                StartCoroutine(ExitCarRoutine());
                
                isInteractionActive = false;
                currentHoldTime = 0f;
                ResetUI();
            }
        }
        else if (isInteractionActive) 
        {
            StopInteraction();
        }
    }

    void StopInteraction()
    {
        isInteractionActive = false;
        ResetUI();
    }

    void ResetUI()
    {
        currentHoldTime = 0f;
        if (loadFillImage != null) loadFillImage.fillAmount = 0f;
        if (loadCanvasObj != null) loadCanvasObj.SetActive(false);
    }
    
    // --- BİNME/İNME FONKSİYONLARI ---

    void EnterCar()
    {
        isProcessing = true;
        isInsideCar = true;

        velocity = Vector3.zero;
        characterController.enabled = false;
        
        // KONUM DÜZELTMESİ (Aynı kaldı)
        float ccHeight = characterController.height;
        Vector3 finalSeatPos = carSeatPoint.position;
        
        finalSeatPos -= transform.up * (ccHeight * enterVerticalOffset); 
        
        transform.SetParent(targetCar.transform);
        transform.position = finalSeatPos; 
        transform.rotation = carSeatPoint.rotation; 
        
        // KAMERAYI ARACA BAĞLA (Göz hizasına) 👈 GÜNCELLEME
        if (cameraRig != null && carCameraPoint != null)
        {
            // cameraRig'in parent'ını arabadaki kamera hedefine ayarla
            cameraRig.SetParent(carCameraPoint); 
            // Yerel pozisyonu ve rotasyonu sıfırla, böylece tam carCameraPoint'te durur
            cameraRig.localPosition = Vector3.zero;
            cameraRig.localRotation = Quaternion.identity; 
        }

        targetCar.enabled = true; 
        
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isinCar", true); 
        }

        xRotation = 0f;
        yRotationInCar = 0f;
        // cameraRig'in localRotation'ı yukarıda zaten sıfırlandı.

        isProcessing = false;
    }

 IEnumerator ExitCarRoutine()
    {
        isProcessing = true;
        isInsideCar = false;
        StopInteraction();

        targetCar.enabled = false;

        // Pozisyonu Global'den Al (Aynı kaldı)
        Vector3 exitPos = carExitPoint.position;
        Quaternion exitRot = Quaternion.Euler(0, carExitPoint.rotation.eulerAngles.y, 0); 
        
        // KAMERAYI KARAKTERE GERİ BAĞLA 👈 GÜNCELLEME
        if (cameraRig != null)
        {
            // Kamerayı başlangıçtaki parent'ına geri bağla
            cameraRig.SetParent(initialCameraParent); 
            // Kaydedilen başlangıçtaki yerel pozisyonuna ve rotasyonuna sıfırla
            cameraRig.localPosition = initialCameraLocalPosition;
            cameraRig.localRotation = Quaternion.identity;
        }

        if (animator != null)
        {
            animator.SetBool("isinCar", false);
        }

        transform.SetParent(null);

        // IŞINLANMA ve Güvenlik Payı (Aynı kaldı)
        float ccHeight = characterController != null ? characterController.height : 2f;
        
        Vector3 finalPos = exitPos + Vector3.up * (ccHeight * 0.5f); 
        finalPos += Vector3.up * 0.1f; 
        
        transform.position = finalPos; 
        transform.rotation = exitRot;
        
        velocity = Vector3.zero;
        
        yield return null; 

        characterController.enabled = true; 
        
        // Kamera ayarlarını yaya moduna döndür (Aynı kaldı)
        if (cameraRig != null)
        {
             xRotation = cameraRig.localEulerAngles.x;
             if (xRotation > 180) xRotation -= 360;
        }

        isProcessing = false;
    }

    // --- DİREKSİYONA ELLERİ KENETLEME (IK) ---

    /// <summary>
    /// Animasyon döngüsünün sonunda Inverse Kinematics (IK) ayarlarını uygular.
    /// Karakter arabaya girdiğinde ellerini hedef noktalara zorlar.
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        // Sadece arabada olduğumuzda ve Animator mevcutsa çalıştır
        if (isInsideCar && animator != null)
        {
            // SOL EL IK AYARLARI
            if (leftHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
                
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }

            // SAĞ EL IK AYARLARI
            if (rightHandTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
                
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }
        }
        else if(animator != null)
        {
            // Arabada değilken, IK etkisini sıfırla
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }
    }
}