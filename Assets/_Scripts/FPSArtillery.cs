using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSArtillery : MonoBehaviour {
	
	private SophieNetworkScript theNetwork;
	
	
	public GameObject pistolBulletPrefab;
	public GameObject swapperBulletPrefab;
	public GameObject grenadeBulletPrefab;
	
	public GameObject rifleDissipationPrefab;
	
	public GameObject grenadeFlashPrefab;
	public GameObject muzzleFlashPrefab;
	
	public GameObject rocketPrefab;
	
	public GameObject soundObjectPrefab;
	public AudioClip sfx_grenadeExplode;
	public AudioClip sfx_rocketExplode;
	public AudioClip sfx_rocketfire;
	public AudioClip sfx_machinegunshoot;
	public AudioClip sfx_pistolshoot;
	public AudioClip sfx_rifleshoot;
	public AudioClip sfx_grenadethrow;
	public AudioClip sfx_swappershoot;
	
	
	
	private List<GrenadeScript> activeGrenades = new List<GrenadeScript>();
	private List<RocketScript> activeRockets = new List<RocketScript>();
	
	public GunTypeScript[] gunTypes;
	
	// Use this for initialization
	void Start () {
		theNetwork = GetComponent<SophieNetworkScript>();
	}
	
	public void Clear(){
		for (int i=0; i<activeGrenades.Count; i++){
			if (activeGrenades[i] != null && activeGrenades[i].gameObject != null) Destroy(activeGrenades[i].gameObject);
		}
		activeGrenades = new List<GrenadeScript>();
	}
	
	public void Shoot(string weaponType, Vector3 origin, Vector3 direction, Vector3 end, NetworkViewID shooterID, NetworkViewID bulletID, double time, bool hit){
		if (weaponType == "pistol" || weaponType == "machinegun" || weaponType == "rifle"){
			
			bool localFire = false;
			Vector3 localstart = origin;
			for (int i=0; i<theNetwork.players.Count; i++){
				if (theNetwork.players[i].viewID == shooterID && theNetwork.players[i].local){
					localFire = true;
					localstart = theNetwork.players[i].fpsEntity.firstPersonGun.transform.position + (Camera.main.transform.forward*0.5f);
				}
			}
			
			GameObject muzzleFlash = (GameObject)GameObject.Instantiate(muzzleFlashPrefab);
			muzzleFlash.transform.position = origin;
			if (localFire) muzzleFlash.transform.position = localstart - (Camera.main.transform.right * 0.2f);
			
			
			if (weaponType == "machinegun"){
				GameObject newBullet = (GameObject)GameObject.Instantiate(pistolBulletPrefab);
				newBullet.GetComponent<SimplePistolBullet>().start = origin;
				newBullet.GetComponent<SimplePistolBullet>().width = 0.025f;
				newBullet.GetComponent<SimplePistolBullet>().col = gunTypes[2].gunMaterial.color;
				if (localFire) newBullet.GetComponent<SimplePistolBullet>().start = localstart;
				newBullet.GetComponent<SimplePistolBullet>().end = end;
				
			} else if (weaponType == "rifle"){
				
				GameObject newBullet = (GameObject)GameObject.Instantiate(pistolBulletPrefab);
				newBullet.GetComponent<SimplePistolBullet>().start = origin;
				newBullet.GetComponent<SimplePistolBullet>().width = 0.2f;
				newBullet.GetComponent<SimplePistolBullet>().col = gunTypes[3].gunMaterial.color;
				if (localFire) newBullet.GetComponent<SimplePistolBullet>().start = localstart;
				newBullet.GetComponent<SimplePistolBullet>().end = end;
				
				GameObject newBullet4 = (GameObject)GameObject.Instantiate(pistolBulletPrefab);
				newBullet4.GetComponent<SimplePistolBullet>().start = origin;
				newBullet4.GetComponent<SimplePistolBullet>().width = 0.02f;
				newBullet4.GetComponent<SimplePistolBullet>().col = new Color(1,1,1,0.5f);
				if (localFire) newBullet4.GetComponent<SimplePistolBullet>().start = localstart;
				newBullet4.GetComponent<SimplePistolBullet>().end = end;
				
				GameObject newBullet2 = (GameObject)GameObject.Instantiate(swapperBulletPrefab);
				newBullet2.GetComponent<SwapperBullet>().start = origin;
				if (localFire && !hit) newBullet2.GetComponent<SwapperBullet>().start = localstart;
				newBullet2.GetComponent<SwapperBullet>().end = end;
				newBullet2.GetComponent<SwapperBullet>().customColor = true;
				newBullet2.GetComponent<SwapperBullet>().Spread = 0.05f;
				newBullet2.GetComponent<SwapperBullet>().col = new Color(1,0,0,0.5f);
				
				GameObject newBullet3 = (GameObject)GameObject.Instantiate(swapperBulletPrefab);
				newBullet3.GetComponent<SwapperBullet>().start = origin;
				if (localFire && !hit) newBullet3.GetComponent<SwapperBullet>().start = localstart;
				newBullet3.GetComponent<SwapperBullet>().end = end;
				newBullet3.GetComponent<SwapperBullet>().customColor = true;
				newBullet3.GetComponent<SwapperBullet>().Spread = 0.05f;
				newBullet3.GetComponent<SwapperBullet>().col = new Color(1,1,0,0.5f);
				
			} else if (weaponType == "pistol") {
				GameObject newBullet = (GameObject)GameObject.Instantiate(pistolBulletPrefab);
				newBullet.GetComponent<SimplePistolBullet>().start = origin;
				if (localFire) newBullet.GetComponent<SimplePistolBullet>().start = localstart;
				newBullet.GetComponent<SimplePistolBullet>().end = end;
				newBullet.GetComponent<SimplePistolBullet>().width = 0.1f;
				newBullet.GetComponent<SimplePistolBullet>().col = gunTypes[0].gunMaterial.color;
				
				Vector3 dissipationStart = origin;
				if (localFire) dissipationStart = localstart;
				Vector3 dissipationDirection = (end-dissipationStart).normalized;
				float dissipationLength = Vector3.Distance(end, dissipationStart);
				if (dissipationLength > 40f) dissipationLength = 40f;
				float dissipationProgress = 0f;
				while (dissipationProgress<dissipationLength){
					GameObject newDiss = (GameObject)GameObject.Instantiate(rifleDissipationPrefab);
					newDiss.GetComponent<RifleDissipationScript>().gravity = true;
					newDiss.transform.position = dissipationStart + (dissipationDirection * dissipationProgress);
					dissipationProgress += Random.Range(0.3f,0.7f);
				}
			}
			
			
		}
		if (weaponType == "grenade"){
			GameObject newGrenade = (GameObject)GameObject.Instantiate(grenadeBulletPrefab);
			newGrenade.GetComponent<GrenadeScript>().start = origin;
			newGrenade.GetComponent<GrenadeScript>().direction = direction;
			newGrenade.GetComponent<GrenadeScript>().startTime = time;
			newGrenade.GetComponent<GrenadeScript>().viewID = bulletID;
			newGrenade.GetComponent<GrenadeScript>().shooterID = shooterID;
			newGrenade.GetComponent<GrenadeScript>().detonationTime = 2f;
			activeGrenades.Add(newGrenade.GetComponent<GrenadeScript>());
		}
		if (weaponType == "rocketlauncher"){
			GameObject newRocket = (GameObject)GameObject.Instantiate(rocketPrefab);
			newRocket.transform.position = origin;
			newRocket.transform.LookAt(origin + direction);
			newRocket.GetComponent<RocketScript>().viewID = bulletID;
			newRocket.GetComponent<RocketScript>().shooterID = shooterID;
			activeRockets.Add(newRocket.GetComponent<RocketScript>());
		}
		if (weaponType == "swapper"){
			bool localFire = false;
			Vector3 localstart = origin;
			for (int i=0; i<theNetwork.players.Count; i++){
				if (theNetwork.players[i].viewID == shooterID && theNetwork.players[i].local){
					localFire = true;
					localstart = theNetwork.players[i].fpsEntity.firstPersonGun.transform.position + (Camera.main.transform.forward*0.5f);
				}
			}
			
			for (int i=0; i<4; i++)
			{
				GameObject newBullet = (GameObject)GameObject.Instantiate(swapperBulletPrefab);
				newBullet.GetComponent<SwapperBullet>().start = origin;
				if (localFire && !hit) newBullet.GetComponent<SwapperBullet>().start = localstart;
				newBullet.GetComponent<SwapperBullet>().end = end;
				newBullet.GetComponent<SwapperBullet>().hit = hit;
				newBullet.GetComponent<SwapperBullet>().Spread = 0.4f;
			}
		}
		
		for (int i=0; i<theNetwork.players.Count; i++){
			if (theNetwork.players[i].viewID == shooterID){
				if (weaponType=="pistol"){
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.clip = sfx_pistolshoot;
					if (theNetwork.players[i].viewID == theNetwork.localPlayer.viewID) theNetwork.players[i].fpsEntity.weaponSoundObj.audio.volume = 0.3f;
					
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.pitch = Random.Range(0.99f,1.01f);
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.Play();
				}
				if (weaponType=="machinegun"){
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.clip = sfx_machinegunshoot;
					if (theNetwork.players[i].viewID == theNetwork.localPlayer.viewID) theNetwork.players[i].fpsEntity.weaponSoundObj.audio.volume = 0.3f;
					
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.pitch = Random.Range(0.95f,1.05f);
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.Play();
				}
				if (weaponType=="rifle"){
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.clip = sfx_rifleshoot;
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.pitch = 1f;
					if (theNetwork.players[i].viewID == theNetwork.localPlayer.viewID) theNetwork.players[i].fpsEntity.weaponSoundObj.audio.volume = 0.3f;
					
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.pitch = 1f;
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.Play();
				}
				if (weaponType=="grenade"){
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.clip = sfx_grenadethrow;
					if (theNetwork.players[i].viewID == theNetwork.localPlayer.viewID) theNetwork.players[i].fpsEntity.weaponSoundObj.audio.volume = 0.3f;
					
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.pitch = Random.Range(0.9f,1.1f);
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.Play();
				}
				if (weaponType=="rocketlauncher"){
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.clip = sfx_rocketfire;
					if (theNetwork.players[i].viewID == theNetwork.localPlayer.viewID) theNetwork.players[i].fpsEntity.weaponSoundObj.audio.volume = 0.6f;
					
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.pitch = 1f;
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.Play();
				}
				if (weaponType=="swapper"){
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.clip = sfx_swappershoot;
					if (theNetwork.players[i].viewID == theNetwork.localPlayer.viewID) theNetwork.players[i].fpsEntity.weaponSoundObj.audio.volume = 0.3f;
					
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.pitch = Random.Range(0.9f,1.1f);
					theNetwork.players[i].fpsEntity.weaponSoundObj.audio.Play();
				}
			}
		}
		
		
	}
	
	public static float GetWeaponDamage(string weaponType){
		if (weaponType == "pistol") return 10f;
		if (weaponType == "grenade") return 70f;
		if (weaponType == "machinegun") return 5f;
		if (weaponType == "rifle") return 60f;
		if (weaponType == "suicide") return 9999f;
		if (weaponType == "rocket") return 70f;
		if (weaponType == "lava") return 9999f;
		if (weaponType == "legs") return 10f;
		if (weaponType == "bones") return 20f;
		return 0;
	}
	
	public void Detonate(NetworkViewID viewID){
		for (int i=0; i<activeGrenades.Count; i++){
			if (viewID == activeGrenades[i].viewID){
				
				//grenade jumping
				for (int k=0; k<theNetwork.players.Count; k++){
					if (theNetwork.players[k].local){
						float Dist = Vector3.Distance(theNetwork.players[k].fpsEntity.transform.position, activeGrenades[i].transform.position);
						float push = 5;
						if (Dist < GetDetonationRadius("grenade")){
							
							if (Dist > GetDetonationRadius("grenade")/2)
							{
								push = 3;
							}
							
							if (theNetwork.players[k].fpsEntity.transform.position.y > activeGrenades[i].transform.position.y){
								theNetwork.players[k].fpsEntity.yMove += push;
							} else if (theNetwork.players[k].fpsEntity.transform.position.y < activeGrenades[i].transform.position.y){
								theNetwork.players[k].fpsEntity.yMove -= push;
							}
							
							theNetwork.players[k].fpsEntity.grounded = false;
							theNetwork.players[k].fpsEntity.sendRPCUpdate = true;
							
							}
						}
					}
				
				
				GameObject grenadeFlash = (GameObject)GameObject.Instantiate(grenadeFlashPrefab);
				grenadeFlash.transform.position = activeGrenades[i].transform.position;
				grenadeFlash.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
				grenadeFlash.GetComponent<GrenadeFlashScript>().Rad = FPSArtillery.GetDetonationRadius("grenade");
				
				
				GameObject grenadeSoundObj = (GameObject)GameObject.Instantiate(soundObjectPrefab);
				grenadeSoundObj.transform.position = activeGrenades[i].transform.position;
				grenadeSoundObj.audio.clip = sfx_grenadeExplode;
				grenadeSoundObj.audio.volume = 5f;
				
				
				Destroy(activeGrenades[i].gameObject);
				activeGrenades.RemoveAt(i);
				
			}
		}
		
		
		for (int i=0; i<activeRockets.Count; i++){
			if (viewID == activeRockets[i].viewID){
				
				int shooteridx = -1;
				for (int j=0; j<theNetwork.players.Count; j++){
					if (theNetwork.players[j].viewID == activeRockets[i].shooterID)
						shooteridx = j;
				}
				
				//rocket jumping
				for (int k=0; k<theNetwork.players.Count; k++){
					
						float Dist = Vector3.Distance(theNetwork.players[k].fpsEntity.transform.position, activeRockets[i].transform.position);
						float push = 5;
						if (Dist < GetDetonationRadius("rocket")){
							
						if (theNetwork.gameSettings.scoreAirrockets) {
							if (!theNetwork.players[k].fpsEntity.grounded && theNetwork.players[k].health > 0)
							{
								if (shooteridx != -1 && shooteridx != k)
								{
									theNetwork.players[shooteridx].fpsEntity.Announce ("airrocket");
									
									if (theNetwork.players[shooteridx].local)
									{
										theNetwork.localPlayer.currentAward = "airrocket";
										theNetwork.localPlayer.currentAwardTime = 3f;
									}
								}
							}
						}
						
						
						if (theNetwork.players[k].local){
							
							if (Dist > GetDetonationRadius("rocket")/2)
							{
								push = 3;
							}
						
							if (theNetwork.players[k].fpsEntity.transform.position.y > activeRockets[i].transform.position.y){
								theNetwork.players[k].fpsEntity.yMove += push;
							} else if (theNetwork.players[k].fpsEntity.transform.position.y < activeRockets[i].transform.position.y){
								theNetwork.players[k].fpsEntity.yMove -= push;
							}
							
							theNetwork.players[k].fpsEntity.grounded = false;
							theNetwork.players[k].fpsEntity.sendRPCUpdate = true;
							
							}
						}
					}
			
				//detonate rocket
				
				GameObject grenadeFlash = (GameObject)GameObject.Instantiate(grenadeFlashPrefab);
				grenadeFlash.transform.position = activeRockets[i].transform.position;
				grenadeFlash.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
				grenadeFlash.GetComponent<GrenadeFlashScript>().Rad = FPSArtillery.GetDetonationRadius("rocket");
				
				GameObject rocketSoundObj = (GameObject)GameObject.Instantiate(soundObjectPrefab);
				rocketSoundObj.transform.position = activeRockets[i].transform.position;
				rocketSoundObj.audio.clip = sfx_rocketExplode;
				rocketSoundObj.audio.volume = 5f;
				
				Destroy(activeRockets[i].gameObject);
				activeRockets.RemoveAt(i);
			}
		}
	}
	
	public static float GetDetonationRadius(string weaponType){
		if (weaponType == "grenade") return 5;
		if (weaponType == "rocket") return 4;
		return 0;
	}
	
}
