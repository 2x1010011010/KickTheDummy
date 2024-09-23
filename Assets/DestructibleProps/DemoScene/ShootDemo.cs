using UnityEngine;
using System.Collections;

public class ShootDemo : MonoBehaviour {
	public GameObject BulletPrefab01;
	public GameObject BulletPrefab02;
	public GameObject BulletPrefab03;
	private GameObject SelectedBullet;
	// Use this for initialization
	void Start () {
		SelectedBullet = BulletPrefab01;
		Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Alpha1)){
			SelectedBullet = BulletPrefab01;
		}
		if(Input.GetKeyDown(KeyCode.Alpha2)){
			SelectedBullet = BulletPrefab02;
		}
		if(Input.GetKeyDown(KeyCode.Alpha3)){
			SelectedBullet = BulletPrefab03;
		}
	if(Input.GetMouseButtonDown(0)){
			GameObject Bullet = Instantiate(SelectedBullet, Camera.main.transform.position, Camera.main.transform.rotation) as GameObject;
			if(SelectedBullet == BulletPrefab01){
				Bullet.GetComponent<Rigidbody>().AddForce(Bullet.transform.forward * 70);
			}
			if(SelectedBullet == BulletPrefab02){
				Bullet.GetComponent<Rigidbody>().AddForce(Bullet.transform.forward * 140);
			}
			if(SelectedBullet == BulletPrefab03){
				Bullet.GetComponent<Rigidbody>().AddForce(Bullet.transform.forward * 210);
			}
		}
	}
	void FixedUpdate () {
		
	}
}
