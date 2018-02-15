using UnityEngine;
using System.Collections;
using SimpleJSON;

public partial class Wit3D : MonoBehaviour {

	void Handle(string textToParse) {

		print (textToParse);
		var N = JSON.Parse (textToParse);
		print ("SimpleJSON: " + N.ToString());

		// string intent = N["outcomes"] [0] ["intent"].Value.ToLower ();
		string intent = N["entities"]["intent"][0]["value"].Value.ToLower ();
		Debug.LogWarning (N["entities"]["intent"][0]["value"].Value.ToLower ());

		// what function should I call?
		switch (intent)
		{
		case "move_object":
			print ("Intent is MOVE OBJECT");
			MoveObject(textToParse);
			break;
		case "create_object":
			print ("Intent is CREATE OBJECT");
			CreateObjectHandler (textToParse);
			break;
		default:
			print ("Sorry, didn't understand your intent.");
			break;
		}


	}
}
