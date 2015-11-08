using UnityEngine;
using System.Collections;

public class GridController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Instantiate (Resources.Load ("Prefabs/VerticalDivider"), new Vector3 (2, 0, -2), Quaternion.identity);
		Instantiate (Resources.Load ("Prefabs/VerticalDivider"), new Vector3 (0, 0, -2), Quaternion.identity);
		Instantiate (Resources.Load ("Prefabs/VerticalDivider"), new Vector3 (-2, 0, -2), Quaternion.identity);
		
		Instantiate (Resources.Load ("Prefabs/HorizontalDivider"), new Vector3 (0, 2, -2), Quaternion.identity);
		Instantiate (Resources.Load ("Prefabs/HorizontalDivider"), new Vector3 (0, 0, -2), Quaternion.identity);
		Instantiate (Resources.Load ("Prefabs/HorizontalDivider"), new Vector3 (0, -2, -2), Quaternion.identity);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
