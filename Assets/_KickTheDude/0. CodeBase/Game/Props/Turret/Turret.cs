using Game.DamageSystem;
using Game.InteractiveSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class Turret : IInteractive<IInteractable>, IUpdateable
{
    public string Name => "SENTRY GUN";
    
    [SerializeField, BoxGroup("SETUP")] private SimpleRotator _barrelRotator;
    [SerializeField, BoxGroup("SETUP")] private HealthContainer _health;
    [SerializeField, BoxGroup("SETUP")] private TurretRotator _turretRotator;
    [SerializeField, BoxGroup("SETUP")] private TurretSensor _sensor;
    [SerializeField, BoxGroup("SETUP")] private TurretShooter _shooter;
    
    [SerializeField, BoxGroup("TURRET VFX")] private GameObject _muzzleFlame;

    private bool _isBroken;
    private bool _isTargetFound;
    private Transform _target;
    private float _startYRotation;

    public IInteractable Interactable { get; private set; }

   public void Init(IInteractable initData)
   {
       Interactable = initData;
       _turretRotator.Init();
       _sensor.OnTargetDetected += OnTargetDetected;
       _sensor.OnDetectionLose += OnTargetLose;
       _health.OnHealthEnded += OnHealthEnded;
   }

   public void Dispose()
   {
       throw new System.NotImplementedException();
   }
   public void StopInteract()
   {
       throw new System.NotImplementedException();
   }

   public void Update()
   {
       if (_isBroken) return;
       
       if (!_isTargetFound)
       {
           _turretRotator.StartRotation();
           return;
       }
       
       _turretRotator.StopRotation();
       _turretRotator.TurnTurretToTarget(_target);
   }

   private void OnTargetDetected(Transform target)
   {
       _target = target;
       _isTargetFound = true;
       _shooter.StartUse(_target);
       _muzzleFlame.SetActive(true);
   }

   private void OnTargetLose()
   {
       _target = null;
       _isTargetFound = false;
       _shooter.StopUse();
       _muzzleFlame.SetActive(false);
   }

   private void OnHealthEnded(HealthContainer healthContainer, float healthCount)
   {
       _isBroken = true;
       _barrelRotator.StopRotate();
       _sensor.OnTargetDetected -= OnTargetDetected;
       _sensor.OnDetectionLose -= OnTargetLose;
       _health.OnHealthEnded -= OnHealthEnded;
   }

}
