using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Destructible : MonoBehaviour, IPreDamageHandler, IDamageHandler
{
    float direction = 0;
    bool destroyed = false;
    Vector3 eulerAngles;
    Vector3 position;

    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;
    [SerializeField] private float corpseAngle;

    private Transform spriteTransform;
    private ParticleSystem ps;

    void Start()
    {
        spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
        ps = GetComponentInChildren<ParticleSystem>(true);
    }

    public void OnPreDamage(DamageInfo info)
    {
        info.Recoil = false;
    }

    public void OnDamage(DamageInfo info)
    {
        if (destroyed)
            return;

        destroyed = true;
        ps.gameObject.SetActive(true);
        spriteTransform.gameObject.SetActive(false);
    }
}
