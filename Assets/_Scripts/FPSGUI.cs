using UnityEngine;
using System;
using System.Collections;
using System.Globalization;

public class FPSGUI : MonoBehaviour {

	private SophieNetworkScript theNetwork;
	private FPSMessages messageScript;
	private FPSArtillery artillery;
	
	public GameModeScript[] modes;
	private int gameModeInt = 1;
	private int hostLevelSelectInt = 0;
	
	public LevelInfoScript[] levels;
	
	public Texture lifeIcon;
	
	public Texture[] swapperCrosshair;
	public int swapperCrossX = 0;
	public int swapperCrossY = 0;
	public bool swapperLocked = false;
	
	public string menuPoint = "top";
	
	
	private Vector2 scrollPos = Vector2.zero;
	
	public Texture backTex;
	public Texture blackTex;
	
	public Texture crossHair;
	
	public Texture gamelogo;
	
	public string offeredPickup = "";
	
	public bool spectateMode = false;
	public int spectateInt = 0;
	
	//settings
	public bool invX = false;
	public bool invY = false;
	public float mouseSensitivity = 2f;
	
	
	public int gunA = 0;
	public float gunACooldown = 0f;
	public int gunB = 0;
	
	
	void Start(){
		theNetwork = GetComponent<SophieNetworkScript>();
		messageScript = GetComponent<FPSMessages>();
		artillery = GetComponent<FPSArtillery>();
		
		//make local player
		theNetwork.localPlayer = new FPSPlayer();
		theNetwork.localPlayer.local = true;
		theNetwork.localPlayer.name = PlayerPrefs.GetString("PlayerName", "Player Playerson");
		theNetwork.localPlayer.headType = PlayerPrefs.GetInt("PlayerHead", 0);
		theNetwork.localPlayer.colA.r = PlayerPrefs.GetFloat("PlayerColA_R", Color.red.r);
		theNetwork.localPlayer.colA.g = PlayerPrefs.GetFloat("PlayerColA_G", Color.red.g);
		theNetwork.localPlayer.colA.b = PlayerPrefs.GetFloat("PlayerColA_B", Color.red.b);
		theNetwork.localPlayer.colA.a = 1;
		theNetwork.localPlayer.colB.r = PlayerPrefs.GetFloat("PlayerColB_R", Color.green.r);
		theNetwork.localPlayer.colB.g = PlayerPrefs.GetFloat("PlayerColB_G", Color.green.g);
		theNetwork.localPlayer.colB.b = PlayerPrefs.GetFloat("PlayerColB_B", Color.green.b);
		theNetwork.localPlayer.colB.a = 1;
		theNetwork.localPlayer.colC.r = PlayerPrefs.GetFloat("PlayerColC_R", Color.cyan.r);
		theNetwork.localPlayer.colC.g = PlayerPrefs.GetFloat("PlayerColC_G", Color.cyan.g);
		theNetwork.localPlayer.colC.b = PlayerPrefs.GetFloat("PlayerColC_B", Color.cyan.b);
		theNetwork.localPlayer.colC.a = 1;
		
		//load settings
		invX = PlayerPrefs.GetInt("InvertX", 0) == 1;
		invY = PlayerPrefs.GetInt("InvertY", 0) == 1;
		mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
		messageScript.textFadeTime = PlayerPrefs.GetFloat("textFadeTime", 10f);
		theNetwork.gunBobbing = PlayerPrefs.GetInt("GunBobbing",1)==1;
		theNetwork.autoPickup = PlayerPrefs.GetInt("autoPickup",0)==1;
		theNetwork.autoPickupHealth = PlayerPrefs.GetInt("autoPickupHealth",0)==1;
		theNetwork.gameVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
	}
	
	
	void OnGUI(){
		
		
		if (!theNetwork.connected){
			
			Screen.lockCursor = false;
			
			if (menuPoint == "top"){
				//Top Menu
				
				GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
				
				if (GUILayout.Button("Host a game")){
					menuPoint = "host";
					theNetwork.gameName = theNetwork.localPlayer.name + "'s Game";
				}
				if (GUILayout.Button("Join a game")){
					menuPoint = "join";
					scrollPos = Vector2.zero;
					MasterServer.RequestHostList(theNetwork.uniqueGameName);
					hostPings = new Ping[0];
				}
				if (GUILayout.Button("Personalise Name/Avatar")){
					menuPoint = "personalise";
				}
				if (GUILayout.Button("Config")){
					menuPoint = "config";
				}
				if (GUILayout.Button("About")){
					menuPoint = "about";
				}
				GUILayout.Label("");
				if (!Application.isWebPlayer){
					if (GUILayout.Button("Quit")){
						Application.Quit();
					}
				}
				
				GUI.DrawTexture(new Rect(0,150,256,256), gamelogo);
				
				GUI.EndGroup();
				
			}else if (menuPoint == "host"){
				
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Back...")){
					menuPoint = "top";
				}
				
				DrawBGBox(false);
				GameSetup(false);
				
			}else if (menuPoint == "join"){
				
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Back...")){
					menuPoint = "top";
				}
				
				DrawBGBox(false);
				
				JoinMenu();
				
				
				
			}else if (menuPoint == "personalise"){
				
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Back...")){
					menuPoint = "top";
					theNetwork.localPlayer.name = PlayerPrefs.GetString("PlayerName", "Player Playerson");
				}
				
				DrawBGBox(true);
				
				PersonaliseMenu();
				
				
			}else if (menuPoint == "config"){
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Back...")){
					menuPoint = "top";
				}
				
				DrawBGBox(false);
				Config();
			}else if(menuPoint=="about"){
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Back...")){
					menuPoint = "top";
				}
				
				DrawBGBox(false);
				
				GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
				
				
				if (GUI.Button(new Rect(0,360,200,40), "Sophie Houlden")){
					Application.OpenURL("http://sophiehoulden.com");
				}
				if (GUI.Button(new Rect(200,360,200,40), "7DFPS")){
					Application.OpenURL("http://7dfps.org");
				}
				if (GUI.Button(new Rect(400,360,200,40), "SPLAT DEATH SALAD\nHomepage")){
					Application.OpenURL("http://7dfps.org/?projects=splat-death-salad");
				}
				
				
				GUILayout.Label(" === SPLAT DEATH SALAD Version 1 ===");
				GUILayout.Label("Made by Sophie Houlden using Unity, for 7DFPS, June 2012");
				GUILayout.Label("Remember, low ping is good, high ping is bad!");
				GUILayout.Label("Don't be surprised if you have poor performance or get kicked with high ping");
				GUILayout.Label("For best results, play across a LAN :)");
				if (Application.isWebPlayer) GUILayout.Label("*** Visit the SDS homepage for standalone client downloads (win/mac) ***");
				
				GUILayout.Label("");
				GUILayout.Label(" === PEW Version 0.1 ===");
				GUILayout.Label("You are playing a modded version of SplatDeathSalad.");
				GUILayout.Label("For further information check the github-repository:");
				GUILayout.Label("http://github.com/madpew/SplatDeathSalad/");
				//GUILayout.Label("*** when it is finished though, I'll be releasing the source ***");
				
				GUI.EndGroup();
			}else if (menuPoint == "connectionError"){
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Back...")){
					menuPoint = "top";
				}
				
				DrawBGBox(false);
				
				GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
				
				GUILayout.Label("Failed to Connect:");
				GUILayout.Label(theNetwork.errorString);
				
				GUI.EndGroup();
			}else if (menuPoint == "connecting"){
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Back...")){
					Network.Disconnect();
					menuPoint = "top";
				}
				
				DrawBGBox(false);
				
				GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
				
				GUILayout.Label("Connecting...");
				
				GUI.EndGroup();
			}else if (menuPoint == "initializingServer"){
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Back...")){
					Network.Disconnect();
					menuPoint = "top";
				}
				
				DrawBGBox(false);
				
				GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
				
				GUILayout.Label("Initialising Server...");
				
				GUI.EndGroup();
			}
			
			
			
			
		
		}
		
		//player model customisation
		if (menuPoint=="top" || menuPoint=="personalise"){
			
			
			if (GameObject.Find("CharaMesh") != null){
				//colours
				Material[] mats = GameObject.Find("CharaMesh").renderer.materials;
				mats[0].color = theNetwork.localPlayer.colA;
				mats[1].color = theNetwork.localPlayer.colB;
				mats[2].color = theNetwork.localPlayer.colC;
				GameObject.Find("NormalHead").renderer.material.color = theNetwork.localPlayer.colA;
				
				//heads
				GameObject.Find("NormalHead").renderer.enabled = false;
				GameObject.Find("CardboardBoxHead").renderer.enabled = false;
				GameObject.Find("FishHead").renderer.enabled = false;
				GameObject.Find("BananaHead").renderer.enabled = false;
				GameObject.Find("CreeperHead").renderer.enabled = false;
				GameObject.Find("ElephantHeadMesh").renderer.enabled = false;
				GameObject.Find("MoonHead").renderer.enabled = false;
				GameObject.Find("PyramidHead").renderer.enabled = false;
				GameObject.Find("ChocoboHead").renderer.enabled = false;
				GameObject.Find("SpikeHead").renderer.enabled = false;
				GameObject.Find("TentacleRoot").renderer.enabled = false;
				GameObject.Find("RobotHead").renderer.enabled = false;
				GameObject.Find("head_spaceship").renderer.enabled = false;
				GameObject.Find("enforcer_face").renderer.enabled = false;
				GameObject.Find("SmileyHead").renderer.enabled = false;
				GameObject.Find("Helmet").renderer.enabled = false;
				GameObject.Find("PaperBag").renderer.enabled = false;
				GameObject.Find("Mahead").renderer.enabled = false;
				
				if (theNetwork.localPlayer.headType == 0) GameObject.Find("NormalHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 1) GameObject.Find("CardboardBoxHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 2) GameObject.Find("FishHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 3) GameObject.Find("BananaHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 4) GameObject.Find("CreeperHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 5) GameObject.Find("ElephantHeadMesh").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 6) GameObject.Find("MoonHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 7) GameObject.Find("PyramidHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 8) GameObject.Find("ChocoboHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 9) GameObject.Find("SpikeHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 10) GameObject.Find("TentacleRoot").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 11) GameObject.Find("RobotHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 12) GameObject.Find("head_spaceship").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 13) GameObject.Find("enforcer_face").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 14) GameObject.Find("SmileyHead").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 15) GameObject.Find("Helmet").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 16) GameObject.Find("PaperBag").renderer.enabled = true;
				if (theNetwork.localPlayer.headType == 17) GameObject.Find("Mahead").renderer.enabled = true;
				
				
				
				GameObject.Find("PlayerNameText").GetComponent<TextMesh>().text = theNetwork.localPlayer.name;
			}
		}
			
		
		if (theNetwork.connected){
			
			if (!Screen.lockCursor){
				
				
				
				DrawBGBox(false);
				
				//top buttons
				if (GUI.Button(new Rect((Screen.width/2)-300,(Screen.height/2)-240,100,40), "Resume")){
					Screen.lockCursor = true;
				}
				if (theNetwork.isServer){
					if (GUI.Button(new Rect((Screen.width/2)-150,(Screen.height/2)-240,100,40), "Config")){
						gameMenuPoint = "config";
					}
					if (GUI.Button(new Rect((Screen.width/2)-50,(Screen.height/2)-240,100,40), "Change Game")){
						gameMenuPoint = "gameType";
					}
					if (GUI.Button(new Rect((Screen.width/2)+50,(Screen.height/2)-240,100,40), "Kick")){
						gameMenuPoint = "kick";
					}
				}
				if (GUI.Button(new Rect((Screen.width/2)+200,(Screen.height/2)-240,100,40), "Disconnect")){
					theNetwork.DisconnectNow();
				}
				
				//show menus
				if (gameMenuPoint == "kick"){
					if (theNetwork.isServer){
						KickMenu();
					}else {
						gameMenuPoint = "config";
					}
				}else if (gameMenuPoint == "gameType"){
					if (theNetwork.isServer){
						GameSetup(true);
					}else{
						gameMenuPoint = "config";
					}
				}else if (gameMenuPoint == "config"){
					Config();
				}
				
				
				
			}else{
				//connected & cursor locked
				
				if (Input.GetKey("tab") || SophieNetworkScript.gameOver){
					DrawBGBox(false);
					Scoreboard();
				}else{
					GUI.DrawTexture(new Rect((Screen.width/2)-8, (Screen.height/2)-8, 16, 16), crossHair);
					
					if (gunA == 5){
						//swapper
						int swapperFrame = Mathf.FloorToInt((Time.time*15f) % swapperCrosshair.Length);
						if (!swapperLocked) swapperFrame = 0;
						
						
						
						GUI.DrawTexture(new Rect(swapperCrossX-32, (Screen.height-swapperCrossY)-32, 64, 64), swapperCrosshair[swapperFrame]);
						
					}
				}
				
				//health bar
				int healthWidth = (Screen.width/3);
				GUI.DrawTexture(new Rect((Screen.width/2)-(healthWidth/2)-2, Screen.height-15, healthWidth+4, 9), blackTex);
				int healthWidthB = (int)((((float)healthWidth)/100f)*theNetwork.localPlayer.health);
				GUI.DrawTexture(new Rect((Screen.width/2)-(healthWidth/2), Screen.height-13, healthWidthB, 5), backTex);
				
				//lives
				if (theNetwork.gameSettings.playerLives>0){
					int lifeCount = 0;
					for (int i=0; i<theNetwork.players.Count; i++){
						if (theNetwork.players[i].local) lifeCount = theNetwork.players[i].lives;
					}
					//Debug.Log(lifeCount);
					for (int i=0; i<lifeCount; i++){
						GUI.DrawTexture(new Rect(Screen.width-60, i*30, 64, 64), lifeIcon);
					}
				}
				
				//pickup stuff
				Color gcol = GUI.color;
				if (offeredPickup != "" && !theNetwork.autoPickup){
					
					if (offeredPickup == "health" && theNetwork.autoPickupHealth)
					{
					}
					else
					{
						GUI.color = Color.black;
						GUI.Label(new Rect((Screen.width/2)-51,(Screen.height/2)+100,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
						GUI.Label(new Rect((Screen.width/2)-49,(Screen.height/2)+100,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
						GUI.Label(new Rect((Screen.width/2)-50,(Screen.height/2)+101,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
						GUI.Label(new Rect((Screen.width/2)-50,(Screen.height/2)+99,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
						GUI.color = gcol;
						GUI.Label(new Rect((Screen.width/2)-50,(Screen.height/2)+100,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
					}
				}
				
				//spectate
				if (spectateMode){
					GUI.color = Color.black;
					GUI.Label(new Rect(4, 5, 300, 60), "Spectating: " + theNetwork.players[spectateInt].name + "\n\nYou will be able to play once this round is over.");
					GUI.Label(new Rect(6, 5, 300, 60), "Spectating: " + theNetwork.players[spectateInt].name + "\n\nYou will be able to play once this round is over.");
					GUI.Label(new Rect(5, 4, 300, 60), "Spectating: " + theNetwork.players[spectateInt].name + "\n\nYou will be able to play once this round is over.");
					GUI.Label(new Rect(5, 6, 300, 60), "Spectating: " + theNetwork.players[spectateInt].name + "\n\nYou will be able to play once this round is over.");
					
					GUI.color = gcol;
					GUI.Label(new Rect(5, 5, 300, 60), "Spectating: " + theNetwork.players[spectateInt].name + "\n\nYou will be able to play once this round is over.");
					
				}
				
				//weapon
				GUI.color = new Color(0.1f,0.1f,0.1f,0.7f);
				if (gunB>=0) GUI.DrawTexture(new Rect(Screen.width-80,Screen.height-95,64,64),artillery.gunTypes[gunB].iconTex);
				GUI.color = gcol;
				if (gunA>=0) GUI.DrawTexture(new Rect(Screen.width-110,Screen.height-70,64,64),artillery.gunTypes[gunA].iconTex);
				
				if (gunA>=0){
					GUI.color = Color.black;
				
					GUI.Label(new Rect(Screen.width-99, Screen.height-20, 100, 30), artillery.gunTypes[gunA].gunName );
					GUI.Label(new Rect(Screen.width-101, Screen.height-20, 100, 30), artillery.gunTypes[gunA].gunName );
					GUI.Label(new Rect(Screen.width-100, Screen.height-21, 100, 30), artillery.gunTypes[gunA].gunName );
					GUI.Label(new Rect(Screen.width-100, Screen.height-19, 100, 30), artillery.gunTypes[gunA].gunName );
				
					
					GUI.color = gcol;
					GUI.Label(new Rect(Screen.width-100, Screen.height-20, 100, 30), artillery.gunTypes[gunA].gunName );
				}
				
				//weapon cooldown
				if (gunA>=0){
					GUI.DrawTexture(new Rect(Screen.width-103, Screen.height-27, 56, 8), blackTex);
					float coolDownPercent = 50f;
					if (artillery.gunTypes[gunA].fireCooldown>0f){
						coolDownPercent = (gunACooldown / artillery.gunTypes[gunA].fireCooldown) * 50f;
						coolDownPercent = 50f-coolDownPercent;
					}
					GUI.DrawTexture(new Rect(Screen.width-100, Screen.height-24, Mathf.FloorToInt(coolDownPercent), 2), backTex);
				}
				GUI.color = gcol;
			}
			
			Color gcolB = GUI.color;
			
			//time
			if (!SophieNetworkScript.gameOver){
				if (theNetwork.gameSettings.gameTime>0f){
					//game has a time limit, let's display the time
					
					GUI.color = Color.black;
					GUI.Label(new Rect((Screen.width/2)-11, 5, 200, 30), TimeStringFromSecs(theNetwork.gameTimeLeft) );
					GUI.Label(new Rect((Screen.width/2)-9, 5, 200, 30), TimeStringFromSecs(theNetwork.gameTimeLeft) );
					GUI.Label(new Rect((Screen.width/2)-10, 4, 200, 30), TimeStringFromSecs(theNetwork.gameTimeLeft) );
					GUI.Label(new Rect((Screen.width/2)-10, 6, 200, 30), TimeStringFromSecs(theNetwork.gameTimeLeft) );
					
					GUI.color = gcolB;
					GUI.Label(new Rect((Screen.width/2)-10, 5, 200, 30), TimeStringFromSecs(theNetwork.gameTimeLeft) );
					//GUI.Label(new Rect((Screen.width/2)-10, 5, 200, 30), Mathf.FloorToInt(theNetwork.gameTimeLeft).ToString() );
				}
			}else{
				//game is over, display countdown to next match
				GUI.color = Color.black;
				GUI.Label(new Rect((Screen.width/2)-51, 5, 200, 30), "Next Game in: " +  Mathf.FloorToInt(theNetwork.nextMatchTime).ToString() + " seconds.");
				GUI.Label(new Rect((Screen.width/2)-49, 5, 200, 30), "Next Game in: " +  Mathf.FloorToInt(theNetwork.nextMatchTime).ToString() + " seconds.");
				GUI.Label(new Rect((Screen.width/2)-50, 4, 200, 30), "Next Game in: " +  Mathf.FloorToInt(theNetwork.nextMatchTime).ToString() + " seconds.");
				GUI.Label(new Rect((Screen.width/2)-50, 6, 200, 30), "Next Game in: " +  Mathf.FloorToInt(theNetwork.nextMatchTime).ToString() + " seconds.");
				
				GUI.color = gcolB;
				GUI.Label(new Rect((Screen.width/2)-50, 5, 200, 30), "Next Game in: " +  Mathf.FloorToInt(theNetwork.nextMatchTime).ToString() + " seconds.");
			}
			
		}
		
		
		
		
	}
	
	string TimeStringFromSecs(float totalSecs){
		string timeString = "";
		
		int seconds = Mathf.FloorToInt(totalSecs);
		
		int minutes = 0;
		while(seconds>60){
			seconds-= 60;
			minutes++;
		}
		
		int hours = 0;
		while (minutes>60){
			minutes-=60;
			hours++;
		}
		
		if (hours>0){
			timeString += hours.ToString() + ":";
			if (minutes<10) timeString += "0";
		}
		timeString += minutes.ToString() + ":";
		if (seconds<10) timeString += "0";
		timeString += seconds.ToString();
		
		return timeString;
	}
	
	
	private string gameMenuPoint = "config";
	
	void DrawBGBox(bool halfBox){
		//bg
		Color guiCol = GUI.color;
		GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.9f);
		int theWidth = 600;
		if (halfBox) theWidth = 300;
		GUI.DrawTexture(new Rect((Screen.width/2)-300, (Screen.height/2)-200, theWidth,400), backTex);
		GUI.color = guiCol;
	}
	
	
	void Scoreboard(){
		GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
		
		GUI.Label(new Rect(250,0,100,20), "Scores:");
		
		Color guiCol = GUI.color;
		
		if (!theNetwork.gameSettings.teamBased){
			
			int highestScrore=-9999;
			if (SophieNetworkScript.gameOver){
				for (int i=0; i<theNetwork.players.Count; i++){
					if (theNetwork.players[i].currentScore > highestScrore){
						highestScrore = theNetwork.players[i].currentScore;
					}
				}
			}
			int mostLives = 0;
			if (SophieNetworkScript.gameOver){
				for (int i=0; i<theNetwork.players.Count; i++){
					if (theNetwork.players[i].lives > mostLives){
						mostLives = theNetwork.players[i].lives;
					}
				}
			}
			
			
			
			GUI.Label(new Rect(10, 20,150,20), "Name:");
			GUI.Label(new Rect(160, 20,50,20), "Kills:");
			GUI.Label(new Rect(210, 20,50,20), "Deaths:");
			GUI.Label(new Rect(270, 20,50,20), "Score:");
			if (theNetwork.gameSettings.playerLives!= 0) GUI.Label(new Rect(400, 20,50,20), "Lives:");
			for (int i=0; i<theNetwork.players.Count; i++){
				GUI.color = new Color(0.8f, 0.8f, 0.8f, 1f);
				if (theNetwork.players[i].local) GUI.color = new Color(1, 1, 1, 1f);
				if (theNetwork.players[i].currentScore == highestScrore && mostLives == 0) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
				if (theNetwork.players[i].lives == mostLives && mostLives>0) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
				GUI.Label(new Rect(10,(i*20) + 40,150,20), theNetwork.players[i].name);
				GUI.Label(new Rect(160,(i*20) + 40,50,20), theNetwork.players[i].kills.ToString());
				GUI.Label(new Rect(210,(i*20) + 40,50,20), theNetwork.players[i].deaths.ToString());
				GUI.Label(new Rect(270,(i*20) + 40,50,20), theNetwork.players[i].currentScore.ToString());
				if (theNetwork.gameSettings.playerLives!= 0) GUI.Label(new Rect(400, (i*20) + 40,50,20), theNetwork.players[i].lives.ToString());
			}
			
		}
		if (theNetwork.gameSettings.teamBased){
			
			GUI.color = new Color(1f, 0f, 0f, 1f);
			if (SophieNetworkScript.gameOver && theNetwork.team1Score>theNetwork.team2Score) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
			GUI.Label(new Rect(100, 20,150,20), "Team 1 Score: " + theNetwork.team1Score.ToString());
			GUI.color = new Color(0f, 1f, 1f, 1f);
			if (SophieNetworkScript.gameOver && theNetwork.team2Score>theNetwork.team1Score) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
			GUI.Label(new Rect(300, 20,150,20), "Team 2 Score: " + theNetwork.team2Score.ToString());
			
			GUI.color = guiCol;
			GUI.Label(new Rect(10, 40,150,20), "Name:");
			GUI.Label(new Rect(160, 40,50,20), "Kills:");
			GUI.Label(new Rect(210, 40,50,20), "Deaths:");
			GUI.Label(new Rect(270, 40,50,20), "Score:");
			
			//team 1
			int yOffset = 0;
			GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), "Team 1:");
			yOffset++;
			for (int i=0; i<theNetwork.players.Count; i++){
				GUI.color = new Color(1f, 0f, 0f, 1f);
				if (theNetwork.players[i].team == 1){
					
					if (theNetwork.players[i].local) GUI.color = new Color(1, 0.3f, 0.3f, 1f);
					if (SophieNetworkScript.gameOver && theNetwork.team1Score>theNetwork.team2Score) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
					
					GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), theNetwork.players[i].name);
					GUI.Label(new Rect(160,(yOffset*20) + 60,50,20), theNetwork.players[i].kills.ToString());
					GUI.Label(new Rect(210,(yOffset*20) + 60,50,20), theNetwork.players[i].deaths.ToString());
					GUI.Label(new Rect(270,(yOffset*20) + 60,50,20), theNetwork.players[i].currentScore.ToString());
					
					yOffset++;
				}
			}
			
			GUI.color = guiCol;
			
			//team 2
			yOffset++;
			GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), "Team 2:");
			yOffset++;
			for (int i=0; i<theNetwork.players.Count; i++){
				GUI.color = new Color(0f, 1f, 1f, 1f);
				if (theNetwork.players[i].team == 2){
					
					if (theNetwork.players[i].local) GUI.color = new Color(0.3f, 1, 1, 1f);
					if (SophieNetworkScript.gameOver && theNetwork.team2Score>theNetwork.team1Score) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
				
					GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), theNetwork.players[i].name);
					GUI.Label(new Rect(160,(yOffset*20) + 60,50,20), theNetwork.players[i].kills.ToString());
					GUI.Label(new Rect(210,(yOffset*20) + 60,50,20), theNetwork.players[i].deaths.ToString());
					GUI.Label(new Rect(270,(yOffset*20) + 60,50,20), theNetwork.players[i].currentScore.ToString());
					
					yOffset++;
				}
			}
			
			GUI.color = Color.black;
			yOffset++;
			if (!SophieNetworkScript.gameOver) GUI.Label(new Rect(10,(yOffset*20) + 60,300,20), ">> TO CHANGE TEAMS, PRESS 'T' <<");
		}
		
		GUI.color = guiCol;
		
		GUI.EndGroup();
	}
	
	string FormatName(string nameString){
		string returnString = "";
		
		for (int i=0; i<nameString.Length; i++){
			bool pass = true;
			if (nameString.Substring(i,1) == " "){
				if (i<nameString.Length-1){
					if (nameString.Substring(i+1,1) == " ") pass = false;
				}
			}
			if (nameString.Substring(i,1) == "\n") pass = false;
			if (nameString.Substring(i,1) == "	") pass = false;
			if (pass) returnString += nameString.Substring(i,1);
		}
		
		return returnString;
	}
	
	void PersonaliseMenu(){
		GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name: ");
		theNetwork.localPlayer.name = GUILayout.TextField(theNetwork.localPlayer.name);
		if (theNetwork.localPlayer.name.Length>20) theNetwork.localPlayer.name = theNetwork.localPlayer.name.Substring(0,20);
		theNetwork.localPlayer.name = FormatName(theNetwork.localPlayer.name);
		GUILayout.EndHorizontal();
			
		GUILayout.Label("Body Color:");
		theNetwork.localPlayer.colA.r = GUILayout.HorizontalSlider(theNetwork.localPlayer.colA.r,0f,1f);
		theNetwork.localPlayer.colA.g = GUILayout.HorizontalSlider(theNetwork.localPlayer.colA.g,0f,1f);
		theNetwork.localPlayer.colA.b = GUILayout.HorizontalSlider(theNetwork.localPlayer.colA.b,0f,1f);
				
		GUILayout.Label("Scarf Color:");
		theNetwork.localPlayer.colB.r = GUILayout.HorizontalSlider(theNetwork.localPlayer.colB.r,0f,1f);
		theNetwork.localPlayer.colB.g = GUILayout.HorizontalSlider(theNetwork.localPlayer.colB.g,0f,1f);
		theNetwork.localPlayer.colB.b = GUILayout.HorizontalSlider(theNetwork.localPlayer.colB.b,0f,1f);
		
		GUILayout.Label("Scarf Stripe Color:");
		theNetwork.localPlayer.colC.r = GUILayout.HorizontalSlider(theNetwork.localPlayer.colC.r,0f,1f);
		theNetwork.localPlayer.colC.g = GUILayout.HorizontalSlider(theNetwork.localPlayer.colC.g,0f,1f);
		theNetwork.localPlayer.colC.b = GUILayout.HorizontalSlider(theNetwork.localPlayer.colC.b,0f,1f);
				
		GUILayout.Label("");
				
		if (GUILayout.Button("Head type: " + theNetwork.localPlayer.headType.ToString())){
			theNetwork.localPlayer.headType++;
			if (theNetwork.localPlayer.headType>17) theNetwork.localPlayer.headType = 0;
		}
		if (theNetwork.localPlayer.headType == 11) GUILayout.Label("Head Credit: @Ast3c");
		if (theNetwork.localPlayer.headType == 12) GUILayout.Label("Head Credit: @IcarusTyler");
		if (theNetwork.localPlayer.headType == 13) GUILayout.Label("Head Credit: @LeanderCorp");
		if (theNetwork.localPlayer.headType == 14) GUILayout.Label("Head Credit: @kagai_shan");
		if (theNetwork.localPlayer.headType == 15) GUILayout.Label("Head Credit: @Ast3c");
		if (theNetwork.localPlayer.headType == 16) GUILayout.Label("Head Credit: @Ast3c");
		if (theNetwork.localPlayer.headType == 17) GUILayout.Label("Head Credit: @Ast3c");
				
		//save player
		if (theNetwork.localPlayer.name != "" && theNetwork.localPlayer.name != " "){
			PlayerPrefs.SetString("PlayerName", theNetwork.localPlayer.name);
		}else{
			PlayerPrefs.SetString("PlayerName", "Player Playerson");
		}
		PlayerPrefs.SetInt("PlayerHead", theNetwork.localPlayer.headType);
		PlayerPrefs.SetFloat("PlayerColA_R", theNetwork.localPlayer.colA.r);
		PlayerPrefs.SetFloat("PlayerColA_G", theNetwork.localPlayer.colA.g);
		PlayerPrefs.SetFloat("PlayerColA_B", theNetwork.localPlayer.colA.b);
		PlayerPrefs.SetFloat("PlayerColB_R", theNetwork.localPlayer.colB.r);
		PlayerPrefs.SetFloat("PlayerColB_G", theNetwork.localPlayer.colB.g);
		PlayerPrefs.SetFloat("PlayerColB_B", theNetwork.localPlayer.colB.b);
		PlayerPrefs.SetFloat("PlayerColC_R", theNetwork.localPlayer.colC.r);
		PlayerPrefs.SetFloat("PlayerColC_G", theNetwork.localPlayer.colC.g);
		PlayerPrefs.SetFloat("PlayerColC_B", theNetwork.localPlayer.colC.b);
		
		GUI.EndGroup();
	}
	
	void KickMenu(){
		GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
		
		GUI.Label(new Rect(250,0,100,20), "Kick a player:");
		
		GUILayout.Label("\n\n\n");
		
		for (int i=0; i<theNetwork.players.Count; i++){
			if (theNetwork.players[i].viewID != theNetwork.localPlayer.viewID){
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Kick")){
					theNetwork.Kick(i);
				}
				string pingString = "?";
				if (theNetwork.players[i].ping.isDone) pingString = theNetwork.players[i].ping.time.ToString();
				GUILayout.Label("- " + theNetwork.players[i].name + " - [Ping: " + pingString + "]");
				GUILayout.EndHorizontal();
			}
		}
		
		GUI.EndGroup();
	}
	
	private int fsWidth = 1024;
	private int fsHeight = 768;
	
	void Config(){
		GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
		
		GUI.Label(new Rect(250,0,100,20), "Config:");
		
		GUILayout.BeginArea(new Rect(5,10,280,380));
		
		GUILayout.Label("---------- Mouse: ----------");
		
		invX = GUILayout.Toggle(invX, "Invert X");
		invY = GUILayout.Toggle(invY, "Invert Y");
		GUILayout.Label("Mouse Sensitivity:");
		mouseSensitivity = GUILayout.HorizontalSlider(mouseSensitivity,0.1f,10f);
		
		
		
		if (GUILayout.Button("Reset Mouse")){
			invX = false;
			invY = false;
			mouseSensitivity = 2f;
			messageScript.textFadeTime = 10f;
		}
		
		
		GUILayout.Label("");
		GUILayout.Label("---------- Misc Settings: ----------");
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Chat messages fade time: ");
		messageScript.textFadeTime = (float)MakeInt(GUILayout.TextField(messageScript.textFadeTime.ToString()));
		GUILayout.EndHorizontal();
		
		theNetwork.gunBobbing = GUILayout.Toggle(theNetwork.gunBobbing, "Gun Bobbing");
		theNetwork.autoPickup = GUILayout.Toggle(theNetwork.autoPickup, "Auto-Pickup");
		theNetwork.autoPickupHealth = GUILayout.Toggle(theNetwork.autoPickupHealth, "Auto-Pickup Health");
		
		GUILayout.BeginHorizontal();
		fsWidth = MakeInt(GUILayout.TextField(fsWidth.ToString()));
		fsHeight = MakeInt(GUILayout.TextField(fsHeight.ToString()));
		if (GUILayout.Button("Fullscreen!")){
			Screen.SetResolution(fsWidth, fsHeight, true);
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Audio Volume:");
		theNetwork.gameVolume = GUILayout.HorizontalSlider(theNetwork.gameVolume,0.0f,1f);
		
		
		if (invX) PlayerPrefs.SetInt("InvertX", 1);
		if (!invX) PlayerPrefs.SetInt("InvertX", 0);
		if (invY) PlayerPrefs.SetInt("InvertY", 1);
		if (!invY) PlayerPrefs.SetInt("InvertY", 0);
		PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
		PlayerPrefs.SetFloat("textFadeTime", messageScript.textFadeTime);
		if (theNetwork.gunBobbing){
			PlayerPrefs.SetInt("GunBobbing", 1);
		}else{
			PlayerPrefs.SetInt("GunBobbing", 0);
		}
		if (theNetwork.autoPickup){
			PlayerPrefs.SetInt("autoPickup", 1);
		}else{
			PlayerPrefs.SetInt("autoPickup", 0);
		}
		
		if (theNetwork.autoPickupHealth){
			PlayerPrefs.SetInt("autoPickupHealth", 1);
		}else{
			PlayerPrefs.SetInt("autoPickupHealth", 0);
		}
		
		PlayerPrefs.SetFloat("GameVolume", theNetwork.gameVolume);
		
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(305,10,280,380));
		
		
		
		GUILayout.Label("---------- Controls: ----------");
		
		GUILayout.Label("W,A,S,D - Move");
		GUILayout.Label("Space - Jump");
		GUILayout.Label("Left Shift/Ctrl - Crouch");
		GUILayout.Label("Left Click - Shoot");
		GUILayout.Label("Q/Right Click - Swap Weapon");
		GUILayout.Label("E - Pickup Weapon (replaces weapon in hand)");
		GUILayout.Label("K - Kill yourself");
		GUILayout.Label("");
		GUILayout.Label("T - Chat");
		GUILayout.Label("Tab - Scoreboard");
		GUILayout.Label("Tab+T - Swap Teams");
		
		GUILayout.EndArea();
		
		GUI.EndGroup();
	}
	
	void GameSetup(bool gameRunning){
		
		GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
		
		if (!gameRunning){
			GUI.Label(new Rect(250,0,100,20), "Host a game:");
		}else{
			GUI.Label(new Rect(250,0,100,20), "Change game:");
		}
				
		if (!gameRunning){
			//set up server name/pass
			GUILayout.BeginArea(new Rect(5,20,290,400));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Game Name: ");
			theNetwork.gameName = GUILayout.TextField(theNetwork.gameName);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Game Password: ");
			theNetwork.password = GUILayout.TextField(theNetwork.password);
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
					
			//set up server connections/port
			GUILayout.BeginArea(new Rect(305,20,290,400));
			GUILayout.Label("Max Connections: " + theNetwork.connections.ToString());
			theNetwork.connections = (int)Mathf.Round(GUILayout.HorizontalSlider(theNetwork.connections,2,32));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Port: ");
			theNetwork.listenPort = MakeInt(GUILayout.TextField(theNetwork.listenPort.ToString()));
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
				
		//game mode
		if (GUI.Button(new Rect(5,100,30,30), "<")){
			int lastInt = gameModeInt;
			
			gameModeInt--;
			//hostLevelSelectInt = 0;
			if (gameModeInt<0) gameModeInt+=modes.Length;
			
			int levelChangeIndex = 0;
			for (int i=0; i<modes[gameModeInt].allowedLevels.Length; i++){
				if (modes[gameModeInt].allowedLevels[i] == modes[lastInt].allowedLevels[hostLevelSelectInt]) levelChangeIndex = i;
			}
			hostLevelSelectInt = levelChangeIndex;
		}
		if (GUI.Button(new Rect(255,100,30,30), ">")){
			int lastInt = gameModeInt;
			
			gameModeInt++;
			//hostLevelSelectInt = 0;
			if (gameModeInt>=modes.Length) gameModeInt-=modes.Length;
			
			int levelChangeIndex = 0;
			for (int i=0; i<modes[gameModeInt].allowedLevels.Length; i++){
				if (modes[gameModeInt].allowedLevels[i] == modes[lastInt].allowedLevels[hostLevelSelectInt]) levelChangeIndex = i;
			}
			hostLevelSelectInt = levelChangeIndex;
		}
		GUI.Label(new Rect(60,100,200,30), "Mode: " + modes[gameModeInt].gameModeName);
				
		//game level
		if (GUI.Button(new Rect(305,100,30,30), "<")){
			hostLevelSelectInt--;
			if (hostLevelSelectInt<0) hostLevelSelectInt+=modes[gameModeInt].allowedLevels.Length;
		}
		if (GUI.Button(new Rect(555,100,30,30), ">")){
			hostLevelSelectInt++;
			if (hostLevelSelectInt>=modes[gameModeInt].allowedLevels.Length) hostLevelSelectInt-=modes[gameModeInt].allowedLevels.Length;
		}
		GUI.Label(new Rect(360,100,200,30), "Level: " + modes[gameModeInt].allowedLevels[hostLevelSelectInt]);
				
				
				
		if (gameModeInt != 0){
			//not custom
			
			//show icon
			for (int i=0; i<levels.Length; i++){
				if (levels[i].name == modes[gameModeInt].allowedLevels[hostLevelSelectInt]){
					GUI.DrawTexture(new Rect(5,135,590,100), levels[i].icon);
				}
			}
			
			//description:
			GUI.Label(new Rect(5,240,590,200), modes[gameModeInt].gameModeName + ":\n" + modes[gameModeInt].gameModeDescription);
			
			if (GUI.Button(new Rect(495,240,100,25), "Customise...")){
				
				int levelChangeIndex = 0;
				for (int i=0; i<modes[0].allowedLevels.Length; i++){
					if (modes[0].allowedLevels[i] == modes[gameModeInt].allowedLevels[hostLevelSelectInt]) levelChangeIndex = i;
				}
				hostLevelSelectInt = levelChangeIndex;
				
				modes[0].killsIncreaseScore = modes[gameModeInt].killsIncreaseScore;
				modes[0].deathsSubtractScore = modes[gameModeInt].deathsSubtractScore;
				modes[0].respawnWait = modes[gameModeInt].respawnWait;
				modes[0].teamBased = modes[gameModeInt].teamBased;
				modes[0].allowFriendlyFire = modes[gameModeInt].allowFriendlyFire;
				modes[0].pitchBlack = modes[gameModeInt].pitchBlack;
				modes[0].gameTime = modes[gameModeInt].gameTime;
				modes[0].winScore = modes[gameModeInt].winScore;
				modes[0].spawnGunA = modes[gameModeInt].spawnGunA;
				modes[0].spawnGunB = modes[gameModeInt].spawnGunB;
				modes[0].offhandCooldown = modes[gameModeInt].offhandCooldown;
				modes[0].scoreAirrockets = modes[gameModeInt].scoreAirrockets;
				modes[0].pickupSlot1 = modes[gameModeInt].pickupSlot1;
				modes[0].pickupSlot2 = modes[gameModeInt].pickupSlot2;
				modes[0].pickupSlot3 = modes[gameModeInt].pickupSlot3;
				modes[0].pickupSlot4 = modes[gameModeInt].pickupSlot4;
				modes[0].pickupSlot5 = modes[gameModeInt].pickupSlot5;
				modes[0].restockTime = modes[gameModeInt].restockTime;
				modes[0].playerLives = modes[gameModeInt].playerLives;
				modes[0].basketball = modes[gameModeInt].basketball;
				
				//hostLevelSelectInt = 0;
				
				gameModeInt = 0;
			}
			
			
		}else{
			//custom, show options here
			
			scrollPos = GUI.BeginScrollView(new Rect(5,135,590,160), scrollPos, new Rect(0,0,570,700));
			
			modes[gameModeInt].killsIncreaseScore = GUILayout.Toggle(modes[gameModeInt].killsIncreaseScore, "Kills Increase score");
			modes[gameModeInt].deathsSubtractScore = GUILayout.Toggle(modes[gameModeInt].deathsSubtractScore, "Deaths Reduce score");
			modes[gameModeInt].scoreAirrockets = GUILayout.Toggle(modes[gameModeInt].scoreAirrockets, "Air-Rocket Bonus");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Respawn Time: ");
			modes[gameModeInt].respawnWait = MakeInt( GUILayout.TextField(modes[gameModeInt].respawnWait.ToString()) );
			GUILayout.EndHorizontal();
			
			modes[gameModeInt].teamBased = GUILayout.Toggle(modes[gameModeInt].teamBased, "Team Based");
			
			if (modes[gameModeInt].teamBased){
				modes[gameModeInt].basketball = GUILayout.Toggle(modes[gameModeInt].basketball, "Basketball");
			}else{
				modes[gameModeInt].basketball = false;
			}
			
			
			modes[gameModeInt].allowFriendlyFire = GUILayout.Toggle(modes[gameModeInt].allowFriendlyFire, "Allow Friendly Fire");
			
			modes[gameModeInt].pitchBlack = GUILayout.Toggle(modes[gameModeInt].pitchBlack, "Pitch Black");
			
			GUILayout.Label(" --- Round end conditions (set to 0 to ignore) --- ");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Round Time (minutes): ");
			modes[gameModeInt].gameTime = MakeInt( GUILayout.TextField(modes[gameModeInt].gameTime.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Winning score: ");
			modes[gameModeInt].winScore = MakeInt( GUILayout.TextField(modes[gameModeInt].winScore.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Player Lives: ");
			modes[gameModeInt].playerLives = MakeInt( GUILayout.TextField(modes[gameModeInt].playerLives.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.Label(" --- Weapon Settings --- ");
			
			//spawn gun A
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[gameModeInt].spawnGunA--;
			if (modes[gameModeInt].spawnGunA<-2) modes[gameModeInt].spawnGunA = artillery.gunTypes.Length-1;
			string gunName = "none";
			if (modes[gameModeInt].spawnGunA==-2) gunName = "random";
			if (modes[gameModeInt].spawnGunA>=0) gunName = artillery.gunTypes[modes[gameModeInt].spawnGunA].gunName;
			GUILayout.Label("Spawn Gun A: " + gunName);
			if (GUILayout.Button(">")) modes[gameModeInt].spawnGunA++;
			if (modes[gameModeInt].spawnGunA>=artillery.gunTypes.Length) modes[gameModeInt].spawnGunA = -2;
			GUILayout.EndHorizontal();
			//spawn gun B
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[gameModeInt].spawnGunB--;
			if (modes[gameModeInt].spawnGunB<-2) modes[gameModeInt].spawnGunB = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[gameModeInt].spawnGunB==-2) gunName = "random";
			if (modes[gameModeInt].spawnGunB>=0) gunName = artillery.gunTypes[modes[gameModeInt].spawnGunB].gunName;
			GUILayout.Label("Spawn Gun B: " + gunName);
			if (GUILayout.Button(">")) modes[gameModeInt].spawnGunB++;
			if (modes[gameModeInt].spawnGunB>=artillery.gunTypes.Length) modes[gameModeInt].spawnGunB = -2;
			GUILayout.EndHorizontal();
			
			modes[gameModeInt].offhandCooldown = GUILayout.Toggle(modes[gameModeInt].offhandCooldown, "Offhand Cooldown");
			
			GUILayout.Label(" --- ");
			//gun slot 1
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[gameModeInt].pickupSlot1--;
			if (modes[gameModeInt].pickupSlot1<-3) modes[gameModeInt].pickupSlot1 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[gameModeInt].pickupSlot1==-2) gunName = "random";
			if (modes[gameModeInt].pickupSlot1==-3) gunName = "health";
			if (modes[gameModeInt].pickupSlot1>=0) gunName = artillery.gunTypes[modes[gameModeInt].pickupSlot1].gunName;
			GUILayout.Label("Pickup Slot 1: " + gunName);
			if (GUILayout.Button(">")) modes[gameModeInt].pickupSlot1++;
			if (modes[gameModeInt].pickupSlot1>=artillery.gunTypes.Length) modes[gameModeInt].pickupSlot1 = -3;
			GUILayout.EndHorizontal();
			//gun slot 2
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[gameModeInt].pickupSlot2--;
			if (modes[gameModeInt].pickupSlot2<-3) modes[gameModeInt].pickupSlot2 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[gameModeInt].pickupSlot2==-2) gunName = "random";
			if (modes[gameModeInt].pickupSlot2==-3) gunName = "health";
			if (modes[gameModeInt].pickupSlot2>=0) gunName = artillery.gunTypes[modes[gameModeInt].pickupSlot2].gunName;
			GUILayout.Label("Pickup Slot 2: " + gunName);
			if (GUILayout.Button(">")) modes[gameModeInt].pickupSlot2++;
			if (modes[gameModeInt].pickupSlot2>=artillery.gunTypes.Length) modes[gameModeInt].pickupSlot2 = -3;
			GUILayout.EndHorizontal();
			//gun slot 3
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[gameModeInt].pickupSlot3--;
			if (modes[gameModeInt].pickupSlot3<-3) modes[gameModeInt].pickupSlot3 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[gameModeInt].pickupSlot3==-2) gunName = "random";
			if (modes[gameModeInt].pickupSlot3==-3) gunName = "health";
			if (modes[gameModeInt].pickupSlot3>=0) gunName = artillery.gunTypes[modes[gameModeInt].pickupSlot3].gunName;
			GUILayout.Label("Pickup Slot 3: " + gunName);
			if (GUILayout.Button(">")) modes[gameModeInt].pickupSlot3++;
			if (modes[gameModeInt].pickupSlot3>=artillery.gunTypes.Length) modes[gameModeInt].pickupSlot3 = -3;
			GUILayout.EndHorizontal();
			//gun slot 4
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[gameModeInt].pickupSlot4--;
			if (modes[gameModeInt].pickupSlot4<-3) modes[gameModeInt].pickupSlot4 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[gameModeInt].pickupSlot4==-2) gunName = "random";
			if (modes[gameModeInt].pickupSlot4==-3) gunName = "health";
			if (modes[gameModeInt].pickupSlot4>=0) gunName = artillery.gunTypes[modes[gameModeInt].pickupSlot4].gunName;
			GUILayout.Label("Pickup Slot 4: " + gunName);
			if (GUILayout.Button(">")) modes[gameModeInt].pickupSlot4++;
			if (modes[gameModeInt].pickupSlot4>=artillery.gunTypes.Length) modes[gameModeInt].pickupSlot4 = -3;
			GUILayout.EndHorizontal();
			//gun slot 4
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[gameModeInt].pickupSlot5--;
			if (modes[gameModeInt].pickupSlot5<-3) modes[gameModeInt].pickupSlot5 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[gameModeInt].pickupSlot5==-2) gunName = "random";
			if (modes[gameModeInt].pickupSlot5==-3) gunName = "health";
			if (modes[gameModeInt].pickupSlot5>=0) gunName = artillery.gunTypes[modes[gameModeInt].pickupSlot5].gunName;
			GUILayout.Label("Pickup Slot 5: " + gunName);
			if (GUILayout.Button(">")) modes[gameModeInt].pickupSlot5++;
			if (modes[gameModeInt].pickupSlot5>=artillery.gunTypes.Length) modes[gameModeInt].pickupSlot5 = -3;
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Restock time (seconds): ");
			modes[gameModeInt].restockTime = MakeInt( GUILayout.TextField(modes[gameModeInt].restockTime.ToString()) );
			GUILayout.EndHorizontal();
			
			
			
			GUI.EndScrollView();
			
			
		}
				
		//init button
		if (!gameRunning){
			if (GUI.Button(new Rect(10,310,580,80), "Init Server!")){
				//init a server with the current game mode settings
				
				theNetwork.serverGameChange = true;
				
				Network.incomingPassword = theNetwork.password;
				
				theNetwork.lastGameWasTeamBased = false;
				
				theNetwork.AssignGameSettings(modes[gameModeInt], modes[gameModeInt].allowedLevels[hostLevelSelectInt]);
				
				theNetwork.comment = theNetwork.gameSettings.gameModeName + "\n" + theNetwork.gameSettings.levelName;
				
				bool useNat = !Network.HavePublicAddress();
				Debug.Log("Initialising server, has public address: " + Network.HavePublicAddress().ToString());
				Network.InitializeServer(theNetwork.connections,theNetwork.listenPort, useNat);
				
				menuPoint = "initializingServer";
			}
		}else{
			if (GUI.Button(new Rect(10,310,580,80), "Change Game")){
				//change game here!
				
				theNetwork.serverGameChange = true;
				
				theNetwork.lastGameWasTeamBased = theNetwork.gameSettings.teamBased;
				
				theNetwork.AssignGameSettings(modes[gameModeInt], modes[gameModeInt].allowedLevels[hostLevelSelectInt]);
				theNetwork.gameViewID = Network.AllocateViewID();
				
				theNetwork.RequestGameData();
				
			}
		}
				
		GUI.EndGroup();
	}
	
	private Ping[] hostPings;// = new Ping[0];
	
	void JoinMenu(){
	
				
				GUI.BeginGroup(new Rect((Screen.width/2)-300, (Screen.height/2)-200, 600,400));
				
				GUI.Label(new Rect(250,0,100,20), "Join a game:");
				
				
				GUILayout.BeginArea(new Rect(5,20,290,400));
				if (GUILayout.Button("Refresh Host List")){
					MasterServer.RequestHostList(theNetwork.uniqueGameName);
					hostPings = new Ping[0];
				}
				GUILayout.EndArea();
				GUILayout.BeginArea(new Rect(305,20,290,400));
					GUILayout.BeginHorizontal();
					GUILayout.Label("Game Password: ");
					theNetwork.password = GUILayout.TextField(theNetwork.password);
					GUILayout.EndHorizontal();
				GUILayout.EndArea();
				
				
				HostData[] hostData = MasterServer.PollHostList();
				
				int scrollHeight = hostData.Length * 40;
				if (scrollHeight < 350) scrollHeight = 350;
				
				scrollPos = GUI.BeginScrollView(new Rect(0,50,600,350), scrollPos, new Rect(0,0,550,scrollHeight+40));
				
				
				if (hostData.Length == 0){
					GUILayout.Label("No hosts found!");
				}else{
					if (hostPings.Length == 0){
						//create new pings for all hosts
						hostPings = new Ping[hostData.Length];
						for (int i=0; i<hostData.Length; i++){
					
							string ipString = "";
							for (int k=0; k<hostData[i].ip.Length; k++){
								ipString += hostData[i].ip[k];
								if (k<hostData[i].ip.Length-1) ipString += ".";
							}
							Debug.Log("GettingPing: " + ipString);
							hostPings[i] = new Ping(ipString);
						}
					}
				}
				
				
				for (int i=0; i<hostData.Length; i++){
					
					GUI.DrawTexture(new Rect(5, (i*40), 550, 1), backTex);
					
					if (GUI.Button(new Rect(5,(i*40)+2, 80,36), "Connect")){
						Network.Connect(hostData[i],theNetwork.password);
						menuPoint = "connecting";
					}
					GUI.Label(new Rect(100,(i*40)+2,150, 30), hostData[i].gameName);
					GUI.Label(new Rect(100,(i*40)+17,150, 30), "[" + hostData[i].connectedPlayers.ToString() + "/" + hostData[i].playerLimit.ToString() + "]");
					GUI.Label(new Rect(300,(i*40)+2,150, 60), hostData[i].comment);
					if (hostData[i].passwordProtected)GUI.Label(new Rect(160,(i*40)+17,150, 30), "[PASSWORDED]");
					
					if (hostPings[i].isDone){
						GUI.Label(new Rect(450,(i*40)+17,150, 30), "Ping: " + hostPings[i].time.ToString());
					}else{
						GUI.Label(new Rect(450,(i*40)+17,150, 30), "Ping: ?");
					}
					
					if (i==hostData.Length-1){
						GUI.DrawTexture(new Rect(5, (i*40)+40, 550, 1), backTex);
					}
				}
				
				GUI.EndScrollView();
				
				GUI.EndGroup();	
	}
	
	private int MakeInt(string v) {
		//convert strings to ints!
		return Convert.ToInt32(v.Trim(), new CultureInfo("en-US"));
	}
}
