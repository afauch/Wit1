using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
// using System.Web;

public class Reference : MonoBehaviour {

	// Class Variables

	// Audio variables
	public AudioClip commandClip;
	int samplerate;

	// API access parameters
	string url;
	string token;
	UnityWebRequest wr;

	// Movement variables
	public float moveTime;
	public float yOffset;

	// GameObject to use as a default spawn point
	public GameObject spawnPoint;

	// Use this for initialization
	void Start () {

		// If you are a Windows user and receiving a Tlserror
		// See: https://github.com/afauch/wit3d/issues/2
		// Uncomment the line below to bypass SSL
		// System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };

		// set samplerate to 16000 for wit.ai
		samplerate = 16000;

	}

	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.Space)) {
			print ("Listening for command");
			commandClip = Microphone.Start(null, false, 10, samplerate);  //Start recording (rewriting older recordings)
		}


		if (Input.GetKeyUp (KeyCode.Space)) {

			// Debug
			print("Thinking ...");

			// Save the audio file
			Microphone.End(null);
			SavWav.Save("sample", commandClip);

			// At this point, we can delete the existing audio clip
			commandClip = null;

			//Grab the most up-to-date JSON file
			// url = "https://api.wit.ai/message?v=20160305&q=Put%20the%20box%20on%20the%20shelf";
			token = "NJP2HHQXIUK3IGW53WXL65NRD74GGJ5B"; //"NJP2HHQXIUK3IGW53WXL65NRD74GGJ5B";
			//"2K772I2M42FBYZPFQG53RQJHLFJI3RC6"

			//Start a coroutine called "WaitForRequest" with that WWW variable passed in as an argument
			string witAiResponse = GetJSONText("Assets/sample.wav");
			print (witAiResponse);
			Handle (witAiResponse);
		}


	}

	string GetJSONText(string file) {

		// get the file w/ FileStream
		FileStream filestream = new FileStream (file, FileMode.Open, FileAccess.Read);
		BinaryReader filereader = new BinaryReader (filestream);
		byte[] BA_AudioFile = filereader.ReadBytes ((Int32)filestream.Length);
		filestream.Close ();
		filereader.Close ();

		// create an HttpWebRequest
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.wit.ai/speech");

		request.Method = "POST";
		request.Headers ["Authorization"] = "Bearer " + token;
		request.ContentType = "audio/wav";
		request.ContentLength = BA_AudioFile.Length;
		request.GetRequestStream ().Write (BA_AudioFile, 0, BA_AudioFile.Length);

		// Process the wit.ai response
		try
		{
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			if (response.StatusCode == HttpStatusCode.OK)
			{
				print("Http went through ok");
				StreamReader response_stream = new StreamReader(response.GetResponseStream());
				return response_stream.ReadToEnd();
			}
			else
			{
				return "Error: " + response.StatusCode.ToString();
				return "HTTP ERROR";
			}
		}
		catch (Exception ex)
		{
			return "Error: " + ex.Message;
			return "HTTP ERROR";
		}       
	}

	void Handle(string textToParse) {

		print (textToParse);
		var N = JSON.Parse (textToParse);
		print ("SimpleJSON: " + N.ToString());

		string intent = N["outcomes"] [0] ["intent"].Value.ToLower ();

		// what function should I call?
		switch (intent)
		{
		case "move_object":
			print ("Intent is MOVE OBJECT");
			MoveObject(textToParse);
			break;
		case "create_object":
			print ("Intent is CREATE OBJECT");
			CreateObject (textToParse);
			break;
		default:
			print ("Sorry, didn't understand your intent.");
			break;
		}


	}

	void MoveObject(string textToParse){

		// Debug: print
		print ("MOVE OBJECT function called");

		// Parse JSON
		var N = JSON.Parse (textToParse);

		// Find the subject
		string subjJson = N["outcomes"][0]["entities"]["subject"][0]["value"].Value.ToLower();
		print ("Subject: " + subjJson);

		// Find the destination
		string destJson = N["outcomes"][0]["entities"]["destination"][0]["value"].Value.ToLower();
		print ("Destination: " + destJson);

		// Find the objects
		GameObject[] gameObjects = GenericFindObjects(new string[] {subjJson, destJson});
		GameObject subject = gameObjects [0];
		GameObject destination = gameObjects [1];

		// Find object's positions
		Vector3 subjectLoc = subject.transform.localPosition;
		string subjectLocDebug = subject.transform.localPosition.ToString ();
		print ("SubjectLoc: " + subjectLoc);

		Vector3 destLoc = destination.transform.localPosition + new Vector3 (0.0f, (destination.transform.lossyScale.y/2), 0.0f);
		string destLocDebug = destination.transform.localPosition.ToString ();

		// Now move the object
		StartCoroutine (MoveToPosition (subject, destLoc, moveTime));

	}

	void CreateObject(string textToParse){

		// Debug: print
		print ("CREATE OBJECT function called");

		// Parse JSON
		var N = JSON.Parse (textToParse);

		// Find the subject
		string subjJson = N["outcomes"][0]["entities"]["subject"][0]["value"].Value.ToLower();
		print ("Subject: " + subjJson);

		// Find the destination, if applicable
		string destJson = N["outcomes"][0]["entities"]["destination"][0]["value"].Value.ToLower();
		print ("Destination: " + destJson);

		// Find the objects
		GameObject[] gameObjects = GenericFindObjects(new string[] {subjJson, destJson});
		GameObject subject = gameObjects [0];
		GameObject destination = gameObjects [1];

		// if there's no destination:
		if (destJson == "") {
			InstantiateObject (subject, spawnPoint);
			// otherwise, instantiate at the specified destination
		} else {
			InstantiateObject (subject, destination);
		}


	}


	// A more generic function that takes an array of strings and returns an array of GameObjects

	GameObject[] GenericFindObjects(string[] objectsToFind) {

		print ("GENERIC FIND OBJECTS function called");
		print ("OBJECTS TO FIND: " + objectsToFind[0]);
		print ("OBJECTS TO FIND: " + objectsToFind[1]);

		// Create a GameObject array, the same size as our objectsToFind array
		GameObject[] gameObjectsArray = new GameObject[objectsToFind.Length];

		for (int i = 0; i < objectsToFind.Length; i++) {

			//debug 
			print("Loop #" + i);

			// if it can be found, add it to the array of GameObjects
			if (GameObject.Find (objectsToFind[i]) != null) {
				print("Trying to find " + objectsToFind [i]);
				gameObjectsArray[i] = GameObject.Find (objectsToFind [i]);
			}

			// otherwise, let us know
			else {
				print ("Couldn't find " + objectsToFind [i]);
			}

		}

		return gameObjectsArray;

	}

	// Coroutine: Move an object from one position to another
	IEnumerator MoveToPosition(GameObject subject, Vector3 newPosition, float time)
	{
		float elapsedTime = 0;
		Vector3 startingPos = subject.transform.position;
		while (elapsedTime < time)
		{
			print ("moving!");
			subject.transform.position = Vector3.Lerp(startingPos, newPosition, (elapsedTime / time));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}

	// Coroutine: Instantiate an object at point
	IEnumerator InstantiateObject(GameObject subject, GameObject destination)
	{
		Vector3 instantiationPoint = destination.transform.position;
		Instantiate (subject, instantiationPoint, default(Quaternion));
		yield return null;
	}

}