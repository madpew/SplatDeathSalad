using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SophieNetworkScript : MonoBehaviour {
	
	//change the client, change this, it should be unique or different types of clients will clash
	public string uniqueGameName = "SophiesTestGame";
	
	//scripts
	private FPSMessages messageScript;
	private FPSGUI guiScript;
	private FPSArtillery artilleryScript;
	public GameModeScript gameSettings;
	
	//networky stuff
	public bool connected = false;
	public bool isServer = false;
	
	public string gameName = "TestSession";
	public int connections = 32;
	public int listenPort = 25000;
	public string comment = "GameMode|Level";
	public string password = "";
	
	public List<FPSPlayer> players = new List<FPSPlayer>();
	
	public List<PickupPoint> pickupPoints = new List<PickupPoint>();
	
	private float nextPingTime = 0f;
	
	public NetworkViewID gameViewID;
	
	public bool serverGameChange = false;
	
	//misc stuff
	public GameObject fpsEntityPrefab;
	
	private bool preppingLevel = false;
	private bool levelLoaded = false;
	
	public int team1Score = 0;
	public int team2Score = 0;
	
	public bool gameOver = false;
	
	public GameObject basketballPrefab;
	private GameObject basketball;
	
	//personal stuff
	public FPSPlayer localPlayer;
	
	
	public bool broadcastPos = false;
	
	public bool lastGameWasTeamBased = false;
	
	private float lastRPCtime = 0f;
	private float lastServerPoketime = 0f;
	
	public bool gunBobbing = true;
	public bool autoPickup = false;
	public bool autoPickupHealth = false;
	public float gameVolume = 1f;
	
	// --------- Init stuff ---------
	void Start(){
		DontDestroyOnLoad(this);
		
		Application.targetFrameRate = 60;
		
		guiScript = GetComponent<FPSGUI>();
		messageScript = GetComponent<FPSMessages>();
		artilleryScript = GetComponent<FPSArtillery>();
		gameSettings = GetComponent<GameModeScript>();
		
		
		Application.LoadLevel("MenuScene");
	}
	
	
	//-------- Network gameplay stuff ----------
	public void SendPlayer(NetworkViewID viewID, Vector3 pos, Vector3 ang, bool crouch, Vector3 moveVec, float yMove, int gunA, int gunB){
		//send out a player's current properties to everyone, so they know where we are
		networkView.RPC("SendPlayerRPC", RPCMode.Others, viewID, pos, ang, crouch, moveVec, yMove, gunA, gunB);
	}
	
	[RPC]
	void SendPlayerRPC(NetworkViewID viewID, Vector3 pos, Vector3 ang, bool crouch, Vector3 moveVec, float yMove, int gunA, int gunB, NetworkMessageInfo info){
		//received a player's properties, let's update the local view of that player
		
		lastRPCtime = Time.time;
		
		for (int i=0; i<players.Count; i++){
			if (viewID == players[i].viewID && players[i].fpsEntity != null){
				players[i].fpsEntity.UpdatePlayer(pos, ang, crouch, moveVec, yMove, info.timestamp, gunA, gunB);
			}
		}
	}
	
	public void ConsumeHealth(NetworkViewID viewID){
		//we just used a health pack, tell the server our health is maxed out
		if (!isServer){
			networkView.RPC("ConsumeHealthRPC", RPCMode.Server, viewID);
		}else{
			ConsumeHealthRPC(viewID);
		}
	}
	[RPC]
	void ConsumeHealthRPC(NetworkViewID viewID){
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID == viewID){
				players[i].health = 100f;
			}
		}
		if (localPlayer.viewID == viewID) localPlayer.health = 100f;
	}
	
	public void Detonate(string weaponType, Vector3 position, NetworkViewID shooterID, NetworkViewID bulletID){
		//we are server and something detonated, tell everyone
		networkView.RPC("DetonateRPC", RPCMode.All, weaponType, position, shooterID, bulletID);
	}
	[RPC]
	void DetonateRPC(string weaponType, Vector3 position, NetworkViewID shooterID, NetworkViewID bulletID){
		//something detonated, let the artillery script deal with it
		
		lastRPCtime = Time.time;
		
		artilleryScript.Detonate(bulletID);
		
		if (isServer){
			//we are server, lets check to see if anyone got hurt in the detonation
			for (int i=0; i<players.Count; i++){
				if (Vector3.Distance(position, players[i].fpsEntity.transform.position) < FPSArtillery.GetDetonationRadius(weaponType) + 0.5f){
					//player in range
					
					bool skip = false;
					//ignore if on the same team as the person who fired
					if (gameSettings.teamBased && !gameSettings.allowFriendlyFire){
						int shooterIndex = -1;
						for (int k=0; k<players.Count; k++){
							if (players[k].viewID == shooterID) shooterIndex = k;
						}
						if (shooterIndex != -1 && players[i].team == players[shooterIndex].team) skip = true;
					}
					
					if (players[i].health>0f && !skip){
						
						//networkView.RPC("RegisterHitRPC", RPCMode.Server, weaponType, shooterID, players[i].viewID);
						RegisterHitRPC(weaponType, shooterID, players[i].viewID, players[i].fpsEntity.transform.position);
					}
				}
			}
		}
	}
	
	public void Shoot(string weaponType, Vector3 origin, Vector3 direction, Vector3 end, NetworkViewID shooterID, bool hit){
		//we have fired a shot, let's tell everyone about it so they can see it
		NetworkViewID bulletID = Network.AllocateViewID();
		networkView.RPC("ShootRPC",RPCMode.All, weaponType, origin, direction, end, shooterID, bulletID, hit);
	}
	[RPC]
	void ShootRPC(string weaponType, Vector3 origin, Vector3 direction, Vector3 end, NetworkViewID shooterID, NetworkViewID bulletID, bool hit, NetworkMessageInfo info){
		//somebody fired a shot, let's show it
		
		lastRPCtime = Time.time;
		
		artilleryScript.Shoot(weaponType, origin, direction, end, shooterID, bulletID, info.timestamp, hit);
	}
	
	public void RegisterHit(string weaponType, NetworkViewID shooterID, NetworkViewID victimID, Vector3 hitPos){
		//we hit somebody, tell the server!
		
		
		if (gameOver) return; //well, don't tell the server if it's game over
		
		if (!isServer){
			networkView.RPC("RegisterHitRPC", RPCMode.Server, weaponType, shooterID, victimID, hitPos);
		}else{
			RegisterHitRPC( weaponType, shooterID, victimID, hitPos);
		}
	}
	[RPC]
	public void RegisterHitRPC(string weaponType, NetworkViewID shooterID, NetworkViewID victimID, Vector3 hitPos){
		//one player hit another, lets subtract health, kill, assign scores etc now
		lastRPCtime = Time.time;
		
		//Debug.Log("happens!UX");
		
		if (gameOver) return; //no killing after game over
		
		int shooterIndex = -1;
		int victimIndex = -1;
		
		bool killShot = false;
		
		
		
		//Debug.Log("hit registered");
		
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID == shooterID) shooterIndex = i;
			if (players[i].viewID == victimID) victimIndex = i;
		}
		
		if (shooterIndex == -1 || victimIndex == -1 || players[victimIndex].health<=0f) return;
		
		if (weaponType == "swapper"){
			networkView.RPC("SwapPlayers",RPCMode.All, shooterID, players[shooterIndex].fpsEntity.transform.position, victimID, players[victimIndex].fpsEntity.transform.position);
			return;
		}
		
		
		//subtract health
		if (shooterIndex == victimIndex && (weaponType == "rocket" || weaponType == "grenade")){
			//rocket jumping + grenade jumping
			players[victimIndex].health -= FPSArtillery.GetWeaponDamage(weaponType)*0.3f;
		}else{
			//normal damage
			players[victimIndex].health -= FPSArtillery.GetWeaponDamage(weaponType);
		}
		if (players[victimIndex].health<=0f){
			//player died
			players[victimIndex].health = 0f;
			killShot = true;
		}
		
		
		
		//scores/kills/death stats
		if (killShot){
			players[victimIndex].deaths++;
			if (gameSettings.deathsSubtractScore) players[victimIndex].currentScore--;
			
			players[shooterIndex].kills++;
			if (gameSettings.killsIncreaseScore) players[shooterIndex].currentScore++;
			
			//Air-Rocket +1 Score Extra
			if (gameSettings.scoreAirrockets) {
				if (!players[victimIndex].fpsEntity.grounded){
					if (weaponType == "rocket"){
						players[shooterIndex].currentScore++;
					}
				}
			}
		}
		
		//team stuff
		if (killShot && gameSettings.teamBased){
			if (players[victimIndex].team == 1 && players[shooterIndex].team == 2 && gameSettings.deathsSubtractScore) team1Score--;
			if (players[victimIndex].team == 2 && players[shooterIndex].team == 1 && gameSettings.deathsSubtractScore) team2Score--;
			if (players[shooterIndex].team == 1 && players[victimIndex].team == 2 && gameSettings.killsIncreaseScore) team1Score++;
			if (players[shooterIndex].team == 2 && players[victimIndex].team == 1 && gameSettings.killsIncreaseScore) team2Score++;
		}
		
		//assign results
		networkView.RPC("AssignPlayerStats", RPCMode.All, victimID, players[victimIndex].health, players[victimIndex].kills, players[victimIndex].deaths, players[victimIndex].currentScore);
		networkView.RPC("AssignPlayerStats", RPCMode.All, shooterID, players[shooterIndex].health, players[shooterIndex].kills, players[shooterIndex].deaths, players[shooterIndex].currentScore);
		
		
		if (killShot) networkView.RPC("AnnounceKill", RPCMode.All, weaponType, shooterID, victimID);
		if (gameSettings.teamBased && killShot) networkView.RPC("AnnounceTeamScores", RPCMode.Others, team1Score, team2Score);
		
		//let players see hit
		networkView.RPC("ShowHit", RPCMode.All, weaponType, hitPos, victimID);
		
		
		//check for game over
		if (gameSettings.winScore > 0){
			for (int i=0; i<players.Count; i++){
				if (players[i].currentScore>=gameSettings.winScore){
					networkView.RPC("AnnounceGameOver",RPCMode.All);
				}
			}
		}
	}
	
	[RPC]
	void SwapPlayers(NetworkViewID shooterID, Vector3 shooterPos, NetworkViewID victimID, Vector3 victimPos){
		for (int i=0; i<players.Count; i++){
			if (players[i].local){
				if (players[i].viewID == shooterID){
					players[i].fpsEntity.transform.position = victimPos;
					players[i].fpsEntity.sendRPCUpdate = true;
					players[i].fpsEntity.PlaySound("Swapped");
					players[i].fpsEntity.ForceLook(shooterPos);
				}else if(players[i].viewID == victimID){
					players[i].fpsEntity.transform.position = shooterPos;
					players[i].fpsEntity.sendRPCUpdate = true;
					players[i].fpsEntity.PlaySound("Swapped");
					players[i].fpsEntity.ForceLook(victimPos);
				}
			}
		}
	}
	
	public GameObject splatPrefab;
	[RPC]
	void ShowHit(string weaponType, Vector3 hitPos, NetworkViewID viewID){
		int splatcount = 4;
		if (weaponType=="pistol") splatcount = 5;
		if (weaponType=="grenade") splatcount = 15;
		if (weaponType=="machinegun") splatcount = 2;
		if (weaponType=="rifle") splatcount = 30;
		if (weaponType=="suicide") splatcount = 30;
		if (weaponType=="rocket") splatcount = 30;
		for (int i=0; i<splatcount; i++){
			GameObject newSplat = (GameObject)GameObject.Instantiate(splatPrefab);
			newSplat.transform.position = hitPos;
		}
		
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID == viewID && players[i].local && players[i].health>0f) players[i].fpsEntity.PlaySound("takeHit");
		}
	}
	
	[RPC]
	void AnnounceGameOver(){
		gameTimeLeft = 0f;
		gameOver = true;
		nextMatchTime = 15f;
	}
	
	[RPC]
	void AnnounceTeamScores(int score1, int score2){
		lastRPCtime = Time.time;
		team1Score = score1;
		team2Score = score2;
	}
	
	[RPC]
	void SharePlayerScores(NetworkViewID viewID, int kills, int deaths, int currentScore){
		lastRPCtime = Time.time;
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID==viewID){
				players[i].kills = kills;
				players[i].deaths = deaths;
				players[i].currentScore = currentScore;
			}
		}
	}
	
	[RPC]
	void AssignPlayerStats(NetworkViewID viewID, float health, int kills, int deaths, int score){
		lastRPCtime = Time.time;
		if (localPlayer.viewID == viewID){
			localPlayer.health = health;
			localPlayer.kills = kills;
			localPlayer.deaths = deaths;
			localPlayer.currentScore = score;
		}
		
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID == viewID){
				players[i].health = health;
				players[i].kills = kills;
				players[i].deaths = deaths;
				players[i].currentScore = score;
			}
		}
	}
	
	[RPC]
	void AnnounceKill(string weaponType, NetworkViewID shooterID, NetworkViewID victimID){
		lastRPCtime = Time.time;
		
		if (localPlayer.viewID == shooterID) localPlayer.totalKills ++;
		if (localPlayer.viewID == victimID) localPlayer.totalDeaths ++;
		
		if (localPlayer.viewID == victimID){
			for (int i=0; i<players.Count; i++){
				if (players[i].viewID == shooterID){
					
					for (int k=0; k<players.Count; k++){
						if (players[k].viewID == localPlayer.viewID){
					
							players[k].fpsEntity.ourKiller = players[i].fpsEntity.gameObject;
							
						}
					}
					
				}
			}
		}
		
		string shooterName = "Someone";
		string victimName = "Someone";
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID == shooterID) shooterName = players[i].name;
			if (players[i].viewID == victimID) victimName = players[i].name;
			if (players[i].viewID == victimID) players[i].fpsEntity.PlaySound("die");
			
			//lives
			if (players[i].viewID == victimID){
				players[i].lives--;
				if (players[i].lives<=0 && players[i].local && gameSettings.playerLives>0){
					//spectate
					players[i].fpsEntity.spectateMode = true;
				}
			}
			
			//basketball
			if (players[i].viewID == victimID){
				if (gameSettings.basketball && players[i].hasBall){
					players[i].hasBall = false;
					if (isServer){
						ThrowBall(players[i].fpsEntity.transform.position, -Vector3.up, 2f);
					}
				}
			}
		}
		
		//lives
		if (isServer && connected && !gameOver && gameSettings.playerLives>0 && players.Count>1){
			int livingPlayers = 0;
			for (int i=0; i<players.Count; i++){
				if (players[i].lives>0) livingPlayers++;
			}
			if (livingPlayers<=1){
				gameTimeLeft = 0f;
				gameOver = true;
				
				nextMatchTime = 15f;
				
				networkView.RPC("AnnounceGameOver", RPCMode.Others);
			}
		}
		
		
		ChatMessage newMsg = new ChatMessage();
		newMsg.sender = "";
		newMsg.senderColor = Color.white;
		if (shooterName != victimName)
			newMsg.message = "> " + shooterName + " [" + weaponType + "] killed " + victimName;
		else
			newMsg.message = "> " + shooterName + " died.";
		messageScript.messages.Add(newMsg);
		messageScript.textDisplayTime = Time.time+messageScript.textFadeTime;
		
	}
	
	
	[RPC]
	void RespawnPlayer(NetworkViewID viewID){
		lastRPCtime = Time.time;
		
		if (viewID == localPlayer.viewID){
			for (int i=0; i<players.Count; i++){
				if (players[i].viewID == viewID){
					if ((gameSettings.playerLives > 0 && players[i].lives>0) || gameSettings.playerLives ==0){
						//respawn
						players[i].fpsEntity.Respawn();
					}
				}
			}
		}
	}
	
	void Update(){
		
		//ping periodically
		if (connected && Time.time > nextPingTime){
			nextPingTime = Time.time + 5f;
			for (int i=0; i<players.Count; i++){
				if (!players[i].local){
					new Ping(players[i].netPlayer.ipAddress);
				}
			}
		}
		
		//let players know they are still connected
		if (connected && isServer){
			if (Time.time > lastServerPoketime+5f){
				lastServerPoketime = Time.time+5f;
				networkView.RPC("ServerSaysHi",RPCMode.All);
			}
		}
		
		//are we still connected to the server?
		if (connected && !isServer){
			if (Time.time > lastRPCtime+30f){
				DisconnectNow();
				guiScript.menuPoint = "connectionError";
				errorString = "Client hasn't heard from host for 30 seconds.\nthis is probably because somebody's internet sucks,\nor latency is crazy high.\n\nplay with people closer to you,\nand shut off those torrents and the like! :P";
			}
		}
		
		//respawn dead players
		if (isServer){
			for (int i=0; i<players.Count; i++){
				if (players[i].health<=0f){
					if (Time.time > players[i].respawnTime){
						if (gameSettings.playerLives==0 || players[i].lives>0){
							players[i].health = 100f;
						}
						networkView.RPC("AssignPlayerStats", RPCMode.All, players[i].viewID, players[i].health, players[i].kills, players[i].deaths, players[i].currentScore);
						networkView.RPC("RespawnPlayer", RPCMode.All, players[i].viewID);
					}
				}else{
					players[i].respawnTime = Time.time + gameSettings.respawnWait;
				}
			}
		}
		
		//change team
		if (connected && gameSettings.teamBased){
			if (Input.GetKeyDown("t") && Input.GetKey("tab")){
				
				if (localPlayer.team == 1){
					localPlayer.team = 2;
				}else{
					localPlayer.team = 1;
				}
				
				networkView.RPC("PlayerChangedTeams",RPCMode.AllBuffered, localPlayer.viewID, localPlayer.team);
				
				for (int i=0; i<players.Count; i++){
					if (players[i].viewID == localPlayer.viewID && players[i].health>0f) players[i].fpsEntity.Respawn();
				}
			}
		}
		
		//count time!
		gameTimeLeft -= Time.deltaTime;
		
		//game time up?
		if (connected && !gameOver){
			if (gameTimeLeft<=0f && gameSettings.gameTime>0f){
				gameTimeLeft = 0f;
				gameOver = true;
				
				nextMatchTime = 15f;
			}
		}
		
		//if game over, count in next match
		if (connected && gameOver){
			nextMatchTime -= Time.deltaTime;
			if (nextMatchTime<=0f){
				nextMatchTime = 0f;
				
				if (isServer){
					//begin next match using current settings
					serverGameChange = true;
					lastGameWasTeamBased = gameSettings.teamBased;
					gameViewID = Network.AllocateViewID();
					RequestGameData();
				}
			}
		}
		
		
		
		
		//pickups
		if (connected && isServer && !gameOver){
			for (int i=0; i<pickupPoints.Count; i++){
				if (!pickupPoints[i].stocked){
					pickupPoints[i].restockTime-=Time.deltaTime;
					if (pickupPoints[i].restockTime<=0f){
						int restockType = -1;
						if (pickupPoints[i].pickupType == 1) restockType = gameSettings.pickupSlot1;
						if (pickupPoints[i].pickupType == 2) restockType = gameSettings.pickupSlot2;
						if (pickupPoints[i].pickupType == 3) restockType = gameSettings.pickupSlot3;
						if (pickupPoints[i].pickupType == 4) restockType = gameSettings.pickupSlot4;
						if (pickupPoints[i].pickupType == 5) restockType = gameSettings.pickupSlot5;
						if (restockType == -2){
							//random
							restockType = Random.Range(-1,artilleryScript.gunTypes.Length);
							if (restockType == -1) restockType--;
						}
						if (restockType != -1){
							networkView.RPC("RestockPickup", RPCMode.All, pickupPoints[i].pickupPointID, restockType);
						}
					}
				}
			}
		}
		
	}
	public Texture healthIcon;
	public GameObject pickupBoxPrefab;
	[RPC]
	void RestockPickup(int pointID, int restockType){
		for (int i=0; i<pickupPoints.Count; i++){
			if (pickupPoints[i].pickupPointID == pointID){
				if (pickupPoints[i].stocked) return;
				pickupPoints[i].stocked = true;
				
				GameObject newPickup = (GameObject)GameObject.Instantiate(pickupBoxPrefab);
				pickupPoints[i].currentAvailablePickup = newPickup;
				newPickup.transform.position = pickupPoints[i].transform.position;
				newPickup.transform.localScale = Vector3.one * 0.5f;
				PickupBoxScript box = newPickup.GetComponent<PickupBoxScript>();
				box.pickupPoint = pickupPoints[i];
				if (restockType<0){
					//health
					box.pickupName = "health";
					box.iconObj.renderer.material.SetTexture("_MainTex",healthIcon);
					Material[] mats = box.boxObj.renderer.materials;
					mats[0].color = Color.green;
					box.boxObj.renderer.materials = mats;
				}else{
					//gun of some type
					box.pickupName = artilleryScript.gunTypes[restockType].gunName;
					box.iconObj.renderer.material.SetTexture("_MainTex",artilleryScript.gunTypes[restockType].iconTex);
					Material[] mats = box.boxObj.renderer.materials;
					mats[0] = artilleryScript.gunTypes[restockType].gunMaterial;
					box.boxObj.renderer.materials = mats;
				}
			}
		}
	}
	
	public void UnstockPickupPoint(PickupPoint point){
		for (int i=0; i<pickupPoints.Count; i++){
			if (point == pickupPoints[i]){
				networkView.RPC("UnstockRPC", RPCMode.All, pickupPoints[i].pickupPointID);
			}
		}
	}
	[RPC]
	void UnstockRPC(int pointID){
		for (int i=0; i<pickupPoints.Count; i++){
			if (pointID == pickupPoints[i].pickupPointID){
				pickupPoints[i].stocked = false;
				if (pickupPoints[i].currentAvailablePickup != null) Destroy(pickupPoints[i].currentAvailablePickup);
				if (isServer) pickupPoints[i].restockTime = gameSettings.restockTime;
			}
		}
	}
	
	[RPC]
	void RequestPickupStocks(){
		//a client has requested the current pickup stock info
		for (int i=0; i<pickupPoints.Count; i++){
			if (pickupPoints[i].stocked){
				int restockType = -1;
				if (pickupPoints[i].pickupType == 1) restockType = gameSettings.pickupSlot1;
				if (pickupPoints[i].pickupType == 2) restockType = gameSettings.pickupSlot2;
				if (pickupPoints[i].pickupType == 3) restockType = gameSettings.pickupSlot3;
				if (pickupPoints[i].pickupType == 4) restockType = gameSettings.pickupSlot4;
				if (pickupPoints[i].pickupType == 5) restockType = gameSettings.pickupSlot5;
				if (restockType == -2){
					//random
					restockType = Random.Range(-1,artilleryScript.gunTypes.Length);
					if (restockType == -1) restockType--;
				}
				if (restockType != -1){
					networkView.RPC("RestockPickup", RPCMode.All, pickupPoints[i].pickupPointID, restockType);
				}
				
			}
		}
	}
	
	public void AnnounceBallCapture(NetworkViewID viewID){
		networkView.RPC("AnnounceBallCaptureRPC", RPCMode.All, viewID);
	}
	
	[RPC]
	void AnnounceBallCaptureRPC(NetworkViewID viewID){
		basketball.GetComponent<BasketballScript>().HoldBall(viewID);
	}
	
	public void ThrowBall(Vector3 fromPos, Vector3 direction, float strength){
		networkView.RPC("ThrowBallRPC", RPCMode.All, fromPos, direction, strength);
	}
	
	[RPC]
	void ThrowBallRPC(Vector3 fromPos, Vector3 direction, float strength){
		basketball.GetComponent<BasketballScript>().Throw(fromPos,direction,strength);
	}
	
	[RPC]
	void ServerSaysHi(){
		lastRPCtime = Time.time;
	}
	
	[RPC]
	void PlayerChangedTeams(NetworkViewID viewID, int team){
		lastRPCtime = Time.time;
		for (int i=0; i<players.Count; i++){
			//always set model visibility on team change, that way if *you* change teams, all lights are changed
			players[i].fpsEntity.SetModelVisibility(!players[i].local);
			if (viewID == players[i].viewID){
				players[i].team = team;
				//players[i].fpsEntity.SetModelVisibility(!players[i].local);
				
				ChatMessage newMsg = new ChatMessage();
				newMsg.sender = "";
				newMsg.message = "";
				if (team == 1){
					if (localPlayer.viewID == viewID){
						newMsg.senderColor = Color.red;
						newMsg.message = "> you defected!";
					}else if (localPlayer.team == 1){
						newMsg.senderColor = Color.red;
						newMsg.message = "> " + players[i].name + " joined your team.";
					}else{
						newMsg.senderColor = Color.cyan;
						newMsg.message = "> " + players[i].name + " left your team.";
					}
				}else{
					if (localPlayer.viewID == viewID){
						newMsg.senderColor = Color.cyan;
						newMsg.message = "> you defected!";
					}else if (localPlayer.team == 2){
						newMsg.senderColor = Color.cyan;
						newMsg.message = "> " + players[i].name + " joined your team.";
					}else{
						newMsg.senderColor = Color.red;
						newMsg.message = "> " + players[i].name + " left your team.";
					}
				}
				messageScript.messages.Add(newMsg);
				messageScript.textDisplayTime = Time.time+messageScript.textFadeTime;
				
			}
		}
	}
	
	//-------- player joining stuff ----------
	[RPC]
	void NewPlayer(NetworkViewID viewID, string name, Vector3 cA, Vector3 cB, Vector3 cC, int head, NetworkPlayer np, int targetTeam, int lives){
		Debug.Log("Received player info! - " + name + " - Lives: " + lives);
		lastRPCtime = Time.time;
		
		
		if (players.Count==1 && gameSettings.playerLives>0){
			if (isServer && connected && !gameOver){
				//this is a lives match, and now we have enough players
				gameTimeLeft = 0f;
				gameOver = true;
				nextMatchTime = 5f; //CHANGE ME
				
				networkView.RPC("AnnounceGameOver", RPCMode.Others);
			}	
		}
		
		
		
		//this could just be a team change update at the start of the level
		bool weExist = false;
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID == viewID){
				weExist = true;
				players[i].team = targetTeam;
				
				players[i].lives = lives;
				players[i].health = 100f;
			}
		}
		if (weExist) return;
		
		
		
		//another player has joined, lets add them to our view of the game
		bool localShopforLocalPeople = false;
		if (viewID == localPlayer.viewID) localShopforLocalPeople = true;
		AddPlayer(localShopforLocalPeople, viewID, VecToCol(cA), VecToCol(cB), VecToCol(cC), head, name, np, targetTeam, lives);
		
		if (levelLoaded){
			//only instantiage the actual object of the player if we are in the right level
			//uninstantiated players are added when the level finished loading
			players[players.Count-1].InstantiateFPSEntity(fpsEntityPrefab);
		}
		
		
		//tell local fpsEntity to broadcast position so new players know
		broadcastPos = true;
		//also let new players know the scores
		if (isServer){
			for (int i=0; i<players.Count; i++){
				networkView.RPC("SharePlayerScores", RPCMode.Others, players[i].viewID, players[i].kills, players[i].deaths, players[i].currentScore);
			}
			networkView.RPC("AnnounceTeamScores", RPCMode.Others, team1Score, team2Score);
		}
		
		
		if (levelLoaded){
			ChatMessage newMsg = new ChatMessage();
			newMsg.sender = "";
			newMsg.senderColor = Color.grey;
			newMsg.message = "> " + name + " joined the game.";
			messageScript.messages.Add(newMsg);
			messageScript.textDisplayTime = Time.time+messageScript.textFadeTime;
		}
	}
	
	void AddPlayer(bool isLocal, NetworkViewID anID, Color cA, Color cB, Color cC, int head, string name, NetworkPlayer np, int targetTeam, int lives){
		FPSPlayer newPlayer = new FPSPlayer();
		
		newPlayer.colA = cA;
		newPlayer.colB = cB;
		newPlayer.colC = cC;
		
		newPlayer.headType = head;
		
		newPlayer.viewID = anID;
		
		newPlayer.local = isLocal;
		
		newPlayer.name = name;
		
		newPlayer.netPlayer = np;
		
		newPlayer.ping = new Ping(newPlayer.netPlayer.ipAddress);
		
		newPlayer.team = targetTeam;
		
		newPlayer.kills = 0;
		newPlayer.deaths = 0;
		newPlayer.currentScore = 0;
		
		
		newPlayer.lives = lives;
		
		players.Add(newPlayer);
	}
	
	
	
	//---------------- Game set-up stuff -------------
	
	public void AssignGameSettings(GameModeScript modeSettings, string levelName){
		
		gameSettings.gameModeName = modeSettings.gameModeName;
		gameSettings.gameModeDescription = modeSettings.gameModeDescription;
		gameSettings.levelName = levelName;
		
		gameSettings.winScore = modeSettings.winScore;
		gameSettings.gameTime = modeSettings.gameTime;
		gameSettings.respawnWait = modeSettings.respawnWait;
		gameSettings.deathsSubtractScore = modeSettings.deathsSubtractScore;
		gameSettings.killsIncreaseScore = modeSettings.killsIncreaseScore;
		gameSettings.teamBased = modeSettings.teamBased;
		gameSettings.allowFriendlyFire = modeSettings.allowFriendlyFire;
		gameSettings.pitchBlack = modeSettings.pitchBlack;
		gameSettings.scoreAirrockets = modeSettings.scoreAirrockets;
		gameSettings.offhandCooldown = modeSettings.offhandCooldown;
		gameSettings.spawnGunA = modeSettings.spawnGunA;
		gameSettings.spawnGunB = modeSettings.spawnGunB;
		gameSettings.pickupSlot1 = modeSettings.pickupSlot1;
		gameSettings.pickupSlot2 = modeSettings.pickupSlot2;
		gameSettings.pickupSlot3 = modeSettings.pickupSlot3;
		gameSettings.pickupSlot4 = modeSettings.pickupSlot4;
		gameSettings.pickupSlot5 = modeSettings.pickupSlot5;
		
		gameSettings.restockTime = modeSettings.restockTime;
		
		gameSettings.playerLives = modeSettings.playerLives;
		
		gameSettings.basketball = modeSettings.basketball;
	}
	
	[RPC]
	public void RequestGameData(){
		//a player has requested game data, pass it out.
		lastRPCtime = Time.time;
		
		//also figure out which team to drop them in
		int team1count = 0;
		int team2count = 0;
		for (int i=0; i<players.Count; i++){
			if (players[i].team == 1) team1count++;
			if (players[i].team == 2) team2count++;
		}
		int targetTeam = 0;
		if (gameSettings.teamBased) targetTeam = 1;
		if (gameSettings.teamBased && team1count>team2count) targetTeam = 2;
		if (gameSettings.teamBased && team2count>team1count) targetTeam = 1;
		
		//keep teams if we are already assigned to teams
		if (serverGameChange && players.Count>0){
			if (lastGameWasTeamBased){
				targetTeam = -1;
			}else{
				targetTeam = -2;
			}
		}
		lastGameWasTeamBased = false;
		
		int livesBroadcast = 0;
		if (serverGameChange){
			gameTimeLeft = gameSettings.gameTime * 60f;
			gameOver = false;
			livesBroadcast = gameSettings.playerLives;
		}else{
			if (gameSettings.playerLives>0){
				livesBroadcast = -1;
			}
		}
		
		networkView.RPC("BroadcastNewGame", RPCMode.All, gameViewID, gameSettings.gameModeName, gameSettings.levelName, gameSettings.gameModeDescription, gameSettings.winScore, gameSettings.gameTime, gameSettings.respawnWait, gameSettings.deathsSubtractScore, gameSettings.killsIncreaseScore, gameSettings.teamBased, targetTeam, gameSettings.allowFriendlyFire, gameSettings.pitchBlack, gameOver, gameTimeLeft, gameSettings.spawnGunA, gameSettings.spawnGunB, gameSettings.pickupSlot1, gameSettings.pickupSlot2, gameSettings.pickupSlot3, gameSettings.pickupSlot4, gameSettings.pickupSlot5, livesBroadcast, serverGameChange, gameSettings.basketball);
	}
	
	public float gameTimeLeft = 0f;
	public float nextMatchTime = 0f;
	
	[RPC]
	void BroadcastNewGame(NetworkViewID viewID, string gameModeName, string levelName, string gameModeDescription, int winScore, float gameTime, float respawnWait, bool deathsSubtractScore, bool killsIncreaseScore, bool teamBased, int targetTeam, bool allowFriendlyFire, bool pitchBlack, bool gameIsOver, float serverGameTime, int spawnGunA, int spawnGunB, int pickupSlot1, int pickupSlot2, int pickupSlot3, int pickupSlot4, int pickupSlot5, int playerLives, bool newGame, bool basketball, NetworkMessageInfo info){
		//we've received game info from the server
		lastRPCtime = Time.time;
		
		if (gameViewID != null && gameViewID == viewID && !serverGameChange) return;
		
		if (!isServer) gameTimeLeft = serverGameTime - (float)(Network.time - info.timestamp);
		
		
		//make sure all connected players have lives
		if (newGame){
			for (int i=0; i<players.Count; i++){
				players[i].lives = playerLives;
			}
		}
		
		
		if (!isServer) gameOver = gameIsOver;
		
		nextMatchTime = 15f;
		
		
		//if we get to this point, it's a new game, so let's get it together!
		gameViewID = viewID;
		serverGameChange = false;
		
		if (!isServer){
			//lets update the local game settings
			gameSettings.gameModeName = gameModeName;
			gameSettings.levelName = levelName;
			//...
			gameSettings.gameModeDescription = gameModeDescription;
			gameSettings.winScore = winScore;
			gameSettings.gameTime = gameTime;
			gameSettings.respawnWait = respawnWait;
			gameSettings.deathsSubtractScore = deathsSubtractScore;
			gameSettings.killsIncreaseScore = killsIncreaseScore;
			gameSettings.teamBased = teamBased;
			gameSettings.allowFriendlyFire = allowFriendlyFire;
			gameSettings.pitchBlack = pitchBlack;
			
			gameSettings.spawnGunA = spawnGunA;
			gameSettings.spawnGunB = spawnGunB;
			
			
			gameSettings.pickupSlot1 = pickupSlot1;
			gameSettings.pickupSlot2 = pickupSlot2;
			gameSettings.pickupSlot3 = pickupSlot3;
			gameSettings.pickupSlot4 = pickupSlot4;
			gameSettings.pickupSlot5 = pickupSlot5;
			
			gameSettings.playerLives = playerLives;
			
			gameSettings.basketball = basketball;
		}
		
		if (targetTeam != -1){
			//don't keep current teams
			localPlayer.team = targetTeam;
			for (int i=0; i<players.Count; i++){
				if (players[i].viewID == localPlayer.viewID){
					players[i].team = targetTeam;
				}
			}
		}if (targetTeam == -1){
			//last game was team based, leave teams as they are
		}if (targetTeam == -2){
			//last game wasn't team based, this is, set team.
			localPlayer.team = 1;
			if (Random.Range(0,10)<5) localPlayer.team = 2;
			for (int i=0; i<players.Count; i++){
				if (players[i].viewID == localPlayer.viewID){
					players[i].team = localPlayer.team;
				}
			}
		}
		team1Score = 0;
		team2Score = 0;
		
		//let's clear stuff out if we are already playing
		preppingLevel = false;
		levelLoaded = false;
		for (int i=0; i<players.Count; i++){
			if (players[i].fpsEntity != null) Destroy(players[i].fpsEntity.gameObject);
			players[i].fpsEntity = null;
			
			players[i].health = 100;
			players[i].kills = 0;
			players[i].deaths = 0;
			players[i].currentScore = 0;
		}
		localPlayer.health = 100;
		localPlayer.kills = 0;
		localPlayer.deaths = 0;
		localPlayer.currentScore = 0;
		artilleryScript.Clear();
		
		//now let's load the level
		preppingLevel = true;
		Application.LoadLevel(levelName);
	}
	
	void OnLevelWasLoaded (){
		if (preppingLevel){
			//level set up, let's play!
			preppingLevel = false;
			levelLoaded = true;
			
			//drop the basket ball in
			if (gameSettings.basketball){
				basketball = (GameObject)GameObject.Instantiate(basketballPrefab);
				if (!isServer) networkView.RPC("RequestBallStatus", RPCMode.Server);
			}else{
				if (GameObject.Find("_BasketRed") != null) Destroy(GameObject.Find("_BasketRed"));
				if (GameObject.Find("_BasketBlue") != null) Destroy(GameObject.Find("_BasketBlue"));
			}
			
			//add fps entities for all known players
			for (int i=0; i<players.Count; i++){
				players[i].InstantiateFPSEntity(fpsEntityPrefab);
			}
			
			//tell everyone we're here
			networkView.RPC("NewPlayer", RPCMode.AllBuffered, localPlayer.viewID, localPlayer.name, ColToVec(localPlayer.colA), ColToVec(localPlayer.colB), ColToVec(localPlayer.colC), localPlayer.headType, Network.player, localPlayer.team, gameSettings.playerLives);
			
			//make sure we know about pickup spawn points
			pickupPoints = new List<PickupPoint>();
			if (GameObject.Find("_PickupSpots")!=null){
				GameObject pickupPointHolder = GameObject.Find("_PickupSpots");
				foreach (Transform child in pickupPointHolder.transform){
					int stockType = -1;
					if (child.GetComponent<PickupPoint>().pickupType == 1) stockType = gameSettings.pickupSlot1;
					if (child.GetComponent<PickupPoint>().pickupType == 2) stockType = gameSettings.pickupSlot2;
					if (child.GetComponent<PickupPoint>().pickupType == 3) stockType = gameSettings.pickupSlot3;
					if (child.GetComponent<PickupPoint>().pickupType == 4) stockType = gameSettings.pickupSlot4;
					if (child.GetComponent<PickupPoint>().pickupType == 5) stockType = gameSettings.pickupSlot5;
					if (stockType != -1){
						pickupPoints.Add(child.GetComponent<PickupPoint>());
					}else{
						Destroy(child.gameObject);
					}
				}
			}
			networkView.RPC("RequestPickupStocks",RPCMode.Server);
			
			Screen.lockCursor = true;
			
		}
	}
	
	[RPC]
	void RequestBallStatus(){
		//player has joined and doesn't yet know the status of the basketball, lets share it
		BasketballScript bballScript = basketball.GetComponent<BasketballScript>();
		networkView.RPC("ShareBallStatus",RPCMode.Others, basketball.transform.position, bballScript.moveVector, bballScript.throwerID, bballScript.held);
	}
	[RPC]
	void ShareBallStatus(Vector3 ballPos, Vector3 ballMovement, NetworkViewID ballThrower, bool ballHeld){
		BasketballScript bballScript = basketball.GetComponent<BasketballScript>();
		bballScript.throwerID = ballThrower;
		if (ballHeld){
			bballScript.HoldBall(ballThrower);
		}else{
			bballScript.Throw(ballPos, ballMovement.normalized, ballMovement.magnitude);
		}
	}
	
	//------------- Connecting/Server setup --------------
	void OnServerInitialized() {
        Debug.Log("Server initialized, now registering...");
		MasterServer.RegisterHost(uniqueGameName, gameName, comment);
    }
	
    void OnMasterServerEvent(MasterServerEvent msEvent) {
        if (msEvent == MasterServerEvent.RegistrationSucceeded){
			//success registering
            Debug.Log("Server registered");
			
			isServer = true;
			
			
			if (!connected){
				//we've just joined a game as host, lets create the local player and it to the RPC buffer
				localPlayer.viewID = Network.AllocateViewID();
				
				guiScript.menuPoint = "wait";
				
				gameViewID = Network.AllocateViewID();
				
				RequestGameData();
				
				
				
			}
			
			connected = true;
			
		}else if (msEvent == MasterServerEvent.RegistrationFailedNoServer || msEvent == MasterServerEvent.RegistrationFailedGameType || msEvent == MasterServerEvent.RegistrationFailedGameName){
			//failure registering
			Debug.Log("server registration failed, disconnecting");
			errorString = "server registration failed";
			guiScript.menuPoint = "connectionError";
			
			localPlayer.viewID = new NetworkViewID();
			gameViewID = new NetworkViewID();
			
			Network.Disconnect();
		}
    }
	
	void OnConnectedToServer(){
		Debug.Log("Connected to a server");
		connected = true;
		
		//we just connected to a host, let's RPC the host and ask for the game info
		networkView.RPC("RequestGameData", RPCMode.Server);
		guiScript.menuPoint = "wait";
		
		lastRPCtime = Time.time;
		
		localPlayer.viewID = Network.AllocateViewID();
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
        Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
    }
	void OnFailedToConnect(NetworkConnectionError error){
		errorString = "";
		if (error == NetworkConnectionError.NoError) errorString = "No Error Reported.";
		if (error == NetworkConnectionError.RSAPublicKeyMismatch) errorString = "We presented an RSA public key which does not match what the system we connected to is using.";
		if (error == NetworkConnectionError.InvalidPassword) errorString = "The server is using a password and has refused our connection because we did not set the correct password.";
		if (error == NetworkConnectionError.ConnectionFailed) errorString = "Connection attempt failed, possibly because of internal connectivity problems.";
		if (error == NetworkConnectionError.TooManyConnectedPlayers) errorString = "The server is at full capacity, failed to connect.";
		if (error == NetworkConnectionError.ConnectionBanned) errorString = "We are banned from the system we attempted to connect to (likely temporarily).";
		if (error == NetworkConnectionError.AlreadyConnectedToServer) errorString = "We are already connected to this particular server (can happen after fast disconnect/reconnect).";
		if (error == NetworkConnectionError.AlreadyConnectedToAnotherServer) errorString = "Cannot connect to two servers at once. Close the connection before connecting again.";
		if (error == NetworkConnectionError.CreateSocketOrThreadFailure) errorString = "Internal error while attempting to initialize network interface. Socket possibly already in use.";
		if (error == NetworkConnectionError.IncorrectParameters) errorString = "Incorrect parameters given to Connect function.";
		if (error == NetworkConnectionError.EmptyConnectTarget) errorString = "No host target given in Connect.";
		if (error == NetworkConnectionError.InternalDirectConnectFailed) errorString = "Client could not connect internally to same network NAT enabled server.";
		if (error == NetworkConnectionError.NATTargetNotConnected) errorString = "The NAT target we are trying to connect to is not connected to the facilitator server.";
		if (error == NetworkConnectionError.NATTargetConnectionLost) errorString = "Connection lost while attempting to connect to NAT target.";
		if (error == NetworkConnectionError.NATPunchthroughFailed) errorString = "NAT punchthrough attempt has failed. The cause could be a too restrictive NAT implementation on either endpoints.";
		
		Debug.Log("Failed to Connect: " + errorString);
		
		Network.Disconnect();
		
		localPlayer.viewID = new NetworkViewID();
		gameViewID = new NetworkViewID();
		
		guiScript.menuPoint = "connectionError";
	}
	public string errorString = "";
	
	//--------- Disconnecting, quitting, kicking -----------
	void OnApplicationQuit(){
		DisconnectNow();
	}
	
	public void DisconnectNow(){
		if (connected){
			if (!isServer){
				networkView.RPC("PlayerLeave", RPCMode.OthersBuffered, localPlayer.viewID, localPlayer.name);
			}else{
				networkView.RPC("ServerLeave", RPCMode.OthersBuffered);
			}
			
			localPlayer.viewID = new NetworkViewID();
			gameViewID = new NetworkViewID();
			
			Network.Disconnect();
			if (isServer) MasterServer.UnregisterHost();
			connected = false;
			isServer = false;
			
			Camera.main.transform.parent = null;
			for (int i=0; i<players.Count; i++){
				if (players[i].fpsEntity!=null) Destroy(players[i].fpsEntity.gameObject);
			}
			players = new List<FPSPlayer>();
			
			guiScript.menuPoint = "top";
			Application.LoadLevel("MenuScene");
			levelLoaded = false;
		}
	}
	
	
	[RPC]
	void PlayerLeave(NetworkViewID viewID, string name){
		lastRPCtime = Time.time;
		Debug.Log("A player did one");
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID == viewID){
				
				if (basketball && players[i].hasBall){
					basketball.transform.parent = null;
					if (isServer){
						ThrowBall(players[i].fpsEntity.transform.position, -Vector3.up, 2f);
					}
				}
				
				
				if (players[i].fpsEntity!= null) Destroy(players[i].fpsEntity.gameObject);
				players.RemoveAt(i);
			}
		}
		
		ChatMessage newMsg = new ChatMessage();
		newMsg.sender = "";
		newMsg.senderColor = Color.grey;
		newMsg.message = "> " + name + " left the game.";
		messageScript.messages.Add(newMsg);
		messageScript.textDisplayTime = Time.time+messageScript.textFadeTime;
	}
	
	public void Kick(int playerIndex){
		bool kicked = false;
		
		
		//do kicking here
		Network.CloseConnection(players[playerIndex].netPlayer, true);
		
		networkView.RPC("KickedPlayer", RPCMode.AllBuffered, players[playerIndex].viewID);
		
	}
	
	[RPC]
	void KickedPlayer(NetworkViewID viewID){
		lastRPCtime = Time.time;
		Debug.Log("A player was kicked");
		
		
		string name = "???";
		
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID == viewID){
				
				if (basketball && players[i].hasBall){
					basketball.transform.parent = null;
					if (isServer){
						ThrowBall(players[i].fpsEntity.transform.position, -Vector3.up, 2f);
					}
				}
				
				name = players[i].name;
				if (players[i].fpsEntity!= null) Destroy(players[i].fpsEntity.gameObject);
				players.RemoveAt(i);
			}
		}
		
		
		ChatMessage newMsg = new ChatMessage();
		newMsg.sender = "";
		newMsg.senderColor = Color.grey;
		newMsg.message = "> " + name + " was kicked.";
		messageScript.messages.Add(newMsg);
		messageScript.textDisplayTime = Time.time+messageScript.textFadeTime;
		
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection netDis){
		Debug.Log("Disconnected from server");
		if (isServer) return;
		
		ChatMessage newMsg = new ChatMessage();
		newMsg.sender = "";
		newMsg.senderColor = Color.white;
		newMsg.message = "> You have been disconnected from the server.";
		messageScript.messages.Add(newMsg);
		messageScript.textDisplayTime = Time.time+messageScript.textFadeTime;
		
		//Network.Disconnect();
		//if (isServer) MasterServer.UnregisterHost();
		connected = false;
		isServer = false;
		
		localPlayer.viewID = new NetworkViewID();
		gameViewID = new NetworkViewID();
				
		Camera.main.transform.parent = null;
		for (int i=0; i<players.Count; i++){
			if (players[i].fpsEntity!= null) Destroy(players[i].fpsEntity.gameObject);
		}
		players = new List<FPSPlayer>();
		
		guiScript.menuPoint = "top";
		Application.LoadLevel("MenuScene");
		levelLoaded = false;
	}
	
	[RPC]
	void ServerLeave(){
		lastRPCtime = Time.time;
		Debug.Log("THE SERVER BUGGERED OFF! HOW RUDE D:");
		
		Network.Disconnect();
		if (isServer) MasterServer.UnregisterHost();
		connected = false;
		isServer = false;
		
		localPlayer.viewID = new NetworkViewID();
		gameViewID = new NetworkViewID();
				
		Camera.main.transform.parent = null;
		for (int i=0; i<players.Count; i++){
			if (players[i].fpsEntity!= null) Destroy(players[i].fpsEntity.gameObject);
		}
		players = new List<FPSPlayer>();
		
		guiScript.menuPoint = "top";
		Application.LoadLevel("MenuScene");
		levelLoaded = false;
		
		ChatMessage newMsg = new ChatMessage();
		newMsg.sender = "";
		newMsg.senderColor = Color.grey;
		newMsg.message = "> The server shut down.";
		messageScript.messages.Add(newMsg);
		messageScript.textDisplayTime = Time.time+messageScript.textFadeTime;
	}
	
	
	
	//-------------- misc functions and stuff ----------
	public Vector3 ColToVec(Color colIn){
		//convert colour to a vector
		Vector3 retVec = Vector3.zero;
		retVec.x = colIn.r;
		retVec.y = colIn.g;
		retVec.z = colIn.b;
		return retVec;
	}
	public Color VecToCol(Vector3 vecIn){
		//convert vector to a color
		Color retCol = Color.white;
		retCol.r = vecIn.x;
		retCol.g = vecIn.y;
		retCol.b = vecIn.z;
		return retCol;
	}
	
}
