using UnityEngine;
using UnityEngine.UI;
public class ShieldSkill : MonoBehaviour
{
    public GameObject shieldPrefab;
    public float duration = 5f;
    public PHealth playerHealth; // ???? ??? PlayerHealth
    public Image shieldImage; // ???? ??? Image
    private Color originalColor; // ????? ????? ??????

    void Start()
    {
        originalColor = shieldImage.color; // ????? ????? ?????? ??? ??? ???????
    }

    public void Activate()
    {
        playerHealth.health = 100; // ????? ????? ??? 100
        shieldImage.color = Color.green; // ????? ????? ??? ??????
        Invoke("Deactivate", duration); // ????? ??????? Deactivate
    }

    void Deactivate()
    {
        shieldImage.color = originalColor; // ????? ????? ??? ?????? ???????
    }
}