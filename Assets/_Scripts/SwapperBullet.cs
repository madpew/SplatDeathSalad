using UnityEngine;
using System.Collections;

public class SwapperBullet : MonoBehaviour {
	
	
	private LineRenderer lr;
	
	private Color col = new Color(1,1,1,0.5f);
	public Vector3 start;
	public Vector3 end;
	
	public bool hit = false;
	
	private Vector3[] points;
	private Vector3[] moves;
	
	// Use this for initialization
	void Start () {
		lr = GetComponent<LineRenderer>();
		
		
		
		if (hit) col = new Color(0,1,1,0.35f);
		if (!hit) col = new Color(1,0.5f,0,0.35f);
		
		lr.SetColors(col,col);
		
		float distance = Vector3.Distance(start,end);
		Vector3 direction = (end-start).normalized;
		
		int stepCount = Mathf.FloorToInt(distance / 0.5f);
		
		points = new Vector3[stepCount];
		moves = new Vector3[stepCount];
		for (int i=0; i<stepCount; i++){
			points[i] = start + (direction * i * 0.5f);
			points[i] += new Vector3(Random.Range(-0.2f,0.2f),Random.Range(-0.2f,0.2f),Random.Range(-0.2f,0.2f));
			
			moves[i] = new Vector3(Random.Range(-0.2f,0.2f),Random.Range(-0.2f,0.2f),Random.Range(-0.2f,0.2f));
		}
		
		lr.SetVertexCount(points.Length+1);
		for (int i=0; i<stepCount; i++){
			lr.SetPosition(i,points[i]);
		}
		lr.SetPosition(points.Length,end);
		//lr.SetPosition(0,start);
		//lr.SetPosition(1,end);
	}
	
	// Update is called once per frame
	void Update () {
		
		for (int i=0; i<points.Length; i++){
			
			points[i] += moves[i] * Time.deltaTime * 1f;
			
			lr.SetPosition(i,points[i]);
		}
		
		lr.SetColors(col,col);
		
		if (!hit){
			col.a -= Time.deltaTime * 0.3f;
		}else{
			col.a -= Time.deltaTime * 0.1f;
		}
		if (col.a<=0f){
			Destroy(gameObject);
		}
	}
}
