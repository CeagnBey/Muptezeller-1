using UnityEngine;

public class Car : MonoBehaviour
{
    // --- DIŞ REFERANSLAR ---
    [Header("Colliderlar (Yeşil Çemberler)")]
    public WheelCollider w1, w2, s1, s2; 

    [Header("Görsel Tekerlekler (Mesh Modelleri)")]
    public Transform w1Mesh, w2Mesh, s1Mesh, s2Mesh; 
    
    [Header("--- DİREKSİYON VİSUAL REFERANS ---")] // 👈 YENİ
    [Tooltip("Direksiyon objesinin Transform'u.")]
    public Transform steeringWheel; 

    // --- AYARLAR ---
    [Header("Ayarlar")]
    public float hiz = 1500f;
    public float dh = 30f;       
    public float frenGucu = 3000f; 

    [Header("Viraj Hız Ayarı")]
    [Tooltip("Virajda hız düşmemesi için motoru kaç kat güçlendirelim?")]
    public float virajTakviyesi = 1.5f; 

    [Header("Direksiyon Ayarları")] // 👈 YENİ
    [Tooltip("Direksiyonun maksimum dönüş açısı (örneğin 45 derece).")]
    public float maxSteeringAngle = 45f;
    [Tooltip("Direksiyonun merkeze dönüş hızı (input sıfırlanınca).")]
    public float steeringReturnSpeed = 3f;

    // --- CONTROLLER'DAN GELEN GİRDİ ---
    [HideInInspector] 
    // Controller script'i bu değişkeni kullanarak A/D inputunu gönderir.
    public float steeringInput = 0f; // 👈 YENİ

    // --- ÖZEL DEĞİŞKENLER ---
    private float currentVisualSteeringAngle = 0f; // Direksiyonun mevcut görsel açısı

    void Update()
    {
        // Girdileri al (Dikey girdi hala W/S'ten alınır)
        float dikeyGiris = Input.GetAxis("Vertical");   

        // Yatay girdi artık Controller'dan gelen 'steeringInput' değişkeninden alınır.
        float yatayGiris = steeringInput; // 👈 DEĞİŞTİ!

        // --- HIZ KAYBINI ÖNLEYEN KOD ---
        float guncelHiz = hiz;

        // Eğer direksiyon çevriliyorsa (Controller'dan gelen input)
        if (Mathf.Abs(yatayGiris) > 0.1f)
        {
            guncelHiz *= virajTakviyesi; 
        }

        // Hesaplanan gücü uygula
        float motor = guncelHiz * dikeyGiris;
        float steer = dh * yatayGiris; // Fiziksel Direksiyon Açısı

        // 1. FİZİKSEL DİREKSİYON (Sadece önler)
        w1.steerAngle = steer;
        w2.steerAngle = steer;

        // 2. FREN VE GAZ MANTIĞI (Aynı kaldı)
        if (Input.GetKey(KeyCode.Space)) 
        {
            // Fren Yap
            s1.brakeTorque = frenGucu;
            s2.brakeTorque = frenGucu;
            w1.brakeTorque = frenGucu; 
            w2.brakeTorque = frenGucu;

            s1.motorTorque = 0;
            s2.motorTorque = 0;
        }
        else 
        {
            // Gaz Ver
            s1.brakeTorque = 0;
            s2.brakeTorque = 0;
            w1.brakeTorque = 0;
            w2.brakeTorque = 0;

            s1.motorTorque = motor;
            s2.motorTorque = motor;
        }

        // 3. Tekerlek Animasyonları (Aynı kaldı)
        TekerlegiDondur(w1, w1Mesh);
        TekerlegiDondur(w2, w2Mesh);
        TekerlegiDondur(s1, s1Mesh);
        TekerlegiDondur(s2, s2Mesh);
        
        // 4. GÖRSEL DİREKSİYON ROTASYONU // 👈 YENİ MANIIK
        HandleSteeringWheelRotation(yatayGiris);
    }

    // --- YENİ FONKSİYON: DİREKSİYONU GÖRSEL OLARAK DÖNDÜRÜR ---
    void HandleSteeringWheelRotation(float input)
    {
        if (steeringWheel == null) return;

        // Hedef Açıyı hesapla (input'a göre maksimum açı içinde kal)
        float targetAngle = input * maxSteeringAngle;
        
        // Mevcut açıyı hedefe doğru yumuşakça (Lerp) döndür
        currentVisualSteeringAngle = Mathf.Lerp(
            currentVisualSteeringAngle, 
            targetAngle, 
            Time.deltaTime * 10f // Hızlı bir takip hızı
        );

        // Eğer input yoksa, direksiyonu merkeze döndür
        if (Mathf.Abs(input) < 0.01f)
        {
            currentVisualSteeringAngle = Mathf.Lerp(
                currentVisualSteeringAngle, 
                0f, 
                Time.deltaTime * steeringReturnSpeed
            );
        }

        // Direksiyonu yerel Z ekseninde döndür (modelinize göre bu eksen değişebilir!)
        steeringWheel.localRotation = Quaternion.Euler(
            steeringWheel.localRotation.eulerAngles.x, 
            steeringWheel.localRotation.eulerAngles.y, 
            -currentVisualSteeringAngle // Eksi (-) koyarak dönüş yönünü ayarlayabilirsiniz
        );
    }

    void TekerlegiDondur(WheelCollider collider, Transform mesh)
    {
        if (mesh == null) return; 

        Vector3 pos;
        Quaternion rot;

        collider.GetWorldPose(out pos, out rot);

        mesh.position = pos;
        mesh.rotation = rot;
    }
}