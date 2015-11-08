using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	public string[] patternTypes;
	public Box[] boxGrid = new Box[16];
	public Box lastBoxClicked;

	public class Box {
		GameController gameController;
		Vector3 position;
		private GameObject pattern;
		private GameObject cover;
		private bool wasOpened;
		private bool isCovered;

		public Box() {
		}

		public Box(GameController controller, Vector3 position, string patternName) {
			this.gameController = controller;
			pattern = Instantiate(Resources.Load ("Prefabs/" + patternName), position, Quaternion.identity) as GameObject;
			Vector3 coverPosition = new Vector3(position.x, position.y, -1);
			cover = Instantiate (Resources.Load ("Prefabs/Cover"), coverPosition, Quaternion.identity) as GameObject;
			wasOpened = false;
			isCovered = true;
			RevealController revealController = cover.GetComponent<RevealController>();
			revealController.parentBox = this;
		}

		public void onClick() {
			if (gameController.getLastBoxClicked() != null) { // This is the second open
				cover.SetActive(true);
				gameController.getLastBoxClicked().cover.SetActive(true);
				gameController.setLastBoxClicked(null);
			}
			wasOpened = true;
			isCovered = !isCovered;
			cover.SetActive (isCovered);
			gameController.setLastBoxClicked(this);
		}
	}

	// Use this for initialization
	void Start () {
		patternTypes = new string[] {
			"arrow-block-rotated", 
			"box-full",
			"diamond-outline",
			"frame-small",
			"gear-hole",
			"next-full",
			"page-overlaid",
			"star-outline",
		};
		Vector3[] positions = new Vector3[16];
		for (int i=0; i < 16; i++) {
			positions[i] = new Vector3((i%4)*2 - 3, (i/4)*2 - 3, 0);
		}
		positions = randomizePos (positions);
		for (int i=0; i < 16; i++) {
			boxGrid[i] = new Box(this, positions[i], patternTypes[i/2]);
		}
	}

	Vector3[] randomizePos(Vector3[] positions) {
		for (int i = 0; i < 16; i++) {
			int rRight = (int) (Random.value * (15-i)) + i;
			print ("rRight = " + rRight);
			Vector3 temp = positions[rRight];
			positions[rRight] = positions[i];
			positions[i] = temp;
		}
		return positions;
	}

	Box getLastBoxClicked() {
		return lastBoxClicked;
	}

	void setLastBoxClicked(Box box) {
		lastBoxClicked = box;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
