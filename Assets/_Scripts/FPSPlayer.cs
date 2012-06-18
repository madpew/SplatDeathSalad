using UnityEngine;
using System.Collections;

public class FPSPlayer {

	public NetworkViewID viewID;
	
	public NetworkPlayer netPlayer;
	
	public bool local;
	
	public string name = "";
	public Color colA;
	public Color colB;
	public Color colC;
	public int headType;
	
	
	public FPSEntity fpsEntity = null;
	
	public int kills;
	public int totalKills;
	public int deaths;
	public int totalDeaths;
	
	public int currentScore;
	
	public Ping ping;
	
	public float health = 100f;
	
	public float respawnTime = 0f;
	
	public int team = 0;
	
	public int lives = 0;
	
	public bool hasBall = false;
	
	public void InstantiateFPSEntity(GameObject fpsEntityPrefab){
		
		//instantiate an entity, duh
		if (fpsEntity != null) return;
		
		GameObject newEntity = (GameObject)GameObject.Instantiate(fpsEntityPrefab);
		fpsEntity = newEntity.GetComponent<FPSEntity>();
		
		fpsEntity.colA = colA;
		fpsEntity.colB = colB;
		fpsEntity.colC = colC;
		
		fpsEntity.headType = headType;
		
		//fpsEntity.viewID = viewID;
		fpsEntity.isLocal = local;
		
		fpsEntity.thisPlayer = this;
		
		
		if (lives<0 && local){
			currentScore = -99;
			fpsEntity.spectateMode = true;
			Debug.Log("Spectate yo");
		}
	}
	
}
