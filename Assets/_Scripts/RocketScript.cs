using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {
	
	public GameObject dissPrefab;
	
	private float life = 20f;
	
	public NetworkViewID viewID;
	public NetworkViewID shooterID;
	
	private SophieNetworkScript theNetwork;
	
	private Vector3 lastPos;
	
	
	
	// Use this for initialization
	void Start () {
		theNetwork = GameObject.Find("_SophieNet").GetComponent<SophieNetworkScript>();
		lastPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (active){
			transform.position += transform.forward * Time.deltaTime * 33.3f;
			
			GameObject newDiss = (GameObject)GameObject.Instantiate(dissPrefab);
			newDiss.transform.position = transform.position;
			
			RaycastHit hitInfo = new RaycastHit();
			int layerMask = (1<<0)|(1<<8);
			Vector3 rayDirection = (transform.position - lastPos).normalized;
			if (Physics.SphereCast(lastPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos), layerMask)){
				active = false;
				//Debug.Log(hitInfo.collider.gameObject.name);
				if (theNetwork.isServer){
					//detonate now
					DetonateNow();
				}
			}
			lastPos = transform.position;
			
			life-= Time.deltaTime;
			if (life<=0f){
				//detonate now
				active = false;
				if (theNetwork.isServer){
					//detonate now
					DetonateNow();
				}
			}
		}else{
			
		}
		
	}
	
	void DetonateNow(){
		theNetwork.Detonate("rocket", transform.position, shooterID, viewID);
	}
}
