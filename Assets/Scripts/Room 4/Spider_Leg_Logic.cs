using UnityEngine;
using DG.Tweening;

public class Spider_Leg_Logic : MonoBehaviour
{
    [SerializeField] private GameObject impactVFX;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform legObject;

    [SerializeField] private float startY = 4.02f;
    [SerializeField] private float anticipateY = 1.75f;
    [SerializeField] private float anticipateDuration = 0.11f;
    [SerializeField] private float attackY = 0f;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float returnY = 4f;
    [SerializeField] private float returnDuration = 0.3f;

    [SerializeField] private Vector3 cameraShakeStrength = new Vector3(0.1f, 0.1f, 0);
    [SerializeField] private float cameraShakeDuration = 0.15f;
    [SerializeField] private float attackInterval = 2f;

    private bool isAttacking;

    void Start()
    {
        legObject.localPosition = new Vector3(legObject.localPosition.x, startY, legObject.localPosition.z);
        InvokeRepeating(nameof(TriggerAttack), 0f, attackInterval);
    }

    public void TriggerAttack()
    {
        if (isAttacking) return;
        isAttacking = true;

        legObject.DOLocalMoveY(anticipateY, anticipateDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            legObject.DOLocalMoveY(attackY, attackDuration).SetEase(Ease.InQuad).OnComplete(() =>
            {
                if (impactVFX) Instantiate(impactVFX, legObject.position, Quaternion.identity);
                if (cam) cam.transform.DOShakePosition(cameraShakeDuration, cameraShakeStrength, 10, 90, false, true);
                legObject.DOLocalMoveY(returnY, returnDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    isAttacking = false;
                });
            });
        });
    }
}
