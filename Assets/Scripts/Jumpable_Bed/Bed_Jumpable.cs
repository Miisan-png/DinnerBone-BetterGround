using UnityEngine;
using DG.Tweening;

public class Bed_Jumpable : MonoBehaviour
{
    [SerializeField] private float bounceForce = 15f;
    [SerializeField] private float scaleAmount = 0.7f;
    [SerializeField] private float animationDuration = 0.5f;
    
    private Vector3 originalScale;
    private Player_Controller _prevJumpOwner;
    
    void Start()
    {
        originalScale = transform.localScale;
    }
    
    void OnTriggerEnter(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller == null) return;
        
        Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * scaleAmount, originalScale.z);
        
        transform.DOScale(targetScale, animationDuration * 0.5f).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOScale(originalScale, animationDuration * 0.5f).SetEase(Ease.OutBounce);
            });
        
        Player_Movement movement = player.GetComponent<Player_Movement>();
        if (movement != null)
        {
            typeof(Player_Movement).GetField("velocity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(movement, new Vector3(0, bounceForce, 0));
        }

        // Kudo's stupid fix for trigger playing twice causing sfx to play twice

        if (_prevJumpOwner != player)
        {
            _prevJumpOwner = player;
            Util.WaitForSeconds(this, () =>
            {
                SoundManager.Instance.PlaySound(new SoundVariationizer("sfx_bed_jump", 0.1f));
                _prevJumpOwner = null;
            }, 0.1f);
        }

        
    }
}