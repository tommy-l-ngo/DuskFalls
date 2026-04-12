using UnityEngine;

[CreateAssetMenu(fileName = "AttackHitbox", menuName = "Combat/Attack Hitbox")]
public class AttackHitBox : ScriptableObject
{
    public string attackName;
    public int damage;
    public float range;
    public float width;
    public string animationTrigger;

    public enum HitboxShape { Directional, Radius }
    public HitboxShape shape;

    public void TriggerAttack(Transform origin, Vector2 facingDirection, LayerMask enemyLayer)
    {
        Debug.Log("TriggerAttack called!");
        Collider[] hits;

        if (shape == HitboxShape.Directional)
        {
            Vector3 center = origin.position + new Vector3(facingDirection.x, 0, facingDirection.y) * range;
            hits = Physics.OverlapBox(center, new Vector3(range, 1f, width), Quaternion.identity, enemyLayer);
        }
        else
        {
            hits = Physics.OverlapSphere(origin.position, range, enemyLayer);
        }

        Debug.Log("Hits found: " + hits.Length);
        foreach (Collider hit in hits)
        {
            Debug.Log("Hit: " + hit.name);
        }
    }

    public void DrawGizmo(Transform origin)
    {
        Gizmos.color = Color.red;
        if (shape == HitboxShape.Radius)
            Gizmos.DrawWireSphere(origin.position, range);
        else
            Gizmos.DrawWireCube((Vector2)origin.position + Vector2.left * range, new Vector2(range, width));
    }
}