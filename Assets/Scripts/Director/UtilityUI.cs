using UnityEngine;
using UnityEngine.UI;
using ZombieFPS;

public class UtilityUI : MonoBehaviour {
    
    [SerializeField] MyDirector _aiController;
    
    [SerializeField] private Image playerHealthImage;
    [SerializeField] private Image playerAmmoImage;
    [SerializeField] private Image playerProgressionImage;
    [SerializeField] private Image playerGrenadeImage;
    [SerializeField] private Image enemyCountImage;
    [SerializeField] private Image enemyPressureImage;
    [SerializeField] private Image timeSinceCombatImage;
    [SerializeField] private Image damageTakenRecentImage;
    [SerializeField] private Image tankSpawnTimerImage;
    
    private void Update() {
        playerAmmoImage.fillAmount = _aiController.Context["PlayerAmmo"];
        playerProgressionImage.fillAmount = _aiController.Context["PlayerProgression"];
        playerGrenadeImage.fillAmount = _aiController.Context["PlayerGrenades"];
        enemyCountImage.fillAmount = _aiController.Context["EnemyCount"];
        enemyPressureImage.fillAmount = _aiController.Context["EnemyPressure"];
        timeSinceCombatImage.fillAmount = _aiController.Context["TimeSinceCombat"];
        damageTakenRecentImage.fillAmount = _aiController.Context["DamageTakenRecent"];
        tankSpawnTimerImage.fillAmount = _aiController.Context["TankSpawnTimer"];
        playerHealthImage.fillAmount = _aiController.Context["PlayerHealth"];
    }
}
