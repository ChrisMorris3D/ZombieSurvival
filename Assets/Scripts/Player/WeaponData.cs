using UnityEngine;

public enum WeaponFireMode
{
    SemiAutomatic,
    Automatic
}

[CreateAssetMenu(fileName = "Weapon_New", menuName = "Zombie Survival/Weapons/Weapon Data")]
public sealed class WeaponData : ScriptableObject
{
    [Header("IDENTITY")]
    [SerializeField] string displayName = "New Weapon";

    [Header("FIRING")]
    [SerializeField] WeaponFireMode fireMode = WeaponFireMode.SemiAutomatic;
    [SerializeField, Min(0f)] float damage = 1f;
    [SerializeField, Min(0.01f)] float shotsPerSecond = 1f;
    [SerializeField, Min(0f)] float range = 100f;
    [SerializeField, Min(1)] int pelletsPerShot = 1;
    [SerializeField, Range(0f, 45f)] float spreadAngle;
    [SerializeField] LayerMask hitMask = ~0;

    [Header("AMMUNITION")]
    [SerializeField, Min(1)] int magazineSize = 12;
    [SerializeField, Min(1)] int ammunitionPerShot = 1;
    [SerializeField, Min(0f)] float reloadDuration = 1.5f;
    [SerializeField] bool autoReloadWhenEmpty = true;

    public string DisplayName => displayName;
    public WeaponFireMode FireMode => fireMode;
    public float Damage => damage;
    public float ShotsPerSecond => shotsPerSecond;
    public float Range => range;
    public int PelletsPerShot => pelletsPerShot;
    public float SpreadAngle => spreadAngle;
    public LayerMask HitMask => hitMask;
    public int MagazineSize => magazineSize;
    public int AmmunitionPerShot => ammunitionPerShot;
    public float ReloadDuration => reloadDuration;
    public bool AutoReloadWhenEmpty => autoReloadWhenEmpty;

    void OnValidate()
    {
        damage = Mathf.Max(0f, damage);
        shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
        range = Mathf.Max(0f, range);
        pelletsPerShot = Mathf.Max(1, pelletsPerShot);
        spreadAngle = Mathf.Clamp(spreadAngle, 0f, 45f);
        magazineSize = Mathf.Max(1, magazineSize);
        ammunitionPerShot = Mathf.Clamp(ammunitionPerShot, 1, magazineSize);
        reloadDuration = Mathf.Max(0f, reloadDuration);
    }
}
