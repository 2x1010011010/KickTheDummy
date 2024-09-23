using UnityEngine;
using System.Collections;

public class PickingUp : MonoBehaviour {
	public KeyCode PickUpKey = KeyCode.E;
	public float PickupDistance = 2;
	public KeyCode DropKey;
	public float DropForce = 500;
	private RaycastHit hit;
	private bool objKeep = false;
	private Vector3 scale;
	private Vector3 dist;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	if (Input.GetKeyDown (PickUpKey)) {
			if(!objKeep){
			Vector3 fwd = transform.TransformDirection(Vector3.forward);
			if(Physics.Raycast(transform.position+transform.TransformDirection(Vector3.forward*0.1f), fwd, out hit)){
				if(Vector3.Distance(transform.position, hit.point)<= PickupDistance && hit.transform.tag == "Pickup"){
						objKeep = true;
						if(GetComponent<ShootDemo>()!= null){
							GetComponent<ShootDemo>().enabled = false;
						}
						scale = hit.transform.localScale;
						hit.transform.GetComponent<Rigidbody>().useGravity = false;
						hit.transform.GetComponent<Rigidbody>().isKinematic = true;
						hit.transform.parent = transform;
					}
				}

			}
			else{
				ObjDrop();
			}
		}
		if (Input.GetKeyDown(DropKey) && objKeep) {
			ObjDrop();
			hit.transform.GetComponent<Rigidbody>().AddForce(transform.TransformDirection(Vector3.forward * DropForce));
		}

}
	void ObjDrop(){
		objKeep = false;
		if(GetComponent<ShootDemo>()!= null){
			GetComponent<ShootDemo>().enabled = true;
		}
		hit.transform.GetComponent<Rigidbody>().useGravity = true;
		hit.transform.GetComponent<Rigidbody>().isKinematic = false;
		hit.transform.parent = null;
		hit.transform.localScale = scale;
	}
}
