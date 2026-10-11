using CrispyCube;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class WeaponController : MonoBehaviour
{
    [Header("CONFIGURATION")]
    [SerializeField] WeaponData weaponData;
    [SerializeField] IntegerVariable roundTimer;

    [Header("SHOOTING")]
    [Tooltip("Uses this transform for ray origin and direction. Falls back to this GameObject's transform.")]
    [SerializeField] Transform shotOrigin;

    [Header("SHOOT FX")]
    [SerializeField] ParticleSystem[] shootEffects;
    [SerializeField] ParticleSystem hitEffectPrefab;
    [SerializeField] AudioSource shotAudioSource;

    const float ShotGizmoFlashDuration = 0.1f;

    int currentAmmo;
    float nextShotTime;
    float reloadCompleteTime;
    float lastShotTime = float.NegativeInfinity;
    bool triggerHeld;
    bool triggerPressedThisFrame;
    bool isReloading;

    public WeaponData Data => weaponData;
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;

    bool IsRoundActive => roundTimer == null || roundTimer.Value > 0;
    Transform ShotOrigin => shotOrigin != null ? shotOrigin : transform;

    void Awake()
    {
        FillMagazine();
    }

    void Update()
    {
        UpdateReload();

        if (weaponData != null && IsRoundActive && !isReloading)
        {
            bool wantsToFire = weaponData.FireMode == WeaponFireMode.Automatic
                ? triggerHeld
                : triggerPressedThisFrame;

            if (wantsToFire)
            {
                TryShoot();
            }
        }

        triggerPressedThisFrame = false;
    }

    public void TriggerPressed()
    {
        triggerHeld = true;
        triggerPressedThisFrame = true;
    }

    public void TriggerReleased()
    {
        triggerHeld = false;
    }

    public bool TryShoot()
    {
        if (weaponData == null || !IsRoundActive || isReloading || Time.time < nextShotTime)
        {
            return false;
        }

        if (currentAmmo < weaponData.AmmunitionPerShot)
        {
            if (weaponData.AutoReloadWhenEmpty)
            {
                TryReload();
            }

            return false;
        }

        currentAmmo -= weaponData.AmmunitionPerShot;
        nextShotTime = Time.time + 1f / weaponData.ShotsPerSecond;
        lastShotTime = Time.time;

        PlayShootEffects();
        PlayShotAudio();

        for (int i = 0; i < weaponData.PelletsPerShot; i++)
        {
            FireRay();
        }

        if (currentAmmo < weaponData.AmmunitionPerShot && weaponData.AutoReloadWhenEmpty)
        {
            TryReload();
        }

        return true;
    }

    public bool TryReload()
    {
        if (weaponData == null || isReloading || currentAmmo >= weaponData.MagazineSize)
        {
            return false;
        }

        if (weaponData.ReloadDuration <= 0f)
        {
            FillMagazine();
            return true;
        }

        isReloading = true;
        reloadCompleteTime = Time.time + weaponData.ReloadDuration;
        return true;
    }

    public void SetWeaponData(WeaponData newWeaponData, bool fillMagazine = true)
    {
        weaponData = newWeaponData;
        isReloading = false;
        nextShotTime = 0f;

        if (fillMagazine)
        {
            FillMagazine();
        }
        else if (weaponData != null)
        {
            currentAmmo = Mathf.Min(currentAmmo, weaponData.MagazineSize);
        }
        else
        {
            currentAmmo = 0;
        }
    }

    void FireRay()
    {
        Transform origin = ShotOrigin;
        Vector3 direction = GetShotDirection(origin);
        Ray ray = new Ray(origin.position, direction);

        if (!Physics.Raycast(ray, out RaycastHit hit, weaponData.Range, weaponData.HitMask, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        PlayHitEffect(hit);

        EnemyHealthController enemyHealth = hit.collider.GetComponentInParent<EnemyHealthController>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(weaponData.Damage);
        }
    }

    Vector3 GetShotDirection(Transform origin)
    {
        if (weaponData.SpreadAngle <= 0f)
        {
            return origin.forward;
        }

        Vector2 spread = Random.insideUnitCircle * Mathf.Tan(weaponData.SpreadAngle * Mathf.Deg2Rad);
        return (origin.forward + origin.right * spread.x + origin.up * spread.y).normalized;
    }

    void UpdateReload()
    {
        if (!isReloading || Time.time < reloadCompleteTime)
        {
            return;
        }

        isReloading = false;
        FillMagazine();
    }

    void FillMagazine()
    {
        currentAmmo = weaponData != null ? weaponData.MagazineSize : 0;
    }

    void PlayShootEffects()
    {
        if (shootEffects == null)
        {
            return;
        }

        for (int i = 0; i < shootEffects.Length; i++)
        {
            if (shootEffects[i] != null)
            {
                shootEffects[i].Play();
            }
        }
    }

    void PlayShotAudio()
    {
        if (shotAudioSource != null)
        {
            shotAudioSource.Play();
        }
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

    void OnDrawGizmosSelected()
    {
        if (weaponData == null)
        {
            return;
        }

        Transform origin = ShotOrigin;
        bool recentlyFired = Application.isPlaying && Time.time - lastShotTime <= ShotGizmoFlashDuration;
        Gizmos.color = recentlyFired ? Color.yellow : Color.red;
        Gizmos.DrawLine(origin.position, origin.position + origin.forward * weaponData.Range);
    }
}
