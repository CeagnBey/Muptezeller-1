using UnityEngine;

public class CharacterAimRotation : MonoBehaviour
{
    [Header("Gerekli Objeler")]
    public Transform aimTarget; // Mouse'u takip eden o küre (Target)
    
    [Header("Ayarlar")]
    public float turnSpeed = 15f; // Dönüş hızı (Yüksek olursa anında döner)

    void Update()
    {
        if (aimTarget == null) return;

        // 1. Hedefin olduğu noktayı al
        Vector3 targetPoint = aimTarget.position;

        // 2. ÇOK ÖNEMLİ: Hedefin Yüksekliğini (Y), karakterin kendi yüksekliğine eşitle.
        // Bunu yapmazsak karakter havaya bakarken arkaya devrilmeye çalışır.
        targetPoint.y = transform.position.y;

        // 3. Karakterden Hedefe doğru bir yön vektörü oluştur
        Vector3 directionToTarget = targetPoint - transform.position;

        // 4. Eğer yön vektörü sıfır değilse (mouse tam karakterin içinde değilse) döndür
        if (directionToTarget != Vector3.zero)
        {
            // Hedefe bakacak rotasyonu hesapla
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

            // Mevcut rotasyondan hedefe doğru yumuşakça dön (Lerp/Slerp)
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        }
    }
}