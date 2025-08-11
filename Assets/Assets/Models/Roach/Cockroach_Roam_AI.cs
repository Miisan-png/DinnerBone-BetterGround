using UnityEngine;

public class Cockroach_Roam_AI : MonoBehaviour
{
    public float walkSpeed = 1f;
    public float turnSpeed = 180f;
    public float walkTime = 2f;
    public float idleTime = 1f;
    public float roamRadius = 5f;
    public Transform roamCenter;
    public Vector3 facingOffset = new Vector3(0, 0, 0); // Adjust Y here if model is rotated in prefab

    Animator anim;
    Vector3 targetPos;
    float timer;
    bool isWalking;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (!roamCenter) roamCenter = transform;
        Idle();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (isWalking)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir) * Quaternion.Euler(facingOffset);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, turnSpeed * Time.deltaTime);
            }
            if (Vector3.Distance(transform.position, targetPos) > 0.05f)
                transform.position += (transform.rotation * Vector3.forward) * walkSpeed * Time.deltaTime;
            if (timer <= 0 || Vector3.Distance(transform.position, targetPos) < 0.05f) Idle();
        }
        else if (timer <= 0) Walk();
    }

    void Walk()
    {
        isWalking = true;
        anim.SetTrigger("Walk");
        timer = walkTime;
        Vector2 rnd = Random.insideUnitCircle * roamRadius;
        targetPos = roamCenter.position + new Vector3(rnd.x, 0, rnd.y);
    }

    void Idle()
    {
        isWalking = false;
        anim.SetTrigger("Idle");
        timer = idleTime;
    }
}
