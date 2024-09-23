using UnityEngine;
using System.Collections;
using System;

public class DeskLamp : MonoBehaviour {
	public GameObject brokenObj;
	public GameObject[] hidingObjs;
	public string[] excludedTags = new string[3]{"Untagged", "Player", "GameController"};//tags that will be excluded from collision.
	// Use this for initialization
	void Start () {
		brokenObj.SetActive(false);
		if(GetComponent<AudioSource>()){
			brokenObj.GetComponent<AudioSource>().pitch = UnityEngine.Random.Range (0.7f, 1.1f);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnCollisionEnter(Collision other){
		if(System.Array.IndexOf(excludedTags, other.gameObject.tag)	== -1){
			foreach(GameObject hidingObj in hidingObjs){
			hidingObj.SetActive(false);
			}
			GetComponent<Renderer>().enabled = false;
			GetComponent<Collider>().enabled = false;
			brokenObj.SetActive(true);
			brokenObj.GetComponent<Animation>().Play();
		}
	}
}
