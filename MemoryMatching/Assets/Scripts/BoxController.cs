using UnityEngine;
using System.Collections;

public class RevealController : MonoBehaviour {
	public GameController.Box parentBox;

	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseDown() {
		parentBox.onClick ();
	}
}
