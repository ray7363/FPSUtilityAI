using UnityEngine;
using UnityEngine.UI;
using ZombieFPS;

public class UtilityUI : MonoBehaviour {
    
    [SerializeField] MyDirector _aiController;
    
    [SerializeField] private Image playerHealthImage;
    [SerializeField] private Image playerAmmoImage;
    [SerializeField] private Image playerProgressionImage;
    
    private void Update() {
        playerHealthImage.fillAmount = _aiController.Context["PlayerHealth"];
        playerAmmoImage.fillAmount = _aiController.Context["PlayerAmmo"];
        playerProgressionImage.fillAmount = _aiController.Context["PlayerProgression"];
    }
}
