using UnityEngine;

public class CarLookHandler : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform cameraRig; // Kameranın bağlı olduğu Transform
    
    [Header("Ayarlar")]
    public float mouseSensitivity = 100f;
    public float lookXLimit = 45f; // Dikey bakış limiti
    public float lookYLimit = 90f; // Yatay bakış limiti

    private float xRotation = 0f; // Dikey kamera dönüş açısı
    private float yRotationInCar = 0f; // Yatay kamera dönüş açısı

    void Start()
    {
        // Başlangıç dönüşünü al (Zaten FreeRoamController'dan ayarlanmıştır)
        if (cameraRig != null)
        {
            xRotation = cameraRig.localEulerAngles.x;
            if (xRotation > 180) xRotation -= 360;
            yRotationInCar = cameraRig.localEulerAngles.y;
            if (yRotationInCar > 180) yRotationInCar -= 360;
        }
        
        // Başlangıçta bu script devre dışı olmalı
        this.enabled = false; 
    }

    void Update()
    {
        // Fare girdilerini al
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Dikey Dönüş (Kafa yukarı/aşağı)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookXLimit, lookXLimit); 

        // Yatay Dönüş (Kafa sağa/sola)
        yRotationInCar += mouseX;
        yRotationInCar = Mathf.Clamp(yRotationInCar, -lookYLimit, lookYLimit); 

        if(cameraRig != null)
        {
            // Yalnızca kamerayı (cameraRig) döndür. Karakterin ana objesi dönmeyecek.
            cameraRig.localRotation = Quaternion.Euler(xRotation, yRotationInCar, 0f);
        }
        
        // ÖNEMLİ: Karakterin ana gövdesini (transform.Rotate) DÖNDÜRMÜYORUZ.
    }
    
    // Yaya moduna geçerken bakış açılarını sıfırlamak için
    public void ResetLook()
    {
        yRotationInCar = 0f;
        // Dikey açıyı koru
        if (cameraRig != null) cameraRig.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}