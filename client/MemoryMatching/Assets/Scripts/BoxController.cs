using UnityEngine;
using System.Collections;

public class BoxController : MonoBehaviour {
	public GameController.Box parentBox;

	// Update is called once per frame
	void Update () {
		
	}

	public void setParentBox(GameController.Box box) {
		parentBox = box;
	}

	void OnMouseDown() {
		parentBox.onClick ();
	}
}
