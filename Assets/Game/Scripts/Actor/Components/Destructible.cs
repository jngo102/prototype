using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Destructible : MonoBehaviour, IDamageHandler
{
    float direction = 0;
    bool destroyed = false;
    Vector3 eulerAngles;
    Vector3 position;

    [SerializeField]
    float speed;

    [SerializeField]
    float rotationSpeed;

    [SerializeField]
    private float corpseAngle;

    Transform spriteTransform;

    void Start()
    {
        spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
        eulerAngles = spriteTransform.eulerAngles;
        position = transform.position;
    }

    public void OnDamage(DamageInfo info)
    {
        direction = Mathf.Sign(transform.position.x - FindObjectOfType<Player>().transform.position.x);
        destroyed = true;
        corpseAngle = Random.Range(30, 60);
    }

    void Update()
    {
        if (destroyed)
        {
            eulerAngles.z += -direction * rotationSpeed * Time.deltaTime;
            spriteTransform.eulerAngles = eulerAngles;
            position.x += Mathf.Cos(corpseAngle * Mathf.Deg2Rad) * speed * Time.deltaTime * direction;
            position.y += Mathf.Sin(corpseAngle * Mathf.Deg2Rad) * speed * Time.deltaTime;
            transform.position = position;
        }
    }
}
