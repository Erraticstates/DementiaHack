using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	public string[] patternTypes;
	public Box[] boxGrid = new Box[16];
	public Box lastBoxClicked;
	public Box secondLastBoxClicked;
	public int skipFrames;
	private int numSolved;

	public class Box {
		GameController gameController;
		Vector3 position;
		public string patternName;
		private GameObject cover;
		private bool wasOpened;
		private bool isCovered;

		public Box() {
		}

		public Box(GameController controller, Vector3 position, string patternName) {
			this.gameController = controller;
			this.patternName = patternName;
			print ("Box instantiated with pattern = " + patternName);
			Instantiate(Resources.Load ("Prefabs/" + patternName), position, Quaternion.identity);
			Vector3 coverPosition = new Vector3(position.x, position.y, -1);
			print ("Position = " + coverPosition.x + ", " + coverPosition.y + ", " + coverPosition.z);
			cover = Instantiate (Resources.Load ("Prefabs/Cover"), coverPosition, Quaternion.identity) as GameObject;
			wasOpened = false;
			isCovered = true;
			BoxController boxController = cover.GetComponent<BoxController>();
			boxController.setParentBox(this);
		}

		public void onClick() {
			print ("onClick");
			if (gameController.skipFrames > 0 
			    || !cover.activeInHierarchy 
			    || (gameController.lastBoxClicked != null && gameController.secondLastBoxClicked != null)) {
				return;
			}
			wasOpened = true;
			isCovered = false;
			if (gameController.lastBoxClicked != null) {
				gameController.secondLastBoxClicked = gameController.lastBoxClicked;
				gameController.skipFrames = 60;
			}
			gameController.lastBoxClicked = this;
		}

		public void Update() {
			cover.SetActive(isCovered);
		}

		public void setCovered() {
			isCovered = true;
		}
	}

	// Use this for initialization
	void Start () {
		GameObject winText = GameObject.FindWithTag ("WinTag");
		winText.transform.localScale = new Vector3(0f, 0f, 0f);
		numSolved = 0;
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
			Vector3 temp = positions[rRight];
			positions[rRight] = positions[i];
			positions[i] = temp;
		}
		return positions;
	}

	// Update is called once per frame
	void Update () {
		if (numSolved >= 8) {
			GameObject winText = GameObject.FindWithTag ("WinTag");
			winText.transform.localScale = new Vector3(0.14f, 0.14f, 1f);
			numSolved = 0;
		}
		// Update every box
		for (int i=0; i<16; i++) {
			boxGrid[i].Update ();
		}

		// Check for 2 clicks
		if (skipFrames > 0) {
			print("skipFrames = " + skipFrames);
			skipFrames--;
		} else if (lastBoxClicked != null && secondLastBoxClicked != null) {
			if (!lastBoxClicked.patternName.Equals(secondLastBoxClicked.patternName)) {
				print ("Patterns don't match, resetting!");
				lastBoxClicked.setCovered();
				secondLastBoxClicked.setCovered();
			} else {
				print ("New pattern solved!");
				numSolved++;
				print ("numSolved = " + numSolved);
			}
			lastBoxClicked = null;
			secondLastBoxClicked = null;
		}
	}
}
