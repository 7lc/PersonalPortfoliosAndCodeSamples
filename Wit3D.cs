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

public partial class Wit3D : MonoBehaviour {

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

	// Uncomment the line below to bypass SSL
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };

	// set samplerate to 16000 for wit.ai
	samplerate = 16000;

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
    
    public void RecordingForWit()
    {

	// Debug
	print("Thinking ...");

	// Save the audio file
	Microphone.End(null);
	SavWav.Save("record", commandClip);

	// At this point, we can delete the existing audio clip
	commandClip = null;

	//Grab the most up-to-date JSON file
	// url = "https://api.wit.ai/message?v=20160305&q=Put%20the%20box%20on%20the%20shelf";
	token = "2A7QSFYK2MFZ3N6RWMAAXR3HJV2ROFLW";

	//Start a coroutine called "WaitForRequest" with that WWW variable passed in as an argument
	string witAiResponse = GetJSONText(Application.persistentDataPath+"/record.wav");
	print(witAiResponse);
	Handle(witAiResponse);
    }
}
