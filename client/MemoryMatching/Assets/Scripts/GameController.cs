using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	public const string URL_BASE = "http://prod-spbe7sxihm.elasticbeanstalk.com/index.php";
	public const string PARSE_BASE = "http://inyour-w9my2y2jii.elasticbeanstalk.com/index.php";
	public const string UID = "uid";
	public const string GAME_ID = "game_id";
	public const string SESSION_ID = "session_id";
	public const string EVENT_TIME = "event_time";
	public const string EVENT_DATA = "event_data";

	public string[] patternTypes;
	public Box[] boxGrid = new Box[16];
	public Box lastBoxClicked;
	public Box secondLastBoxClicked;
	public int skipFrames;
	private int numSolved;
	private EventLogger eventLogger;

	public class Box {
		GameController gameController;
		Vector3 position;
		public string patternName;
		private GameObject cover;
		private GameObject pattern;
		private bool isCovered;

		public Box(GameController controller, Vector3 position, string patternName) {
			this.gameController = controller;
			this.patternName = patternName;
			this.position = position;
			print ("Box instantiated with pattern = " + patternName);
			pattern = Instantiate(Resources.Load ("Prefabs/" + patternName), position, Quaternion.identity) 
				as GameObject;
			Vector3 coverPosition = new Vector3(position.x, position.y, -1);
			print ("Position = " + coverPosition.x + ", " + coverPosition.y + ", " + coverPosition.z);
			cover = Instantiate (Resources.Load ("Prefabs/Cover"), coverPosition, Quaternion.identity) as GameObject;
			isCovered = true;
			BoxController boxController = cover.GetComponent<BoxController>();
			boxController.setParentBox(this);
		}

		public void onClick() {
			print ("clicked on " + position.x + ", " + position.y);
			if (gameController.skipFrames > 0 
			    || !cover.activeInHierarchy 
			    || (gameController.lastBoxClicked != null && gameController.secondLastBoxClicked != null)) {
				return;
			}
			isCovered = false;
			if (gameController.lastBoxClicked != null) {
				gameController.secondLastBoxClicked = gameController.lastBoxClicked;
				gameController.skipFrames = 60;
			}
			gameController.eventLogger.addEvent (gameController.calculatePosFromVector (position));
			gameController.lastBoxClicked = this;
		}

		public void Update() {
			cover.SetActive(isCovered);
		}

		public void setCovered() {
			isCovered = true;
		}

		public void Destroy() {
			UnityEngine.Object.Destroy (cover);
			UnityEngine.Object.Destroy (pattern);
			gameController = null;
		}
	}

	public void Reset() {
		lastBoxClicked = null;
		secondLastBoxClicked = null;
		GameObject[] clones = GameObject.FindGameObjectsWithTag ("Clones");
		foreach (GameObject clone in clones) {
			clone.SetActive(false);
			UnityEngine.GameObject.DestroyImmediate(clone);
		}
		Application.LoadLevel (Application.loadedLevel);
	}

	// Use this for initialization
	void Start () {
		Screen.fullScreen = false;
//		GameObject winText = GameObject.FindWithTag ("WinTag");
//		winText.transform.localScale = new Vector3(0f, 0f, 0f);
		numSolved = 0;
		patternTypes = new string[] {
			"arrow-block-rotated", // 0
			"box-full", // 1
			"diamond-outline", // 2
			"frame-small", // 3
			"gear-hole", // 4
			"next-full", // 5
			"page-overlaid", // 6
			"star-outline", // 7
		};
		Vector3[] positions = new Vector3[16];
		int[] patterns = new int[16];
		for (int i=0; i < 16; i++) {
			positions[i] = new Vector3((i%4)*2 - 3, (i/4)*2 - 3, 0);
		}
		positions = randomizePos (positions);
		for (int i=0; i < 16; i++) {
			boxGrid[i] = new Box(this, positions[i], patternTypes[i/2]);
			patterns[calculatePosFromVector(positions[i])] = i/2;
		}

		eventLogger = new EventLogger ();
		eventLogger.setup (this);
		eventLogger.registerPattern (patterns);
	}

	public int calculatePosFromVector(Vector3 vec) {
		return (int)(vec.x + 3) / 2 + (int)(vec.y + 3) * 2;
	}

	Vector3[] randomizePos(Vector3[] positions) {
		for (int i = 0; i < 16; i++) {
			int rRight = (int) (Random.value * (16-i)) + i;
			Vector3 temp = positions[rRight];
			positions[rRight] = positions[i];
			positions[i] = temp;
		}
		return positions;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}
		if (numSolved >= 8) {
//			GameObject winText = GameObject.FindWithTag ("WinTag");
//			winText.transform.localScale = new Vector3(0.14f, 0.14f, 1f);
			eventLogger.publish();
			Instantiate(Resources.Load("Prefabs/WinScreen"));
			numSolved = 0;
		}
		// Update every box
		for (int i=0; i<16; i++) {
			boxGrid[i].Update();
		}

		// Check for 2 clicks
		if (skipFrames > 0) {
//			print("skipFrames = " + skipFrames);
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

	public void sendRequest(WWW www) {
		StartCoroutine (WaitForRequest (www));
	}

	public IEnumerator WaitForRequest(WWW www) {
		yield return www;
		
		if (www.error == null) {
			print ("WWW OK! " + www.text);
		} else {
			print ("WWW ERROR! " + www.text);
		}
	}
	
	public class EventLogger {
		public const string URL_BASE = "http://prod-spbe7sxihm.elasticbeanstalk.com/index.php";
		public const string UID = "uid";
		public const string GAME_ID = "game_id";
		public const string SESSION_ID = "session_id";
		public const string EVENT_TIME = "event_time";
		public const string EVENT_DATA = "event_data";
		public const string PATTERN = "pattern";
		public const string EVENTS = "events";
		public const string POSITION = "pos";
		public const string TIME = "time";
		private GameController gameController;
		System.DateTime epochStart;
		double cur_time;
		JSONObject eventDataJSON;
		JSONObject eventsArrayJSON;
		WWWForm form;

		public void setup(GameController gameController) {
			this.gameController = gameController;
			epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			cur_time = (System.DateTime.UtcNow - epochStart).TotalSeconds;
			eventDataJSON = new JSONObject (JSONObject.Type.OBJECT);
			eventsArrayJSON = new JSONObject (JSONObject.Type.ARRAY);
			eventDataJSON.AddField (EVENTS, eventsArrayJSON);
		}

		public void registerPattern(int[] patterns) {
			JSONObject arr = new JSONObject (JSONObject.Type.ARRAY);
			for (int i=0; i<patterns.Length; i++) {
				arr.Add (patterns [i]);
			}
			eventDataJSON.AddField (PATTERN, arr);
			print ("Patterns = " + eventDataJSON.Print ());
		}

		public void addEvent(int pos) {
			JSONObject eventJson = new JSONObject (JSONObject.Type.OBJECT);
			eventJson.AddField (POSITION, pos);
			eventJson.AddField(TIME, (System.DateTime.UtcNow - epochStart).TotalSeconds.ToString());
			eventsArrayJSON.Add (eventJson);
			print ("Event = " + eventJson.Print());
		}

		public void publish() {
			WWWForm form = new WWWForm ();
			int uid = (int)(Random.value * 10) + 3;
			form.AddField (UID, uid);
			form.AddField (GAME_ID, "1");
			form.AddField (SESSION_ID, (int) (Random.value*10000f));
			form.AddField (EVENT_TIME, cur_time.ToString());
			form.AddField (EVENT_DATA, eventDataJSON.Print());
			print ("Final JSON = " + eventDataJSON.Print ());
			WWW www = new WWW (PARSE_BASE, form);
			gameController.sendRequest (www);
		}
	}
}
