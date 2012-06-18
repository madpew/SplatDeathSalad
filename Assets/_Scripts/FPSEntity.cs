using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSEntity : MonoBehaviour {
	
	
	public bool isLocal = true;
	
	private CharacterController cc;
	
	public GameObject weaponSoundObj;
	
	
	public GameObject camHolder;
	private Vector3 camAngle;
	private bool crouched = false;
	
	public FPSPlayer thisPlayer;
	
	//public NetworkViewID viewID;
	
	public string offeredPickup = "";
	public PickupBoxScript currentOfferedPickup;
	
	public SophieNetworkScript theNetwork;
	private FPSGUI theGUI;
	private FPSArtillery artillery;
	
	public GameObject animObj;
	
	private Vector3 swapperLock = Vector3.zero;
	
	private Vector3 moveVec = Vector3.zero;
	
	public Material invisibleMat;
	public Material gunMat;
	public Material dummyAMat;
	public Material dummyBMat;
	public Material dummyCMat;
	public Material boxMat;
	public Material fishMat;
	public Material bananaMat;
	public Material creeperMat;
	public Material elephantMat;
	public Material moonMat;
	public Material pyramidMat;
	public Material chocoboMat;
	public Material spikeMat;
	public Material tentacleMat;
	public Material robotHeadMat;
	public Material speaceshipMat;
	public Material enforcerMat;
	public Material smileyMat;
	public Material helmetMat;
	public Material paperbagMat;
	public Material maheadMat;
	
	public Color colA;
	public Color colB;
	public Color colC;
	public int headType = 0;
	
	public GameObject meshObj;
	public GameObject gunMesh1;
	public GameObject gunMesh2;
	
	public GameObject[] heads;
	
	public bool grounded;
	public float yMove = 0f;

	private double lastUpdateTime = -1f;
	
	private Vector3 lastCamAngle = Vector3.zero;
	private Vector3 lastMoveVector = Vector3.zero;
	private bool lastCrouch = false;
	private float lastYmove = 0f;
	private float lastHealth = 0f;
	
	public Light firstPersonLight;
	
	
	public int handGun = 0;
	public float handGunCooldown = 0f;
	public int holsterGun = 1;
	public float holsterGunCooldown = 0f;
	
	
	public int lastKnownHandGun = -99;
	public int lastKnownHolsterGun = -99;
	
	public GameObject firstPersonGun;
	
	public bool spectateMode = false;
	private int spectateInt = 0;
	
	private int swapperLockTarget = -1;
	
	public void SetModelVisibility(bool visible){
		
		
		Material[] mats = meshObj.renderer.materials;
		
		
		Material newMatA = new Material(dummyAMat);
		Material newMatB = new Material(dummyBMat);
		Material newMatC = new Material(dummyCMat);
		newMatA.color = thisPlayer.colA;
		newMatB.color = thisPlayer.colB;
		newMatC.color = thisPlayer.colC;
		
		if (theNetwork.gameSettings.teamBased){
			if (thisPlayer.team == 1) {
				newMatA.color = Color.red;
			}
			if (thisPlayer.team == 2) {
				newMatA.color = Color.cyan;
			}
		}
		
		if (!visible){
			mats[0] = invisibleMat;
			mats[1] = invisibleMat;
			mats[2] = invisibleMat;
			meshObj.renderer.materials = mats;
			
			if (gunMesh1.renderer) gunMesh1.renderer.material = invisibleMat;
			if (gunMesh2.renderer) gunMesh2.renderer.material = invisibleMat;
		}else{
			mats[0] = newMatA;
			mats[1] = newMatB;
			mats[2] = newMatC;
			meshObj.renderer.materials = mats;
			
			//heads[0].renderer.material.color = thisPlayer.colA;
			
			if (handGun>=0 && gunMesh1.renderer) gunMesh1.renderer.material = artillery.gunTypes[handGun].gunMaterial;
			if (holsterGun>=0 && gunMesh2.renderer) gunMesh2.renderer.material = artillery.gunTypes[holsterGun].gunMaterial;
		}
		
		//heads
		for (int i=0; i<heads.Length; i++){
			if (i!=headType){
				heads[i].renderer.enabled = false;
			}
			if (!visible){
				heads[i].renderer.material = invisibleMat;
			}	
		}
		if (visible){
			heads[0].renderer.material = newMatA;
			heads[1].renderer.material = boxMat;
			heads[2].renderer.material = fishMat;
			heads[3].renderer.material = bananaMat;
			heads[4].renderer.material = creeperMat;
			heads[5].renderer.material = elephantMat;
			heads[6].renderer.material = moonMat;
			heads[7].renderer.material = pyramidMat;
			heads[8].renderer.material = chocoboMat;
			heads[9].renderer.material = spikeMat;
			heads[10].renderer.material = tentacleMat;
			heads[11].renderer.material = robotHeadMat;
			heads[12].renderer.material = speaceshipMat;
			heads[13].renderer.material = enforcerMat;
			heads[14].renderer.material = smileyMat;
			heads[15].renderer.material = helmetMat;
			heads[16].renderer.material = paperbagMat;
			heads[17].renderer.material = maheadMat;
		}
		
		if (firstPersonGun != null && thisPlayer.local && firstPersonGun.renderer && handGun>=0){
			if (!visible){
				firstPersonGun.renderer.enabled = true;
				firstPersonGun.renderer.material = artillery.gunTypes[handGun].gunMaterial;
			}else{
				firstPersonGun.renderer.enabled = false;
			}
		}
		
		
		if (!theNetwork.gameSettings.pitchBlack || !thisPlayer.local){
			firstPersonLight.enabled = false;
		
		}
		if (!thisPlayer.local && theNetwork.gameSettings.pitchBlack){
			if (theNetwork.gameSettings.teamBased && thisPlayer.team == theNetwork.localPlayer.team){
				firstPersonLight.enabled = true;
				if (thisPlayer.team == 1){
					firstPersonLight.color = Color.red;
				}else{
					firstPersonLight.color = Color.cyan;
				}
			}
		}
		
		
	}
	
	// Use this for initialization
	void Start () {
		
		theNetwork = GameObject.Find("_SophieNet").GetComponent<SophieNetworkScript>();
		theGUI = GameObject.Find("_SophieNet").GetComponent<FPSGUI>();
		artillery = GameObject.Find("_SophieNet").GetComponent<FPSArtillery>();
		
		if (thisPlayer.local && theNetwork.gameSettings.pitchBlack){
			Camera.main.backgroundColor = Color.black;
			RenderSettings.ambientLight = Color.black;
		}
		
		cc = GetComponent<CharacterController>();
		
		if (thisPlayer.lives>=0){
			if (isLocal){
				SetModelVisibility(false);
			}else{
				SetModelVisibility(true);
			}
			
			Respawn();
		}else{
			//we joined as a spectator
			SetModelVisibility(false);
			transform.position = -Vector3.up * 99f;
		}
		
		
		
		
		
		
		
		
	}
	
	public GameObject ourKiller;
	
	public bool sendRPCUpdate = false;
	
	public AudioClip sfx_takeDamage;
	public AudioClip sfx_jump;
	public AudioClip sfx_land;
	public AudioClip sfx_die;
	public AudioClip sfx_weaponChange;
	public AudioClip sfx_reload;
	public AudioClip sfx_swapped;
	public AudioClip sfx_catchBall;
	
	public void PlaySound(string sound){
		if (sound == "takeHit"){
			audio.clip = sfx_takeDamage;
			audio.Play();
		}
		if (sound == "jump"){
			audio.clip = sfx_jump;
			audio.volume = 1f;
			audio.volume = 0.2f;
			audio.Play();
		}
		if (sound == "land"){
			audio.clip = sfx_land;
			audio.volume = 0.5f;
			audio.Play();
		}
		if (sound == "die"){
			audio.clip = sfx_die;
			audio.volume = 1f;
			audio.Play();
		}
		if (sound == "weaponChange"){
			audio.clip = sfx_weaponChange;
			audio.volume = 0.2f;
			audio.Play();
		}
		if (sound == "reload"){
			audio.clip = sfx_reload;
			audio.volume = 0.2f;
			audio.Play();
		}
		if (sound == "Swapped"){
			audio.clip = sfx_swapped;
			audio.volume = 0.4f;
			audio.Play();
		}
		if (sound == "catchBall"){
			audio.clip = sfx_catchBall;
			audio.volume = 0.4f;
			audio.Play();
		}
	}
	
	private float rpcCamtime = 0f;
	
	// Update is called once per frame
	void Update () {
		
		//volume
		AudioListener.volume = theNetwork.gameVolume;
		
		theGUI.spectateMode = spectateMode;
		theGUI.spectateInt = spectateInt;
		
		if (spectateMode && isLocal){
			if (theNetwork.players.Count>0){
				if (firstPersonGun) firstPersonGun.renderer.enabled = false;
				
				if (Input.GetKeyDown("mouse 0")) spectateInt++;
				if (spectateInt>=theNetwork.players.Count) spectateInt = 0;
				if (theNetwork.players[spectateInt].lives<=0) spectateInt++;
				if (spectateInt>=theNetwork.players.Count) spectateInt = 0;
				
				Camera.main.transform.parent = null;
				Camera.main.transform.position = theNetwork.players[spectateInt].fpsEntity.transform.position;
				float invX = 1f;
				float invY = 1f;
				if (theGUI.invX) invX = -1f;
				if (theGUI.invY) invY = -1f;
				camAngle.x -= Input.GetAxis("Mouse Y") * Time.deltaTime * 30f * theGUI.mouseSensitivity * invY;
				camAngle.y += Input.GetAxis("Mouse X") * Time.deltaTime * 30f * theGUI.mouseSensitivity * invX;
				if (camAngle.x>85f) camAngle.x = 85f;
				if (camAngle.x<-85f) camAngle.x = -85f;
				Camera.main.transform.eulerAngles = camAngle;
				Camera.main.transform.Translate(0,0,-3);
			}
			
			return;
		}
		
		
		if (isLocal){
			if (spectateMode){
				
				
				
				
			}else{
				Vector3 lastPos = transform.position;
				if (thisPlayer.health>0f){
					
					
					theGUI.offeredPickup = offeredPickup;
					if (offeredPickup != ""){
						
						bool pickup = false;
						if (!theNetwork.autoPickup && Input.GetKeyDown("e")) pickup = true;
						if (theNetwork.autoPickup) pickup = true;
						
						if (pickup){
							//pickup weapon.
							
							for (int i=0; i<artillery.gunTypes.Length; i++){
								if (offeredPickup == artillery.gunTypes[i].gunName){
									handGun = i;
									handGunCooldown = 0f;
									
									gunRecoil += Vector3.right * 5f;
									gunRecoil -= Vector3.up * 5f;
									PlaySound("weaponChange");
									
									currentOfferedPickup.Pickup();
								}
							}
							if (offeredPickup == "health" && thisPlayer.health<100f){
								
								theNetwork.ConsumeHealth(thisPlayer.viewID);
								theNetwork.localPlayer.health = 100f;
								thisPlayer.health = 100f;
								
								PlaySound("weaponChange");
								
								currentOfferedPickup.Pickup();
							}
							
							
							
						}
					}
				}
				offeredPickup = "";
				
				
				theGUI.gunA = handGun;
				theGUI.gunACooldown = handGunCooldown;
				theGUI.gunB = holsterGun;
				
				if (thisPlayer.health>0f){
					
					
					
					if (Camera.main.transform.parent == null) SetModelVisibility(false);
					
					ourKiller = null;
					//Debug.Log(GameSettings.respawnWait);
					
					Camera.main.transform.parent = camHolder.transform;
					Camera.main.transform.localPosition = Vector3.zero;
					Camera.main.transform.localEulerAngles = Vector3.zero;
					
					float invX = 1f;
					float invY = 1f;
					if (theGUI.invX) invX = -1f;
					if (theGUI.invY) invY = -1f;
					
					camAngle.x -= Input.GetAxis("Mouse Y") * Time.deltaTime * 30f * theGUI.mouseSensitivity * invY;
					camAngle.y += Input.GetAxis("Mouse X") * Time.deltaTime * 30f * theGUI.mouseSensitivity * invX;
					if (camAngle.x>85f) camAngle.x = 85f;
					if (camAngle.x<-85f) camAngle.x = -85f;
					
					camHolder.transform.eulerAngles = camAngle;
					
					Vector3 inputVector = Vector3.zero;
					if (Input.GetKey("w")) inputVector += Camera.main.transform.forward;
					if (Input.GetKey("s")) inputVector -= Camera.main.transform.forward;
					if (Input.GetKey("d")) inputVector += Camera.main.transform.right;
					if (Input.GetKey("a")) inputVector -= Camera.main.transform.right;
					inputVector.y = 0f;
					inputVector.Normalize();
					
					if (!crouched){
						cc.Move(inputVector * Time.deltaTime * 10f);
					}else{
						cc.Move(inputVector * Time.deltaTime * 5f);
					}
					
					
					if (yMove<=0f){
						cc.Move(Vector3.up * -0.2f);
						bool landed = grounded;
						grounded = cc.isGrounded;
						if (!grounded) cc.Move(Vector3.up * 0.2f);
						
						if (!landed && grounded){
							PlaySound("land");
							sendRPCUpdate = true;
						}
					}else{
						grounded = false;
					}
					
					if (grounded){
						yMove = 0f;
						if (Input.GetKeyDown("space")){
							yMove = 4f;
							PlaySound("jump");
							sendRPCUpdate = true;
						}
					}else{
						yMove -= Time.deltaTime * 10f;
					}
					cc.Move(Vector3.up * yMove * Time.deltaTime * 5f);
					
					crouched = false;
					if (Input.GetKey("left shift") || Input.GetKey("left ctrl")) crouched = true;
					
					moveVec = inputVector;
					
					Ray lavaRay = new Ray( lastPos, transform.position - lastPos);
					RaycastHit lavaHit = new RaycastHit();
					float lavaRayLength = Vector3.Distance(transform.position, lastPos);
					int lavaLayer = (1<<10);
					if (Physics.Raycast(lavaRay, out lavaHit, lavaRayLength, lavaLayer)){
						transform.position = lavaHit.point+ (Vector3.up*0.35f);
						sendRPCUpdate = true;
						inputVector = Vector3.zero;
						theNetwork.RegisterHit("lava", thisPlayer.viewID, thisPlayer.viewID, lavaHit.point);
					}
					
					
					//sendRPCUpdate = false;
					if (camAngle != lastCamAngle && Time.time>rpcCamtime) sendRPCUpdate = true;
					if (moveVec != lastMoveVector) sendRPCUpdate = true;
					if (crouched != lastCrouch) sendRPCUpdate = true;
					//if (yMove != lastYmove) sendRPCUpdate = true;
					if (thisPlayer.health != lastHealth) sendRPCUpdate = true;
					if (theNetwork.broadcastPos){
						theNetwork.broadcastPos = false;
						sendRPCUpdate = true;
					}
					
					lastCamAngle = camAngle;
					lastMoveVector = moveVec;
					lastCrouch = crouched;
					lastYmove = yMove;
					lastHealth = thisPlayer.health;
					
					if (sendRPCUpdate){
						theNetwork.SendPlayer(thisPlayer.viewID, transform.position, camAngle, crouched, moveVec, yMove, handGun, holsterGun);
						sendRPCUpdate = false;
						
						rpcCamtime = Time.time;// + 0.02f;
					}
					
					if (handGun>=0 && handGunCooldown > 0f && handGunCooldown - Time.deltaTime <= 0f && artillery.gunTypes[handGun].fireCooldown>=0.5f) PlaySound("reload");
					handGunCooldown -= Time.deltaTime;
					if (handGunCooldown<0f) handGunCooldown = 0f;
					
					
					theGUI.swapperLocked = false;
					swapperLockTarget = -1;
					if (handGun == 5){
						//swapper aiming
						List<int> validSwapTargets = new List<int>();
						
						for (int i=0; i<theNetwork.players.Count; i++){
							if (!theNetwork.players[i].local && Vector3.Dot(Camera.main.transform.forward, (theNetwork.players[i].fpsEntity.transform.position - Camera.main.transform.position).normalized) > 0.94f){
								
								Ray swapCheckRay = new Ray(Camera.main.transform.position, theNetwork.players[i].fpsEntity.transform.position - Camera.main.transform.position);
								RaycastHit swapCheckHit = new RaycastHit();
								int swapCheckLayer = 1<<0;
								float swapCheckLength = Vector3.Distance(theNetwork.players[i].fpsEntity.transform.position, Camera.main.transform.position);
								if (!Physics.Raycast(swapCheckRay, out swapCheckHit, swapCheckLength, swapCheckLayer)){
									validSwapTargets.Add(i);
									theGUI.swapperLocked = true;
								}
							}
						}
						int nearestScreenspacePlayer = 0;
						float nearestDistance = 9999f;
						for (int i=0; i<validSwapTargets.Count; i++){
							Vector3 thisPos = Camera.main.WorldToScreenPoint(theNetwork.players[validSwapTargets[i]].fpsEntity.transform.position);
							if (Vector3.Distance(thisPos, new Vector3(Screen.width/2,Screen.height/2,0))<nearestDistance){
								nearestScreenspacePlayer = validSwapTargets[i];
							}
						}
						if (theGUI.swapperLocked){
							//move target to locked on player
							Vector3 screenPos = Camera.main.WorldToScreenPoint(theNetwork.players[nearestScreenspacePlayer].fpsEntity.transform.position);
							swapperLock -= (swapperLock-screenPos) * Time.deltaTime * 10f;
							swapperLockTarget = nearestScreenspacePlayer;
						}else{
							//move target to center
							swapperLock -= (swapperLock-new Vector3(Screen.width/2, Screen.height/2, 0)) * Time.deltaTime * 10f;
						}
					}else{
						swapperLock = new Vector3(Screen.width/2, Screen.height/2, 0);
					}
					
					theGUI.swapperCrossX = Mathf.RoundToInt(swapperLock.x);
					theGUI.swapperCrossY = Mathf.RoundToInt(swapperLock.y);
					
					
					if (handGun>=0 && !thisPlayer.hasBall){
						//shooting
						if (Input.GetKeyDown("mouse 0") && Screen.lockCursor == true && !artillery.gunTypes[handGun].isAutomatic){
							if (handGunCooldown<=0f){
								Fire();
								handGunCooldown += artillery.gunTypes[handGun].fireCooldown;
							}
						}else if (Input.GetKey("mouse 0") && Screen.lockCursor == true && artillery.gunTypes[handGun].isAutomatic){
							if (handGunCooldown<=0f){
								Fire();
								handGunCooldown += artillery.gunTypes[handGun].fireCooldown;
							}
						}
					}
					if ((Input.GetKeyDown("mouse 1") || Input.GetKeyDown("q")) && Screen.lockCursor == true){
						//swap guns
						int tempInt = handGun;
						float tempFloat = handGunCooldown;
						handGun = holsterGun;
						handGunCooldown = holsterGunCooldown;
						holsterGun = tempInt;
						holsterGunCooldown = tempFloat;
						
						gunRecoil += Vector3.right * 5f;
						gunRecoil -= Vector3.up * 5f;
						PlaySound("weaponChange");
					}
					
					//ball throwing
					if (thisPlayer.hasBall && Input.GetKeyDown("mouse 0") && Screen.lockCursor == true){
						theNetwork.ThrowBall(Camera.main.transform.position, Camera.main.transform.forward, 20f);
						
					}
					
					if (Input.GetKeyDown("k")){
						theNetwork.RegisterHitRPC("suicide",thisPlayer.viewID,thisPlayer.viewID,transform.position);
					}
					
					moveFPGun();
				
				}else{
					//we dead
					
					if (Camera.main.transform.parent != null) SetModelVisibility(true);
					
					if (ourKiller!=null){
						Camera.main.transform.parent = null;
						Camera.main.transform.position = transform.position - animObj.transform.forward;
						Camera.main.transform.LookAt(ourKiller.transform.position);
						Camera.main.transform.Translate(0,0,-2f);
					}
				}
			}
		}else{
			if (lastUpdateTime>0f){
				
				NonLocalUpdate();
				
				
			}
		}
		
		if (!crouched){
			camHolder.transform.localPosition = Vector3.up * 0.7f;
		}else{
			camHolder.transform.localPosition = Vector3.zero;
		}
		
		
		
		//visible person model anims
		Vector3 lookDir = camHolder.transform.forward;
		lookDir.y = 0;
		lookDir.Normalize();
		animObj.transform.LookAt(animObj.transform.position + lookDir);
		
		if (thisPlayer.health>0f){
			if (yMove == 0f){
				if (moveVec.magnitude>0.1f){
					if (crouched){
						animObj.animation.Play("crouchrun");
					}else{
						animObj.animation.Play("run");
					}
					if (Vector3.Dot(moveVec, lookDir)<-0.5f){
						animObj.animation["crouchrun"].speed = -1;
						animObj.animation["run"].speed = -1;
					}else{
						animObj.animation["crouchrun"].speed = 1;
						animObj.animation["run"].speed = 1;
					}
				}else{
					if (crouched){
						animObj.animation.Play("crouch");
					}else{
						animObj.animation.Play("idle");
					}
				}
			}else{
				if (yMove>0f){
					animObj.animation.Play("rise");
				}else{
					animObj.animation.Play("fall");
				}
			}
		}else{
			animObj.animation.Play("die");
		}
		
		//show correct guns
		if (handGun != lastKnownHandGun){
			Transform gunParent = gunMesh1.transform.parent;
			Destroy(gunMesh1);
			if (handGun>=0){
				gunMesh1 = (GameObject)GameObject.Instantiate(artillery.gunTypes[handGun].modelPrefab);
			}else{
				gunMesh1 = new GameObject();
			}
			gunMesh1.transform.parent = gunParent;
			gunMesh1.transform.localEulerAngles = new Vector3(0,180,90);
			gunMesh1.transform.localPosition = Vector3.zero;
			lastKnownHandGun = handGun;
			
			if (thisPlayer.local){
				
				
				
				if (firstPersonGun != null) Destroy(firstPersonGun);
				if (handGun>=0){
					firstPersonGun = (GameObject)GameObject.Instantiate(artillery.gunTypes[handGun].modelPrefab);
				}else{
					firstPersonGun = new GameObject();
				}
				firstPersonGun.transform.parent = Camera.main.transform;
				firstPersonGun.transform.localEulerAngles = new Vector3( -90, 0, 0);
				firstPersonGun.transform.localPosition = new Vector3( 0.47f, -0.48f, 0.84f);
				if (firstPersonGun.renderer) firstPersonGun.renderer.castShadows = false;
			}
			
			sendRPCUpdate = true;
			
			if (thisPlayer.health<=0f || !thisPlayer.local){
				SetModelVisibility(true);
			}else{
				SetModelVisibility(false);
			}
		}
		if (holsterGun != lastKnownHolsterGun){
			Transform gunParentB = gunMesh2.transform.parent;
			Destroy(gunMesh2);
			if (holsterGun>=0){
				gunMesh2 = (GameObject)GameObject.Instantiate(artillery.gunTypes[holsterGun].modelPrefab);
			}else{
				gunMesh2 = new GameObject();
			}
			gunMesh2.transform.parent = gunParentB;
			gunMesh2.transform.localEulerAngles = new Vector3(0,180,90);
			gunMesh2.transform.localPosition = Vector3.zero;
			lastKnownHolsterGun = holsterGun;
			
			sendRPCUpdate = true;
			
			if (thisPlayer.health<=0f || !thisPlayer.local){
				SetModelVisibility(true);
			}else{
				SetModelVisibility(false);
			}
		}
		
		
		
		
		//if dead, make unshootable
		if (thisPlayer.health>0f){
			gameObject.layer = 8;
		}else{
			gameObject.layer = 2;
		}
		//if no friendly fire & on same team, make unshootable
		if (theNetwork.gameSettings.teamBased && !theNetwork.gameSettings.allowFriendlyFire){
			if (thisPlayer.team == theNetwork.localPlayer.team){
				gameObject.layer = 2;
			}
		}
		
		if (thisPlayer.hasBall){
			if (thisPlayer.local && firstPersonGun && firstPersonGun.renderer) firstPersonGun.renderer.enabled = false;
		}else{
			if (thisPlayer.local && firstPersonGun && firstPersonGun.renderer && thisPlayer.health>0f) firstPersonGun.renderer.enabled = true;
		}
	}
	
	public GameObject aimBone;
	
	private Vector3 gunInertia = Vector3.zero;
	private Vector3 gunRecoil = Vector3.zero;
	private Vector3 gunRot = Vector3.zero;
	private float gunBounce = 0f;
	
	void moveFPGun(){
		if (firstPersonGun == null) return;
		
		//angle
		firstPersonGun.transform.eulerAngles = gunRot;
		Quaternion fromRot = firstPersonGun.transform.rotation;
		firstPersonGun.transform.localEulerAngles = new Vector3( -90, 0, 0);
		firstPersonGun.transform.rotation = Quaternion.Slerp(fromRot, firstPersonGun.transform.rotation, Time.deltaTime * 30f);
		gunRot = firstPersonGun.transform.eulerAngles;
		
		
		//position
		firstPersonGun.transform.localPosition = new Vector3( 0.47f, -0.48f, 0.84f);
		
		gunInertia -= (gunInertia-new Vector3(0f, yMove, 0f)) * Time.deltaTime * 5f;// * Time.deltaTime;
		if (gunInertia.y<-3f) gunInertia.y = -3f;
		firstPersonGun.transform.localPosition += gunInertia * 0.1f;
		
		float recoilRest = 5f;
		if (handGun == 0) recoilRest = 5f;//pistol
		if (handGun == 1) recoilRest = 8f;//grenade
		if (handGun == 2) recoilRest = 8f;//machinegun
		if (handGun == 3) recoilRest = 2f;//rifle
		if (handGun == 4) recoilRest = 1f;//rocket launcher
		gunRecoil -= (gunRecoil-Vector3.zero) * Time.deltaTime * recoilRest;
		
		
		firstPersonGun.transform.localPosition += gunRecoil * 0.1f;
		
		if (grounded){
			
			
			if (moveVec.magnitude > 0.1f && theNetwork.gunBobbing){
				if (crouched){
					gunBounce += Time.deltaTime * 6f;
				}else{
					gunBounce += Time.deltaTime * 15f;
				}
				firstPersonGun.transform.position += Vector3.up * Mathf.Sin(gunBounce) *0.05f;
			}
			
		}
	}
	
	void Fire(){
		if (handGun == 0){
			//pistol
			FireBullet("pistol");
			gunRecoil -= Vector3.forward * 2f;
		}else if (handGun == 1){
			//grenade
			theNetwork.Shoot("grenade", Camera.main.transform.position, Camera.main.transform.forward, Camera.main.transform.position + Camera.main.transform.forward, theNetwork.localPlayer.viewID, false);
			
			gunRecoil += Vector3.forward * 6f;
		}else if (handGun == 2){
			//machine gun
			FireBullet("machinegun");
			gunRecoil -= Vector3.forward * 2f;
			gunRecoil += new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f)).normalized * 0.2f;
		}else if (handGun == 3){
			//rifle
			FireBullet("rifle");
			gunRecoil -= Vector3.forward * 5f;
		}else if (handGun == 4){
			//rocket launcher
			theNetwork.Shoot("rocketlauncher", Camera.main.transform.position, Camera.main.transform.forward, Camera.main.transform.position + Camera.main.transform.forward, theNetwork.localPlayer.viewID, false);
			gunRecoil -= Vector3.forward * 5f;
		}else if (handGun == 5){
			//swapper
			if (swapperLockTarget == -1){
				//not locked on, we miss
				FireBullet("swapper");
			}else{
				//locked on, we hit
				theNetwork.Shoot("swapper", transform.position, theNetwork.players[swapperLockTarget].fpsEntity.transform.position - transform.position, theNetwork.players[swapperLockTarget].fpsEntity.transform.position , theNetwork.localPlayer.viewID, true);
				theNetwork.RegisterHit("swapper", theNetwork.localPlayer.viewID, theNetwork.players[swapperLockTarget].viewID, theNetwork.players[swapperLockTarget].fpsEntity.transform.position);
				
			}
			gunRecoil -= Vector3.forward * 5f;
		}
			
	}
	
	public void ForceLook(Vector3 targetLookPos){
		GameObject lookObj = new GameObject();
		lookObj.transform.position = Camera.main.transform.position;
		lookObj.transform.LookAt(targetLookPos);
		camAngle = lookObj.transform.eulerAngles;
		
		while (camAngle.x>85f) camAngle.x-=180f;
		while (camAngle.x<-85f) camAngle.x+=180f;
		//Debug.Log("Force look: " + targetLookPos.ToString() + " ??? " + lookObj.transform.position.ToString() + " ??? " + camAngle.ToString());
	}
	
	void FireBullet(string weaponType){
		//fire instant-bullet type gun (change this later when multiple guns do things);
			Vector3 bulletStart = Camera.main.transform.position;
			Vector3 bulletDirection = Camera.main.transform.forward;
			Vector3 bulletEnd = bulletStart + (bulletDirection*999f);
			bool hit = false;
		
			bool registerhit = false;
			int hitPlayer = -1;
		
			if (weaponType == "machinegun"){
				float shakeValue = 0.01f;
				bulletDirection += new Vector3(Random.Range(-shakeValue,shakeValue),Random.Range(-shakeValue,shakeValue),Random.Range(-shakeValue,shakeValue));
				bulletDirection.Normalize();
			}
					
			Ray bulletRay = new Ray(bulletStart, bulletDirection);
			RaycastHit bulletHit = new RaycastHit();
			int bulletLayer = (1<<0)|(1<<8);//walls & players
					
			gameObject.layer = 2;
			if (Physics.Raycast(bulletRay, out bulletHit, 999f, bulletLayer)){
				bulletEnd = bulletHit.point;
						
				if (bulletHit.collider.gameObject.layer == 8){
					//hit a player, tell the server
				
					hit = true;
					
					
					for (int i=0; i<theNetwork.players.Count; i++){
						if (bulletHit.collider.gameObject == theNetwork.players[i].fpsEntity.gameObject){
							hitPlayer = i;
						}
					}
				
					registerhit = true;
					//theNetwork.RegisterHit(weaponType, theNetwork.localPlayer.viewID, theNetwork.players[hitPlayer].viewID, bulletHit.point);
				}
			}else{
				//miss
			}
			gameObject.layer = 8;
					
			bulletStart = transform.position;
			bulletStart = gunMesh1.transform.position + (Camera.main.transform.forward*0.5f);
			
					
			//RPC the shot regardless
			theNetwork.Shoot(weaponType, bulletStart, bulletDirection, bulletEnd, theNetwork.localPlayer.viewID, hit);
		
			if (registerhit) theNetwork.RegisterHit(weaponType, theNetwork.localPlayer.viewID, theNetwork.players[hitPlayer].viewID, bulletHit.point);
	}
	
	public void Respawn(){
		
		Vector3 spawnPos = Vector3.up;
		Vector3 spawnAngle = Vector3.zero;
		
		if (GameObject.Find("_Spawns") != null){
			SpawnPointScript spawns = GameObject.Find("_Spawns").GetComponent<SpawnPointScript>();
			
			if (!theNetwork.gameSettings.teamBased){
				int randomIndex = Random.Range(0,spawns.normalSpawns.Length);
				spawnPos = spawns.normalSpawns[randomIndex].transform.position + Vector3.up;
				spawnAngle = spawns.normalSpawns[randomIndex].transform.eulerAngles;
			}else if (thisPlayer.team == 1){
				int randomIndex = Random.Range(0,spawns.team1Spawns.Length);
				spawnPos = spawns.team1Spawns[randomIndex].transform.position + Vector3.up;
				spawnAngle = spawns.team1Spawns[randomIndex].transform.eulerAngles;
			}else if (thisPlayer.team == 2){
				int randomIndex = Random.Range(0,spawns.team2Spawns.Length);
				spawnPos = spawns.team2Spawns[randomIndex].transform.position + Vector3.up;
				spawnAngle = spawns.team2Spawns[randomIndex].transform.eulerAngles;
			}
			
		}
		
		
		//assign spawn guns
		if (firstPersonGun) Destroy(firstPersonGun);
		if (theNetwork.gameSettings.spawnGunA == -2){
			handGun = Random.Range(0,artillery.gunTypes.Length);
		}else{
			handGun = theNetwork.gameSettings.spawnGunA;
		}
		if (theNetwork.gameSettings.spawnGunB == -2){
			holsterGun = Random.Range(0,artillery.gunTypes.Length);
		}else{
			holsterGun = theNetwork.gameSettings.spawnGunB;
		}
		lastKnownHandGun = -99;
		lastKnownHolsterGun = -99;
		
		
		
		handGunCooldown = 0f;
		holsterGunCooldown = 0f;
		
		transform.position = spawnPos;
		camAngle = spawnAngle;
		yMove = 0f;
		moveVec = Vector3.zero;
	}
	
	void LateUpdate(){
		if (thisPlayer.health>0f){
			aimBone.transform.localEulerAngles += new Vector3(0, camAngle.x, 0);
			animObj.transform.localPosition = (animObj.transform.forward * camAngle.x * -0.002f) - Vector3.up;
		}
	}
	
	void NonLocalUpdate(){
		
		if (thisPlayer.health<=0f) moveVec = Vector3.zero;
		
		if (cc == null) cc = GetComponent<CharacterController>();
		
		float timeDelta = (float)(Network.time - lastUpdateTime);
		lastUpdateTime = Network.time;
		
		
		if (!crouched){
			cc.Move(moveVec * timeDelta * 10f);
		}else{
			cc.Move(moveVec * timeDelta * 5f);
		}
		
		
		if (yMove<=0f){
			cc.Move(Vector3.up * -0.2f);
			grounded = cc.isGrounded;
			if (!grounded) cc.Move(Vector3.up * 0.2f);
		}else{
			grounded = false;
		}
		
		if (grounded){
			yMove = 0f;
		}else{
			yMove -= timeDelta * 10f;
		}
		cc.Move(Vector3.up * yMove * timeDelta * 5f);
		
	}
	
	public void UpdatePlayer(Vector3 pos, Vector3 ang, bool crouch, Vector3 move, float yMovement, double time, int gunA, int gunB){
		transform.position = pos;
		camHolder.transform.eulerAngles = ang;
		camAngle = ang;
		crouched = crouch;
		moveVec = move;
		yMove = yMovement;
		lastUpdateTime = time;
		
		handGun = gunA;
		holsterGun = gunB;
		
		NonLocalUpdate();
	}
}
