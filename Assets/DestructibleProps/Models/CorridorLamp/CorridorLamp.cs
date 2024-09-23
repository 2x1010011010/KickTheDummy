using UnityEngine;
using System.Collections;

public class CorridorLamp : MonoBehaviour {
	public GameObject NormalMesh;
	public Mesh BrokenMesh;
	public GameObject FX;
	private bool Crushed = false;
	public string[] excludedTags = new string[3]{"Untagged", "Player", "GameController"};//tags that will be excluded from collision.
	// Use this for initialization
	void Start () {

		if(GetComponent<AudioSource>()){
			GetComponent<AudioSource>().pitch = Random.Range (0.8f, 1.2f);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnCollisionEnter(Collision other){
		if(System.Array.IndexOf(excludedTags, other.gameObject.tag)	== -1 && !Crushed){
			Crushed = true;
			GetComponent<Animation>().Play();
			if(GetComponent<AudioSource>()){
			GetComponent<AudioSource>().Play();
			}
			FX.GetComponent<ParticleSystem>().enableEmission = true;
			FX.GetComponent<ParticleSystem>().Play();
			if(NormalMesh.GetComponent<SkinnedMeshRenderer>()){
				NormalMesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = BrokenMesh;
			}
			else{
				NormalMesh.GetComponent<MeshFilter>().sharedMesh = BrokenMesh;
				FX.GetComponent<ParticleSystem>().enableEmission = true;
				FX.GetComponent<ParticleSystem>().Play();
				GetComponent<Animation>().Play();
				if(GetComponent<AudioSource>()){
					GetComponent<AudioSource>().Play();
				}
			}
		}
	}
}
