using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;

public class Terraformer : MonoBehaviour
{

	public event System.Action onTerrainModified;

	public LayerMask terrainMask;

	public float terraformRadius = 5;
	public float terraformSpeedNear = 0.1f;
	public float terraformSpeedFar = 0.25f;

	private InputDevice targetDevice;

	Transform cam;
	GenTest genTest;
	bool hasHit;
	Vector3 hitPoint;

	GameObject sphere;
	List<GameObject> sphereList = new List<GameObject>();
	Vector3 campos;
	bool teleportAction = false;

	bool isTerraforming;
	Vector3 lastTerraformPointLocal;

	int clientID;
	string IP;
	NetworkStream ns;
	byte[] msg = new byte[1024];
	List<(Vector3, int)> actionList = new List<(Vector3,int)>();
	Thread serveThread;


	void Start()
	{
		genTest = FindObjectOfType<GenTest>();
		cam = Camera.main.transform;
		campos = cam.position;

		// VR
		List<InputDevice> devices = new List<InputDevice>();
		//InputDeviceCharacteristics rightConChar = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
		//InputDevices.GetDevicesWithCharacteristics(rightConChar, devices);
		InputDevices.GetDevices(devices);
		Debug.Log(devices.Count);
		Debug.Log(XRSettings.isDeviceActive);
		Debug.Log("DeviceName " + XRSettings.loadedDeviceName);
		foreach (var item in devices)
		{
			Debug.Log(item.name + item.characteristics);
		}
		if (devices.Count > 0)
		{
			targetDevice = devices[0];
		}
		else
		{
			//targetDevice = XRSettings.LoadDeviceByName;
		}
		Debug.Log(targetDevice.name);
		var inputFeatures = new List<UnityEngine.XR.InputFeatureUsage>();
		Debug.Log("inputFeatures.Count " + inputFeatures.Count); // 0 -> Keine Knöpfe

		// Blue Sphere for the HitPoint
		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = new Vector3(0, 0, 0);
		Renderer r = sphere.GetComponent<Renderer>();
		r.material.color = Color.blue;

		//---| Network Connection |---//
		IP = "localhost";
		TcpClient client = new TcpClient();

		try 
		{
			client.Connect(IP, 4040);
		} 
		catch (Exception) 
		{
			Debug.Log("Kein Server gefunden");
		}

		ns = client.GetStream();
		byte[] inMsg = new byte[1024];
		ns.Read(inMsg, 0, inMsg.Length);
		clientID = Int32.Parse(Encoding.Default.GetString(inMsg));
		Debug.Log("Vergebene ID: " + clientID);

		if (client.Connected)
        {
			serveThread = new Thread(() => {				
				string inString;
				string[] inSep;
				Vector3 terraformPoint;
				int actionID;

				while (client.Connected)
                {
					try
					{
						byte[] msg = new byte[1024];
						ns.Read(msg, 0, msg.Length);
						inString = Encoding.Default.GetString(msg).Trim(' ');
						inSep = inString.Split(',');    // Erzeugt ein Array mit dem Inhalt: ["{senderID}","{ActionID}","{PosX}","{PosY}","{PosZ}",???] Der Letzte Eintrag im array ist verwirrend und mir unbekannt

						if (Int32.Parse(inSep[0]) == clientID) continue;

						actionID = Int32.Parse(inSep[1]);
						terraformPoint = new Vector3(float.Parse(inSep[2])/10, float.Parse(inSep[3])/10, float.Parse(inSep[4])/10);	//Mit den Koordinaten ein wenig herumspielen, ich weiß nicht wieso die Koordinaten beim Konvertieren zu Float verzehnfacht werden. (auch wenn ich es mir denken kann...)
						Debug.Log("ID: " + actionID + ", Vector3: " + terraformPoint.ToString());

						if (actionID == 3) // 3: Teleport Action
                        {
							actionList.Add((terraformPoint, Int32.Parse(inSep[0]) + 3));
						}
                        else
                        {
							actionList.Add((terraformPoint, actionID));
						}

					}
					catch(ThreadInterruptedException) { Debug.Log("Spiel beendet"); }
					catch (Exception) { Debug.Log("Exception"); }

				}
				Debug.Log("THREAD_ENDE: Disconnected");
			});
			serveThread.Start();
		}

	}

	void Update()
	{
		RaycastHit hit;
		hasHit = false;

		bool wasTerraformingLastFrame = isTerraforming;
		isTerraforming = false;

		int numIterations = 5;
		bool rayHitTerrain = false;



		for (int i = 0; i < numIterations; i++)
		{
			float rayRadius = terraformRadius * Mathf.Lerp(0.01f, 1, i / (numIterations - 1f));
			// TerrainHit test
			if (Physics.SphereCast(cam.position, rayRadius, cam.forward, out hit, 1000, terrainMask))
			{
				lastTerraformPointLocal = MathUtility.WorldToLocalVector(cam.rotation, hit.point);
				Terraform(hit.point);
				rayHitTerrain = true;
				break;
			}
		}

		if (hasHit)
		{
			sphere.transform.position = hitPoint;
		}

		if (!rayHitTerrain && wasTerraformingLastFrame)
		{
			Vector3 terraformPoint = MathUtility.LocalToWorldVector(cam.rotation, lastTerraformPointLocal);
			Terraform(terraformPoint);
		}

		//TODO: Anzahl iterationen von der Anzahl der Nutzer abhängig machen.
		for(int i=0; i<=5; i++) {				// Zahl 5 ist willkürlich gewählt, bei mehr Nutzern die zeitgleich handeln wird es zu Verzögerungen kommen
			if (actionList.Count == 0) break;
			if (actionList[0].Item2 == 1 || actionList[0].Item2 == 2)
			{
				Terraform(actionList[0].Item1, actionList[0].Item2);
			}
			// teleport der Clients
			else if (actionList[0].Item2 >= 3)
			{
				Teleport(actionList[0].Item1, actionList[0].Item2);
			}
			actionList.RemoveAt(0);
        }
	}

    private void OnDestroy()
    {
		serveThread.Interrupt();
    }

    void Terraform(Vector3 terraformPoint) // lokal und senden einer Nachricht
	{
		//Debug.DrawLine(cam.position, point, Color.green);
		hasHit = true;
		hitPoint = terraformPoint;

		const float dstNear = 10;
		const float dstFar = 60;

		float dstFromCam = (terraformPoint - cam.position).magnitude;
		float weight01 = Mathf.InverseLerp(dstNear, dstFar, dstFromCam);
		float weight = Mathf.Lerp(terraformSpeedNear, terraformSpeedFar, weight01);

		string vectorString;
		string msgString;

		string camposString;


		// hier Sachen ändern irgendwie den erstellten Controller ansprechen, da das MockHMD keine Steuerung hat
		// deswegen zur Zeit auch Maus/Tastatur Eingaben
		targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue);
		targetDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButtonValue);
		targetDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButtonValue);
		var inputFeatures = new List<UnityEngine.XR.InputFeatureUsage>();
		//Debug.Log(inputFeatures.Count); // 0 -> Keine Knöpfe
		if (targetDevice.TryGetFeatureUsages(inputFeatures))
		{
			foreach (var feature in inputFeatures)
			{
				if (feature.type == typeof(bool))
				{
					bool featureValue;
					if (targetDevice.TryGetFeatureValue(feature.As<bool>(), out featureValue))
					{
						Debug.Log(string.Format("Bool feature '{0}''s value is '{1}'",
								  feature.name, featureValue.ToString()));
					}
				}
			}
		}

		vectorString = terraformPoint.ToString().Trim('(', ')');

		// Add terrain
		if (primaryButtonValue || Input.GetKey("b") && dstFromCam > 8)
		{
			Debug.Log("Pressing Primary Button");
			isTerraforming = true;
			genTest.Terraform(terraformPoint, -weight, terraformRadius);
			try
			{
				msgString = clientID + "," + 1 + "," + vectorString + ",";
				msg = Encoding.Default.GetBytes(msgString);
				ns.Write(msg, 0, msg.Length);
			}
			catch (Exception) { }
		}
		// Subtract terrain
		else if (secondaryButtonValue || Input.GetKey("n"))
		{
			Debug.Log("Pressing Secondary Button");
			isTerraforming = true;
			genTest.Terraform(terraformPoint, weight, terraformRadius);
			try
			{
				msgString = clientID + "," + 2 + "," + vectorString + ",";
				msg = Encoding.Default.GetBytes(msgString);
				ns.Write(msg, 0, msg.Length);
			}
			catch (Exception) { }
		}
		// Teleport check
		else if (gripButtonValue || Input.GetKeyUp("g"))
        {
			Debug.Log("Releasing Grip Button");
			teleportAction = true;
			
		}
		else if (teleportAction)
        {
			if (campos != cam.position)
            {
				try
				{
					campos = cam.position;
					teleportAction = false;
					camposString = cam.position.ToString().Trim('(', ')');
					msgString = clientID + "," + 3 + "," + camposString + ",";
					msg = Encoding.Default.GetBytes(msgString);
					ns.Write(msg, 0, msg.Length);
				}
				catch (Exception) { }
			}
			
		}


			if (isTerraforming)
		{
			onTerrainModified?.Invoke();
		}
	}

	void Terraform(Vector3 terraformPoint, int wert) // erhaltene Nachricht ausführen
	{
		//Debug.DrawLine(cam.position, point, Color.green);
		hasHit = true;
		hitPoint = terraformPoint;

		const float dstNear = 10;
		const float dstFar = 60;

		float dstFromCam = (terraformPoint - cam.position).magnitude;
		float weight01 = Mathf.InverseLerp(dstNear, dstFar, dstFromCam);
		float weight = Mathf.Lerp(terraformSpeedNear, terraformSpeedFar, weight01);

		// Add terrain

		if (wert == 1)
		{
			Debug.Log("Pressing Primary Button");
			isTerraforming = true;
			genTest.Terraform(terraformPoint, -weight, terraformRadius);
		}
		// Subtract terrain
		else if (wert == 2)
		{
			Debug.Log("Pressing Secondary Button");
			isTerraforming = true;
			genTest.Terraform(terraformPoint, weight, terraformRadius);
		}


		if (isTerraforming)
		{
			onTerrainModified?.Invoke();
		}
	}

	// Teleport der Clients
	void Teleport(Vector3 teleportPoint, int clientNR)
    {
		clientNR = clientNR - 3;
		if (clientNR != clientID)
        {
			while (sphereList.Count <= clientNR)
            {
				GameObject newsphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				newsphere.transform.localScale = new Vector3((float)0.5, 1, (float)0.5);
				Renderer r = newsphere.GetComponent<Renderer>();
				r.material.color = Color.gray;
				sphereList.Add(newsphere);
            }
			sphereList[clientNR].transform.position = teleportPoint;
		}
		Debug.Log("Test");
    }

	// HitPoint test
	void OnDrawGizmos()
	{
		if (hasHit)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(hitPoint, 0.25f);
		}
	}
}
