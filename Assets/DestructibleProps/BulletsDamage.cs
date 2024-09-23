using UnityEngine;
using System.Collections;

public class BulletsDamage : MonoBehaviour {
	[Range(1,100)]
	public int Damage = 34;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnCollisionEnter(Collision other){
		Destroy (gameObject);
	}
}
