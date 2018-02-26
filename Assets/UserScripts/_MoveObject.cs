using UnityEngine;
using System.Collections;
using SimpleJSON;

public partial class Wit3D : MonoBehaviour {

	void MoveObject(string textToParse){

		// Debug: print
		print ("MOVE OBJECT function called");

		// Parse JSON
		var N = JSON.Parse (textToParse);

		// Find the subject
		string subjJson = N["entities"]["subject"][0]["value"].Value.ToLower();
		print ("Subject: " + subjJson);

		// Find the destination
		string destJson = N["entities"]["destination"][0]["value"].Value.ToLower();
		print ("Destination: " + destJson);

		// Find the objects
		GameObject subject = GameObject.Find(subjJson);
		GameObject destination = GameObject.Find(destJson);

		// Find object's positions
		Vector3 subjectLoc = subject.transform.localPosition;
		string subjectLocDebug = subject.transform.localPosition.ToString ();
		print ("SubjectLoc: " + subjectLoc);

		Vector3 destLoc = destination.transform.localPosition + new Vector3 (0.0f, (destination.transform.lossyScale.y/2), 0.0f);
		string destLocDebug = destination.transform.localPosition.ToString ();

		// Now move the object
		StartCoroutine (MoveToPosition (subject, destLoc, moveTime));

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


}
