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

			int shooterIdx = -1;
			for (int i=0; i<theNetwork.players.Count; i++){
					if (theNetwork.players[i].viewID == shooterID){
						shooterIdx = i;
					}
			}
			
			RaycastHit hitInfo = new RaycastHit();
			int layerMask = (1<<0)|(1<<8);
			
			//put Shooter on Layer2
			if (shooterIdx != -1) theNetwork.players[shooterIdx].fpsEntity.gameObject.layer = 2;
			
			Vector3 rayDirection = (transform.position - lastPos).normalized;
			if (Physics.SphereCast(lastPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos)*2, layerMask)){
				
				if (hitInfo.collider.gameObject.layer == 8 || hitInfo.collider.gameObject.layer == 0)
				{
					active = false;
					//if (theNetwork.isServer){// does this work now ?
						if (hitInfo.collider.gameObject.layer == 8)
							DetonateNow(false);
						else
							DetonateNow(true);
					//}
				}
			}
			
			//put Shooter on Layer8
			if (shooterIdx != -1) theNetwork.players[shooterIdx].fpsEntity.gameObject.layer = 8;
			lastPos = transform.position;
			
			life-= Time.deltaTime;
			if (life<=0f){
				active = false;
				if (theNetwork.isServer){
					DetonateNow(true);
				}
			}
		}
	}
	
	void DetonateNow(bool isSplash){
		if (isSplash)
			theNetwork.Detonate("rocketsplash", transform.position, shooterID, viewID);
		else	
			theNetwork.Detonate("rocket", transform.position, shooterID, viewID);
	}
}
