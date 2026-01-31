using UnityEngine;

public class MouseAimController : MonoBehaviour
{
    [Header("Atamalar")]
    public Transform playerBody; // Karakterin ana gövdesi (Capsule veya Player)
    public Transform aimTarget;  // Oluşturduğun "AimTarget" küresi
    public LayerMask groundLayer; // Zemin layer'ı (Raycast için)

    [Header("Ayarlar")]
    public float bodyTurnSpeed = 10f; // Gövdenin dönme hızı
    public float targetHeight = 1.6f; // Hedefin yerden yüksekliği (Kafa hizası)

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 1. Mouse'un ekrandaki yerinden dünyaya ışın at
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 hitPoint = hit.point;

            // 2. HEDEFİ TAŞIMA (KAFA İÇİN)
            // Hedef objeyi (AimTarget) mouse'un vurduğu yere götür.
            // Ama Yüksekliğini (Y) sabit tutuyoruz ki kafa yere yapışmasın, hep boyun hizasında kalsın.
            // Eğer "kafa tam mouse'a baksın, yere de baksın" istersen alttaki satırı değiştirip direkt hitPoint yapabilirsin.
            Vector3 targetPos = hitPoint;
            targetPos.y = playerBody.position.y + targetHeight; 
            
            aimTarget.position = targetPos;

            // 3. GÖVDEYİ DÖNDÜRME (SAĞA/SOLA)
            // Gövde sadece Y ekseninde (sağa sola) dönsün, yukarı aşağı eğilmesin.
            Vector3 lookDirection = hitPoint - playerBody.position;
            lookDirection.y = 0; // Yükseklik farkını sıfırla

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetRotation, bodyTurnSpeed * Time.deltaTime);
            }
        }
    }
}