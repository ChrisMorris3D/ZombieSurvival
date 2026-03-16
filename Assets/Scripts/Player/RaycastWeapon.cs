using CrispyCube;
using UnityEngine;

[DisallowMultipleComponent]
public class RaycastWeapon : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] Camera weaponCamera;
    [SerializeField] ParticleSystem[] shootEffects;

    [Header("SHOOTING")]
    [SerializeField] float range = 100f;
    [SerializeField] LayerMask hitMask = ~0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        PlayShootEffects();

        if (weaponCamera == null)
        {
            return;
        }

        Ray ray = new Ray(weaponCamera.transform.position, weaponCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        EnemyBase enemy = hit.collider.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage();
        }
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
        }
    }

    void OnValidate()
    {
        range = Mathf.Max(0f, range);
    }

    void Reset()
    {
        weaponCamera = Camera.main;
    }
}
