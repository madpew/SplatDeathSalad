using UnityEngine;
using System.Collections;

public class GrenadeFlashScript : MonoBehaviour {

	// Update is called once per frame
	public float Rad = 1;
	
	void Update () {
		Vector3 scale = transform.localScale;
		
		scale.x += Time.deltaTime * 20f;
		scale.y += Time.deltaTime * 20f;
		scale.z += Time.deltaTime * 20f;
		transform.localScale = scale;
		
		if (transform.localScale.x > Rad) Destroy(gameObject);
	}
}
