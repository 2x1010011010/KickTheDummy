using UnityEngine;
using System.Collections;
using Game.InteractiveSystem;

public class destruction : SimpleDamageable {

    [SerializeField] private InteractableObject interactableObject;

	public GameObject[] Chunks;
	public GameObject[] HidingObjs; //list of the objects that will be hidden after the crush.
	public AudioSource audioSource; //list of the objects that will be hidden after the crush.
	[Range(1,100)]
	public int Health = 100;
	public float ExplosionForce = 200; //force added to every chunk of the broken object.
	public float ChunksRotation = 20; //rotation force added to every chunk when it explodes.
	public float strength = 5; //How easily the object brokes.
	public bool BreakByClick = false;
	public bool DestroyAftertime = true; //if true, then chunks will be destroyed after time.
	public float time = 15; //time before chunks will be destroyed from the scene.
	public GameObject FX;
	public bool AutoDestroy = true; //if true, then object will be automatically break after after "AutoDestTime" since game start.
	public float AutoDestTime = 2; //Auto destruction time (counts from game start).

	void Start () {

		if(AutoDestroy){
			Invoke("Crushing", AutoDestTime);
		}

		if(HidingObjs.Length !=0){
			foreach(GameObject hidingObj in HidingObjs){
				hidingObj.SetActive(true);
			}
		}
	}

    protected override void DamageTaken(Damage damage, IDamageable damageable)
    {
        if (damage.DamageType != DamageType.Explosion) return;

        Health -= (int)damage.DamageCount;

        if (Health <= 0)
        {
            Crushing();
        }
    }

    void OnCollisionEnter(Collision other){
		if(other.gameObject.GetComponent<BulletsDamage>()){
		Health -= other.gameObject.GetComponent<BulletsDamage>().Damage;
			if(Health <= 0){
				Crushing();
				}
		}
		else if(other.relativeVelocity.magnitude > strength){
			Crushing();
		}

	}
	void OnMouseDown(){
		if(BreakByClick){
			Crushing();
			BreakByClick = false;
		}
		}

	void Crushing(){
		if(HidingObjs.Length !=0){
			foreach(GameObject hidingObj in HidingObjs){
				hidingObj.SetActive(false);
			}
		}
		if(FX){
			FX.SetActive(true);
		}

        audioSource.Play();

        interactableObject.StopInteract();
        interactableObject.GetInteractive<IExplodeable>()?.Explode();

        foreach (GameObject chunk in Chunks){
			chunk.SetActive(true);
			chunk.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * -ExplosionForce);
			chunk.GetComponent<Rigidbody>().AddRelativeTorque(Vector3.forward * -ChunksRotation*Random.Range(-5f, 5f));
			chunk.GetComponent<Rigidbody>().AddRelativeTorque(Vector3.right * -ChunksRotation*Random.Range(-5f, 5f));
		}
		if(DestroyAftertime){
			Invoke("DestructObject", time);
		}
	}

	void DestructObject(){
		Destroy(gameObject);
	}

}
