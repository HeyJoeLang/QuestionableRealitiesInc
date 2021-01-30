using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class lb_BirdController : MonoBehaviour {
	public int idealNumberOfBirds;
	public int maximumNumberOfBirds;
	public Camera currentCamera;
	public float unspawnDistance = 10.0f;
	public bool highQuality = true;
	public bool collideWithObjects = true;
	public LayerMask groundLayer;
	public float birdScale = 1.0f;

	public bool robin = true;
	public bool blueJay = true;
	public bool cardinal = true;
	public bool chickadee = true;
	public bool sparrow = true;
	public bool goldFinch = true;
	public bool crow = true;

	bool pause = false;
	GameObject[] myBirds;
	List<string> myBirdTypes = new List<string>();
	List<GameObject>  birdGroundTargets = new List<GameObject>();
	List<GameObject> birdPerchTargets = new List<GameObject>();
	int activeBirds = 0;
	int birdIndex = 0;
	GameObject[] featherEmitters = new GameObject[3];
	public Transform[] spawnTransforms;

	public void AllFlee(){
		if(!pause){
			for(int i=0;i<myBirds.Length;i++){
				if(myBirds[i].activeSelf){
					myBirds[i].SendMessage ("Flee");
				}
			}
		}
	}
	
	public void Pause(){
		if(pause){
			AllUnPause ();
		}else{
			AllPause ();
		}
	}
	
	public void AllPause(){
		pause = true;
		for(int i=0;i<myBirds.Length;i++){
			if(myBirds[i].activeSelf){
				myBirds[i].SendMessage ("PauseBird");
			}
		}
	}
	
	public void AllUnPause(){
		pause = false;
		for(int i=0;i<myBirds.Length;i++){
			if(myBirds[i].activeSelf){
				myBirds[i].SendMessage ("UnPauseBird");
			}
		}
	}

	public void SpawnAmount(int amt){
		for(int i=0;i<=amt;i++){
			SpawnBird ();
		}
	}

	public void ChangeCamera(Camera cam){
		currentCamera = cam;
	}

	void Start () {
		//find the camera
		if (currentCamera == null){
			currentCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		}

		if(idealNumberOfBirds >= maximumNumberOfBirds){
			idealNumberOfBirds = maximumNumberOfBirds-1;
		}
		//set up the bird types to use
		if(robin){
			myBirdTypes.Add ("lb_robin");
		}
		if (blueJay){
			myBirdTypes.Add ("lb_blueJay");
		}
		if(cardinal){
			myBirdTypes.Add ("lb_cardinal");
		}
		if(chickadee){
			myBirdTypes.Add ("lb_chickadee");
		}
		if(sparrow){
			myBirdTypes.Add ("lb_sparrow");
		}
		if(goldFinch){
			myBirdTypes.Add ("lb_goldFinch");
		}
		if(crow){
			myBirdTypes.Add ("lb_crow");
		}
		//Instantiate birds based on amounts and bird types
		myBirds = new GameObject[maximumNumberOfBirds];
		GameObject bird;
		for(int i=0;i<myBirds.Length;i++){
			if(highQuality){
				bird = Resources.Load (myBirdTypes[Random.Range (0,myBirdTypes.Count)]+"HQ",typeof(GameObject)) as GameObject;
			}else{
				bird = Resources.Load (myBirdTypes[Random.Range (0,myBirdTypes.Count)],typeof(GameObject)) as GameObject;
			}
			myBirds[i] = Instantiate (bird,Vector3.zero,Quaternion.identity) as GameObject;
			myBirds[i].transform.localScale = myBirds[i].transform.localScale*birdScale;
			myBirds[i].transform.parent = transform;
			myBirds[i].SendMessage ("SetController",this);
			myBirds[i].SetActive (false);
		}

		//find all the targets
		GameObject[] groundTargets = GameObject.FindGameObjectsWithTag("lb_groundTarget");
		GameObject[] perchTargets = GameObject.FindGameObjectsWithTag("lb_perchTarget");

		for (int i=0;i<groundTargets.Length;i++){
			if(Vector3.Distance (groundTargets[i].transform.position,currentCamera.transform.position)<unspawnDistance){
				birdGroundTargets.Add(groundTargets[i]);
			}
		}
		for (int i=0;i<perchTargets.Length;i++){
			if(Vector3.Distance (perchTargets[i].transform.position,currentCamera.transform.position)<unspawnDistance){
				birdPerchTargets.Add(perchTargets[i]);
			}
		}

		//instantiate 3 feather emitters for killing the birds
		GameObject fEmitter = Resources.Load ("featherEmitter",typeof(GameObject)) as GameObject;
		for(int i=0;i<3;i++){
			featherEmitters[i] = Instantiate (fEmitter,Vector3.zero,Quaternion.identity) as GameObject;
			featherEmitters[i].transform.parent = transform;
			featherEmitters[i].SetActive (false);
		}

		SpawnAmount(idealNumberOfBirds);
	}

	void OnEnable(){
		InvokeRepeating("UpdateBirds",1,1);
		StartCoroutine("UpdateTargets");
	}

	Vector3 FindPointInGroundTarget(GameObject target){
		//find a random point within the collider of a ground target that touches the ground
		Vector3 point;
		point.x = Random.Range (target.GetComponent<Collider>().bounds.max.x,target.GetComponent<Collider>().bounds.min.x);
		point.y = target.GetComponent<Collider>().bounds.max.y;
		point.z = Random.Range (target.GetComponent<Collider>().bounds.max.z,target.GetComponent<Collider>().bounds.min.z);
		//raycast down until it hits the ground
		RaycastHit hit;
		if (Physics.Raycast (point,-Vector3.up,out hit,target.GetComponent<Collider>().bounds.size.y,groundLayer)){
			return hit.point;
		}

		return point;
	}

	void UpdateBirds(){
		//this function is called once a second
		if(activeBirds < idealNumberOfBirds  && AreThereActiveTargets()){
			//if there are less than ideal birds active, spawn a bird
			SpawnBird();
		}else if(activeBirds < maximumNumberOfBirds && Random.value < .05 && AreThereActiveTargets()){
			//if there are less than maximum birds active spawn a bird every 20 seconds
			SpawnBird();
		}

		//check one bird every second to see if it should be unspawned
		if(myBirds[birdIndex].activeSelf && BirdOffCamera (myBirds[birdIndex].transform.position) && Vector3.Distance(myBirds[birdIndex].transform.position,currentCamera.transform.position) > unspawnDistance){
			//if the bird is off camera and at least unsapwnDistance units away lets unspawn
			Unspawn(myBirds[birdIndex]);
		}

		birdIndex = birdIndex == myBirds.Length-1 ? 0:birdIndex+1;
	}

	//this function will cycle through targets removing those outside of the unspawnDistance
	//it will also add any new targets that come into range
	IEnumerator UpdateTargets(){
		List<GameObject> gtRemove = new List<GameObject>();
		List<GameObject> ptRemove = new List<GameObject>();

		while(true){
			gtRemove.Clear();
			ptRemove.Clear();
			//check targets to see if they are out of range
			for(int i=0;i<birdGroundTargets.Count;i++){
				if (Vector3.Distance (birdGroundTargets[i].transform.position,currentCamera.transform.position)>unspawnDistance){
					gtRemove.Add (birdGroundTargets[i]);
				}
				yield return 0;
			}
			for (int i=0;i<birdPerchTargets.Count;i++){
				if (Vector3.Distance (birdPerchTargets[i].transform.position,currentCamera.transform.position)>unspawnDistance){
					ptRemove.Add (birdPerchTargets[i]);
				}
				yield return 0;
			}
			//remove any targets that have been found out of range
			foreach (GameObject entry in gtRemove){
				birdGroundTargets.Remove(entry);
			}
			foreach (GameObject entry in ptRemove){
				birdPerchTargets.Remove(entry);
			}
			yield return 0;
			//now check for any new Targets
			Collider[] hits = Physics.OverlapSphere(currentCamera.transform.position,unspawnDistance);
			foreach(Collider hit in hits){
				if (hit.tag == "lb_groundTarget" && !birdGroundTargets.Contains (hit.gameObject)){
					birdGroundTargets.Add (hit.gameObject);
				}
				if (hit.tag == "lb_perchTarget" && !birdPerchTargets.Contains (hit.gameObject)){
					birdPerchTargets.Add (hit.gameObject);
				}
			}
			yield return 0;
		}
	}

	bool BirdOffCamera(Vector3 birdPos){
		Vector3 screenPos = currentCamera.WorldToViewportPoint(birdPos);
		if (screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1){
			return true;
		}else{
			return false;
		}
	}

	void Unspawn(GameObject bird){
		bird.transform.position = Vector3.zero;
		bird.SetActive (false);
		activeBirds --;
	}

	void SpawnBird(){
		if (!pause){
			GameObject bird = null;
			int randomBirdIndex = Mathf.FloorToInt (Random.Range (0,myBirds.Length));
			int loopCheck = 0;
			//find a random bird that is not active
			while(bird == null){
				if(myBirds[randomBirdIndex].activeSelf == false){
					bird = myBirds[randomBirdIndex];
				}
				randomBirdIndex = randomBirdIndex+1 >= myBirds.Length ? 0:randomBirdIndex+1;
				loopCheck ++;
				if (loopCheck >= myBirds.Length){
					//all birds are active
					return;
				}
			}
			//Find a point off camera to positon the bird and activate it
			bird.transform.position = FindPositionOffCamera();
			bird.SetActive (true);
			activeBirds++;
			BirdFindTarget(bird);
		}
	}

	bool AreThereActiveTargets(){
		return true;
	}

	public Vector3 FindPositionOffCamera(){
		return spawnTransforms[Random.Range(0, spawnTransforms.Length)].position;
	}
	
	void BirdFindTarget(GameObject bird){
		bird.SendMessage ("FlyToTarget", birdPerchTargets[Random.Range(0, birdPerchTargets.Count)].transform.position);
	}

	public void FeatherEmit(Vector3 pos){
		foreach (GameObject fEmit in featherEmitters){
			if(!fEmit.activeSelf){
				fEmit.transform.position = pos;
				fEmit.SetActive (true);
				StartCoroutine("DeactivateFeathers",fEmit);
				break;
			}
		}
	}

	IEnumerator DeactivateFeathers(GameObject featherEmit){
		yield return new WaitForSeconds(4.5f);
		featherEmit.SetActive (false);
	}
}
