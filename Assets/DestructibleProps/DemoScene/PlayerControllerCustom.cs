using UnityEngine;
using System.Collections;

public class PlayerControllerCustom : MonoBehaviour {
	private Rigidbody PlayerController;
	public float MovingSpeed = 3;
	public float StrafeSpeed = 2;
	public KeyCode RunButton = KeyCode.LeftShift;
	public float RunMultiplier = 2;
	private float RunSpeed = 1;
	public float JumpVel = 2;
	private float Moving;
	private float Strafe;
	private bool Jump = false;
	private bool isGrounded = true;
	private RaycastHit groundHit;
	private float PlayerHighest;
	public GameObject Cam;
	private float yRot = 0;
	private float xRot = 0;
	public float HorizontalSens = 2;
	public float VerticalSens = 2;

	// Use this for initialization
	void Start () {
		PlayerController = GetComponent<Rigidbody> ();
		PlayerHighest = GetComponent<CapsuleCollider> ().height * transform.localScale.y;
	}
	
	// Update is called once per frame
	void Update () {
		//yRot += Input.GetAxis("Mouse X") * HorizontalSens;
		yRot = Input.GetAxis ("Mouse X") * HorizontalSens;
		xRot += Input.GetAxis("Mouse Y") * VerticalSens;

		xRot = Mathf.Clamp (xRot, -80, 80);

		Cam.transform.localEulerAngles = new Vector3 (-xRot, 0, 0);

		transform.Rotate (0,yRot,0);

		Moving = Input.GetAxis ("Vertical") * -MovingSpeed;
		Strafe = Input.GetAxis ("Horizontal")*-StrafeSpeed;

		RunSpeed = 1;
		if (Input.GetKey (RunButton)) {
			RunSpeed = RunMultiplier;
		}
		Jump = false;
		if(Input.GetKeyDown(KeyCode.Space)){
			Jump = true;
		}


	}
	void FixedUpdate(){
		if (Physics.Raycast (transform.position, -Vector3.up, out groundHit)) {
			if (groundHit.distance < 0.05f + PlayerHighest/2) {
				isGrounded = true;

			}
			else {
				isGrounded = false;
			}
		}
		if(isGrounded){
			if(Jump){
				PlayerController.AddForce(Vector3.up * JumpVel*100);
			}
			PlayerController.AddRelativeForce(-Strafe * RunSpeed, 0, -Moving*RunSpeed);
			PlayerController.velocity = Vector3.ClampMagnitude(PlayerController.velocity, 2*RunSpeed);
			}

	}	
}
