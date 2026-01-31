using System.Collections;
using UnityEngine;

public class CarEtkilesim : MonoBehaviour
{
    [Header("Gerekli Atamalar")]
    public Transform carEntryPoint; // Arabanın kapısının önündeki hizalama noktası (Empty Object)
    public Transform carSeatParent; // Arabanın kendisi (veya koltuk) - Karakter buna parent olacak
    public GameObject interactionUI; // "E Bin" yazısı

    [Header("İnce Ayarlar")]
    public float interactionDistance = 3.0f; // Ne kadar yakından binilsin?
    public float sittingYOffset = 0.0f; // Koltukta yukarı/aşağı ince ayar (Örn: -0.1 veya 0.05)
    
    // Animasyon sürelerini manuel girmek en sağlıklısıdır (Animasyonun saniyesi neyse onu yaz)
    public float enterAnimDuration = 2.5f; 
    
    private Animator animator;
    private CharacterController controller;
    private bool isInsideCar = false;
    private bool isActionInProgress = false; // O sırada binme/inme işlemi var mı?

    // Karakterin Mesh'inin (Görselinin) orijinal yerini saklamak için
    private Vector3 initialMeshLocalPos;
    private Quaternion initialMeshLocalRot;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        
        // UI Başlangıçta kapalı
        if (interactionUI != null) interactionUI.SetActive(false);

        // Orijinal mesh konumunu kaydet (İnince bozulmasın diye)
        if (animator != null)
        {
            initialMeshLocalPos = animator.transform.localPosition;
            initialMeshLocalRot = animator.transform.localRotation;
        }
    }

    void Update()
    {
        // İşlem sürüyorsa (biniyor veya iniyorsa) hiçbir şey yapma, bekle.
        if (isActionInProgress) return;

        // UI Göster/Gizle Mantığı
        if (carEntryPoint != null && !isInsideCar)
        {
            float mesufe = Vector3.Distance(transform.position, carEntryPoint.position);
            if (interactionUI != null) interactionUI.SetActive(mesufe <= interactionDistance);
        }
        else
        {
            if (interactionUI != null) interactionUI.SetActive(false);
        }

        // E Tuşuna Basınca
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isInsideCar)
            {
                StartCoroutine(ExitRoutine()); // İçerdeysek in
            }
            else
            {
                // Dışardaysak ve yakınsak bin
                float dist = Vector3.Distance(transform.position, carEntryPoint.position);
                if (dist <= interactionDistance)
                {
                    StartCoroutine(EnterRoutine());
                }
            }
        }
    }

    IEnumerator EnterRoutine()
    {
        isActionInProgress = true;
        if (interactionUI != null) interactionUI.SetActive(false);

        // 1. ÖNCE Karakteri Durdur (Kaymayı önle)
        // Eğer hareket halindeyken basarsan momentum devam edebilir, bunu sıfırlayalım.
        if (controller != null)
        {
             // Basit bir trick: Controller'ı kapatmadan önce hareketi kes
             // (Bu kısım opsiyonel ama güvenlidir)
        }

        // 2. FİZİKLERİ KAPAT
        controller.enabled = false; 

        // 3. KRİTİK NOKTA: Fiziğin kapandığını Unity'nin anlaması için 1 kare bekle
        yield return null; 

        // 4. HİZALAMA (IŞINLANMA)
        // Artık fizik kapalı olduğu için bu işlem %100 çalışacak.
        transform.position = carEntryPoint.position;
        transform.rotation = carEntryPoint.rotation;

        // Işınlanmanın görsel olarak oturması için 1 kare daha bekle (Garanti olsun)
        yield return new WaitForEndOfFrame();

        // 5. ANİMASYONU BAŞLAT
        if (animator != null)
        {
            // Transition Duration 0 olduğu için anında buradan başlayacak
            animator.SetBool("isinCar", true); 
            animator.SetBool("isWalking", false); // Yürüme varsa keselim
            
            // Animasyon başladığı an Root Motion'ı aç
            animator.applyRootMotion = true; 
        }

        // 6. ANİMASYON SÜRESİ KADAR BEKLE
        yield return new WaitForSeconds(enterAnimDuration);

        // 7. ARABAYA PARENT YAP VE OTURT
        isInsideCar = true;
        transform.SetParent(carSeatParent); 

        // Offset düzeltmesi
        if (animator != null)
        {
            Vector3 finalPos = animator.transform.localPosition;
            finalPos.y += sittingYOffset; 
            animator.transform.localPosition = finalPos;
        }

        isActionInProgress = false;
    }

    IEnumerator ExitRoutine()
    {
        isActionInProgress = true;

        // 1. ÖNCE PARENT'TAN ÇIKAR (Araba hareket ederse sorun olmasın diye)
        transform.SetParent(null);

        // 2. ANİMASYONU TERSİNE ÇEVİR (Speed -1 ayarlı Exiting state)
        if (animator != null)
        {
            animator.SetBool("isinCar", false);
            animator.applyRootMotion = true; // Root motion ile geri geri çıkacak
            
            // Offset ayarını sıfırla ki karakter yerde iken gömülü kalmasın
            animator.transform.localPosition = initialMeshLocalPos;
            animator.transform.localRotation = initialMeshLocalRot;
        }

        // 3. ÇIKIŞ SÜRESİ KADAR BEKLE (Giriş süresiyle aynıdır)
        yield return new WaitForSeconds(enterAnimDuration);

        // 4. FİZİKLERİ TEKRAR AÇ
        isInsideCar = false;
        controller.enabled = true; // Artık tekrar yürüyebilirsin
        
        // Root Motion'ı kapat, kontrolcü devralıyor
        if (animator != null) animator.applyRootMotion = false;

        isActionInProgress = false;
    }
}