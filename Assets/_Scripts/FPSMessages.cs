using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSMessages : MonoBehaviour {

	public List<ChatMessage> messages = new List<ChatMessage>();
	
	
	public float textFadeTime = 10f;
	public float textDisplayTime = -100;
	
	
	public bool chatTextEntry = false;
	private string chatTextMessage = "";
	
	private SophieNetworkScript theNetwork;
	
	void Start(){
		theNetwork = GetComponent<SophieNetworkScript>();
	}
	
	
	void Update(){
		if (Input.GetKeyDown("t") && !Input.GetKey("tab") && !chatTextEntry){
			chatTextEntry = true;
		}
		if (Input.GetKeyDown("escape")){
			chatTextEntry = false;
		}
	}
	
	void OnGUI(){
		
		//display messages
		if (Time.time< textDisplayTime){
		for (int i=0; i<15; i++){
			
			if (i<messages.Count){
				
				Color guiColA = GUI.color;
				
				//highlight
				GUI.color = new Color(1, 1, 1, 0.5f);
				GUI.Label(new Rect(10,Screen.height - 55 - (i*15) -1, 500, 20), messages[messages.Count-i-1].sender + " " + messages[messages.Count-i-1].message);
				
				//drop shadow
				GUI.color = new Color(0, 0, 0, 0.5f);
				//GUI.color = new Color((messages[messages.Count-i-1].senderColor.r * -1) + 0.5f, (messages[messages.Count-i-1].senderColor.g * -1) + 0.5f, (messages[messages.Count-i-1].senderColor.b * -1) + 0.5f, 1);
				GUI.Label(new Rect(11,Screen.height - 55 - (i*15) +1, 500, 20), messages[messages.Count-i-1].sender + " " + messages[messages.Count-i-1].message);
				//GUI.Label(new Rect(9,Screen.height - 55 - (i*15) -1, 500, 20), messages[messages.Count-i-1].sender + " " + messages[messages.Count-i-1].message);
				
				//normal
				GUI.color = messages[messages.Count-i-1].senderColor;
				//GUI.color = new Color(messages[messages.Count-i-1].senderColor.r + 0.25f, messages[messages.Count-i-1].senderColor.g + 0.25f, messages[messages.Count-i-1].senderColor.b + 0.25f, 1);
				GUI.Label(new Rect(10,Screen.height - 55 - (i*15), 500, 20), messages[messages.Count-i-1].sender + " " + messages[messages.Count-i-1].message);
				
				GUI.color = guiColA;
			}
		}
		}
		if (chatTextEntry){
			
			textDisplayTime = Time.time + textFadeTime;
			
			Event e = Event.current;
			if (e.keyCode == KeyCode.Escape){
				chatTextEntry = false;
				chatTextMessage = "";
				
			}else if (e.keyCode == KeyCode.Return){
				chatTextEntry = false;
					
				if (chatTextMessage != ""){
					if (theNetwork.connected){
						networkView.RPC("SendChatMessage",RPCMode.All, theNetwork.localPlayer.name + ":", chatTextMessage, theNetwork.ColToVec(theNetwork.localPlayer.colA));
					}else{
						SendChatMessage( theNetwork.localPlayer.name + ":", chatTextMessage, theNetwork.ColToVec(theNetwork.localPlayer.colA));
					}
				}
				
				
				chatTextMessage = "";
			}else{
				GUI.SetNextControlName("TextField");
				chatTextMessage = GUI.TextField(new Rect(10,Screen.height - 35, 500, 20), chatTextMessage);
				GUI.FocusControl("TextField");
			}

    
			
			
			
		}
	}
	
	[RPC]
	void SendChatMessage( string name, string msg, Vector3 col){
				
		ChatMessage newMessage = new ChatMessage();
		newMessage.sender = name;
		newMessage.senderColor = theNetwork.VecToCol(col);
		newMessage.message = msg;
		messages.Add(newMessage);
		
		textDisplayTime = Time.time + textFadeTime;
	}
}
