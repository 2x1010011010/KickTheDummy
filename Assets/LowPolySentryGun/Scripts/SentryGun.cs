using UnityEngine;

public class SentryGun : MonoBehaviour {
    private AudioSource[] m_AudioSources;

    [Header("References")]
    [SerializeField] private Transform m_HorizontalAxisTransform;
    [SerializeField] private Transform m_VerticalAxisTransform;
    [SerializeField] private Transform m_ShootPoint;
    [SerializeField] private Animator m_BarrelAnimator;
    [SerializeField] private AudioClip m_FireSound;

    [Header("Sentry Gun Settings")]
    [SerializeField] private float m_FireRate = 0.05f;
    [SerializeField] private float m_Damage = 10;
    [SerializeField] private float m_Range = 30f;
    [SerializeField] private float m_RotatingSpeed = 10;
    private bool m_FireLock = false;
    private int m_AudioIndex = 0;

    void Start() {
        m_AudioSources = GetComponents<AudioSource>();
    }

    void Update() {
        if (DemoEnemySpawner.SpawnedEnemies == null) {
            return;
        }

        DemoEnemy[] enemies = DemoEnemySpawner.SpawnedEnemies;
        DemoEnemy closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach(DemoEnemy enemy in enemies) {
            float distanceFromEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceFromEnemy < minDistance) {
                closestEnemy = enemy;
                minDistance = distanceFromEnemy;
            }
        }

        if (closestEnemy) {
            TrackAndFire(closestEnemy);
        }
    }

    void TrackAndFire(DemoEnemy targetEnemy) {
        Transform targetTransform = targetEnemy.transform;

        // Rotate Turret
        Vector3 directionToTarget = targetTransform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

        m_HorizontalAxisTransform.rotation = Quaternion.Lerp(m_HorizontalAxisTransform.rotation, Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), Time.deltaTime * m_RotatingSpeed);
        m_VerticalAxisTransform.localRotation = Quaternion.Lerp(m_VerticalAxisTransform.localRotation, Quaternion.Euler(lookRotation.eulerAngles.x, 0, 0), Time.deltaTime * m_RotatingSpeed);

        // Shoot if it's in range or it can be fire
        if (Vector3.Distance(transform.position, targetTransform.position) > m_Range || m_FireLock == true) {
            return;
        }

        // Shoot Enemy
        targetTransform.GetComponent<DemoEnemy>().Health -= m_Damage;

        PlaySoundSequence(m_FireSound);
        m_BarrelAnimator.CrossFadeInFixedTime("Fire", 0.01f);

        m_FireLock = true;
        Invoke("ResetFireLock", m_FireRate);
    }

    void ResetFireLock() {
        m_FireLock = false;
    }

    void PlaySoundSequence(AudioClip audioClip) {
        m_AudioSources[m_AudioIndex].clip = audioClip;
        m_AudioSources[m_AudioIndex].Play();
        
        m_AudioIndex++;

        if (m_AudioIndex >= m_AudioSources.Length) {
            m_AudioIndex = 0;
        }
    }
}
