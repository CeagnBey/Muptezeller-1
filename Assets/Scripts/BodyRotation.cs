using UnityEngine;

public class BodyRotation : MonoBehaviour
{
    [Header("Atamalar")]
    public Transform aimTarget; // Mouse'u takip eden o "Target" küresi

    [Header("Ayarlar")]
    public float turnSpeed = 20f; // Dönüş hızı

    void Update()
    {
        if (aimTarget == null) return;

        // 1. Hedefin pozisyonunu al
        Vector3 targetPosition = aimTarget.position;

        // 2. Yükseklik farkını yok et (Karakter yere paralel dönsün)
        targetPosition.y = transform.position.y;

        // 3. Karakterden hedefe bir bakış vektörü oluştur
        Vector3 direction = targetPosition - transform.position;

        // 4. Eğer vektör sıfır değilse döndür
        if (direction != Vector3.zero && direction.magnitude > 0.1f) // Titremeyi önlemek için mesafe kontrolü
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            
            // Mevcut açıdan hedef açıya yumuşakça dön
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        }
    }
}