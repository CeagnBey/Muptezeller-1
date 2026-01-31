using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InterractionController : MonoBehaviour
{
    [Header("--- REFERANSLAR ---")]
    // FreeRoamController yerine Controller kullanılıyor
    [Tooltip("Yaya kontrol scripti (Controller)")]
    public Controller controller;
    
    [Tooltip("Araba kontrol scripti")]
    public Car targetCar; 
    
    [Tooltip("Sadece kafa bakış scripti")]
    public CarLookHandler carLookHandler;
    
    [Header("Araba Pozisyonları")]
    public Transform carSeatPoint; // Oturma noktası
    public Transform carExitPoint; // İniş noktası

    [Header("Etkileşim Ayarları")]
    public float interactionDistance = 5.0f;
    public float holdDuration = 1.0f; 

    [Header("UI Ayarları")]
    public GameObject loadCanvasObj; 
    public Image loadFillImage; 
    
    // --- ÖZEL DEĞİŞKENLER ---
    private CharacterController characterController; // Karakterin kendisindeki CC
    private bool isInsideCar = false;
    private bool isInteractionActive = false;
    private float currentHoldTime = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Güvenlik: Başlangıçta tüm scriptler doğru durumda olmalı
        if (targetCar == null || controller == null || carLookHandler == null)
        {
             Debug.LogError("InteractionController referansları eksik! Lütfen Controller, Car ve CarLookHandler referanslarını bağlayın.");
             this.enabled = false;
             return;
        }

        targetCar.enabled = false;
        carLookHandler.enabled = false;
        // FreeRoamController yerine Controller kullanılıyor
        controller.enabled = true;
        ResetUI();
    }

    void Update()
    {
        if (isInsideCar)
        {
            HandleExitInteraction(); 
        }
        else
        {
            HandleEntryInteraction(); 
        }
    }

    // --- BİNME İŞLEMİ (Yaya Modu Aktifken) ---

    void HandleEntryInteraction()
    {
        if (targetCar == null || carExitPoint == null) return;
        
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
                StopInteraction();
            }
        }
        else if (isInteractionActive && (Input.GetKeyUp(KeyCode.E) || !isCloseEnough))
        {
            StopInteraction();
        }
    }
    
    // --- İNME İŞLEMİ (Araba Modu Aktifken) ---

    void HandleExitInteraction()
    {
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
            }
        }
        else if (isInteractionActive && Input.GetKeyUp(KeyCode.E))
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
    
    // --- BİNME İŞLEMİ ---

    void EnterCar()
    {
        isInsideCar = true;

        // 1. KONTROLLERİ DEĞİŞTİR: Yaya kontrolünü kapat, Araba/Kafa kontrolünü aç
        // Controller kullanılıyor
        controller.enabled = false;
        targetCar.enabled = true;
        carLookHandler.enabled = true;

        // 2. CharacterController'ı devre dışı bırak
        characterController.enabled = false;
        
        // 3. Parenting ve Pozisyon
        transform.SetParent(targetCar.transform);
        transform.position = carSeatPoint.position;
        transform.rotation = carSeatPoint.rotation; 
        
        // 4. Animasyon ayarları (Sizin animasyonlarınızı devre dışı bırakır)
        // Controller kullanılıyor
        if (controller.GetComponentInChildren<Animator>() != null)
        {
            Animator anim = controller.GetComponentInChildren<Animator>();
            anim.SetBool("isWalking", false);
            // Gerekli araba içi animasyon parametrelerini buraya ekleyebilirsiniz.
        }
    }

    // --- İNME İŞLEMİ (Coroutine ile güvenli geçiş) ---

    IEnumerator ExitCarRoutine()
    {
        isInsideCar = false;
        StopInteraction();

        // 1. KONTROLLERİ DEĞİŞTİR: Araba/Kafa kontrolünü kapat
        targetCar.enabled = false;
        carLookHandler.enabled = false;
        carLookHandler.ResetLook(); // Kafa dönüşünü sıfırla

        // 2. Pozisyonu Al (Global)
        Vector3 exitPos = carExitPoint.position;
        Quaternion exitRot = Quaternion.Euler(0, carExitPoint.rotation.eulerAngles.y, 0); 
        
        // 3. Parent'tan AYRIL
        transform.SetParent(null);

        // 4. IŞINLANMA ve Çarpışma Önleme
        float ccHeight = characterController != null ? characterController.height : 2f;
        Vector3 finalPos = exitPos + Vector3.up * (ccHeight * 0.5f);
        
        transform.position = finalPos; 
        transform.rotation = exitRot;
        
        // 5. Kritik Bekleme
        yield return null; 

        // 6. CharacterController'ı Aç
        characterController.enabled = true;
        
        // 7. KONTROLLERİ AÇ: Yaya kontrolünü aç
        // Controller kullanılıyor
        controller.enabled = true;
    }
}