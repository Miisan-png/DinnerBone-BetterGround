using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpiderDeathZone : MonoBehaviour
{
    [SerializeField] bool drawGizmo = true;
    [SerializeField] Color gizmoColor = new Color(1f, 0f, 0f, 0.35f);

    Collider col;

    void Reset()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmo) return;
        var c = GetComponent<Collider>();
        if (!c) return;

        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (c is BoxCollider b)
            Gizmos.DrawCube(b.center, b.size);
        else if (c is SphereCollider s)
            Gizmos.DrawSphere(s.center, s.radius);
        else if (c is CapsuleCollider cp)
            Gizmos.DrawCube(cp.center, new Vector3(cp.radius * 2f, cp.height, cp.radius * 2f));
    }
}
