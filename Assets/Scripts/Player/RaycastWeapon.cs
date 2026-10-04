using CrispyCube;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class RaycastWeapon : MonoBehaviour
{
    [Header("ROUND")]
    [SerializeField] IntegerVariable roundTimer;

    [Header("SHOOT FX")]
    [SerializeField] ParticleSystem[] shootEffects;
    [SerializeField] ParticleSystem hitEffectPrefab;
    [SerializeField] AudioSource shotAudioSource;

    [Header("SHOOTING")]
    [SerializeField] float range = 100f;
    [SerializeField] float shotsPerSecond = 10f;
    [SerializeField] float damage = 1f;
    [SerializeField] LayerMask hitMask = ~0;

    const float ShotGizmoFlashDuration = 0.1f;

    float nextShotTime;
    float lastShotTime = float.NegativeInfinity;

    bool CanFire => roundTimer != null && roundTimer.Value > 0;

    protected virtual void Update()
    {
        if (CanFire && IsFireInputActive())
        {
            Shoot();
        }
    }

    protected abstract bool IsFireInputActive();

    public void Shoot()
    {
        if (!CanFire || Time.time < nextShotTime)
        {
            return;
        }

        nextShotTime = Time.time + 1f / shotsPerSecond;
        lastShotTime = Time.time;
        PlayShootEffects();
        PlayShotAudio();
        OnShotFired();

        Ray ray = new Ray(transform.position, transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        PlayHitEffect(hit);

        EnemyHealthController enemyHealth = hit.collider.GetComponentInParent<EnemyHealthController>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }

    protected virtual void OnShotFired()
    {
    }

    void PlayShootEffects()
    {
        if (shootEffects == null)
        {
            return;
        }

        for (int i = 0; i < shootEffects.Length; i++)
        {
            ParticleSystem shootEffect = shootEffects[i];
            if (shootEffect == null)
            {
                continue;
            }

            shootEffect.Play();
            Debug.Log("Play fx");
        }
    }

    void PlayShotAudio()
    {
        if (shotAudioSource == null || shotAudioSource.resource == null)
        {
            return;
        }

        Debug.Log("Play sound fx");

        shotAudioSource.Play();
    }

    void PlayHitEffect(RaycastHit hit)
    {
        if (hitEffectPrefab == null)
        {
            return;
        }

        ParticleSystem hitEffect = Instantiate(
            hitEffectPrefab,
            hit.point,
            Quaternion.LookRotation(hit.normal));

        hitEffect.Play(true);

        float effectLifetime = 0f;
        ParticleSystem[] particleSystems = hitEffect.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem.MainModule main = particleSystems[i].main;
            effectLifetime = Mathf.Max(effectLifetime, main.duration + main.startLifetime.constantMax);
        }

        Destroy(hitEffect.gameObject, Mathf.Max(0.1f, effectLifetime));
    }

    void OnValidate()
    {
        range = Mathf.Max(0f, range);
        shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
        damage = Mathf.Max(0f, damage);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayEnd = rayOrigin + transform.forward * range;

        bool recentlyFired = Application.isPlaying && Time.time - lastShotTime <= ShotGizmoFlashDuration;
        Gizmos.color = recentlyFired ? Color.yellow : Color.red;
        Gizmos.DrawLine(rayOrigin, rayEnd);
    }
}
