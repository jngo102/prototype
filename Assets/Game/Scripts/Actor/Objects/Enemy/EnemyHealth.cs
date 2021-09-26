using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageHandler, IPostDamageHandler
{
    [SerializeField]
    private int _health;
    private Animator _animator;

    public bool IsDead => _health <= 0;

    public void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void OnDamage(DamageInfo info)
    {
        _animator.SetTrigger("Damage");
        _health -= 1;
    }

    public void OnPostDamage(DamageInfo info)
    {
        if (_health <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}