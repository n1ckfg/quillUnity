﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using ICSharpCode.SharpZipLibUnityPort.Core;
using ICSharpCode.SharpZipLibUnityPort.Zip;
using SimpleJSON;

public class QuillLoader : MonoBehaviour {

    public QuillStroke prefab;
    public string readFileName;
    public int numStrokes;
    public List<QuillStroke> strokes;

    [HideInInspector] public JSONNode json;
    [HideInInspector] public byte[] bytes;

    private ZipFile zipFile;

    void Start() {
        read(readFileName);
    }

	public void read(string readFileName) {
        StartCoroutine(reader(readFileName));
	}

	private IEnumerator reader(string readFileName) {
        // A quill zipfile should contain three items: Quill.json, Quill.qbin, and State.json
        // Quill.json describes data structures with an index in the Quill.qbin binary blob.
        string url = formPath(readFileName);

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        getEntriesFromZip(www.downloadHandler.data);

        parseQuill();
    }

    private byte[] readEntryAsBytes(ZipEntry _entry) {
        Stream zippedStream = zipFile.GetInputStream(_entry);
        MemoryStream ms = new MemoryStream();
        zippedStream.CopyTo(ms);
        return ms.ToArray();
    }

    private string readEntryAsString(ZipEntry _entry) {
        Stream zippedStream = zipFile.GetInputStream(_entry);
        StreamReader read = new StreamReader(zippedStream, true);
        return read.ReadToEnd();
    }

    private JSONNode readEntryAsJson(ZipEntry _entry) {
        return JSON.Parse(readEntryAsString(_entry));
    }

    private void getEntriesFromZip(byte[] _bytes) {
        // https://gist.github.com/r2d2rigo/2bd3a1cafcee8995374f

        MemoryStream fileStream = new MemoryStream(_bytes, 0, _bytes.Length);
        zipFile = new ZipFile(fileStream);

        foreach (ZipEntry entry in zipFile) {
			switch(entry.Name.ToLower()) {
                case "quill.json":
                    json = readEntryAsJson(entry);
                    break;
                case "quill.qbin":
                    bytes = readEntryAsBytes(entry);
                    break;
			}
        }
    }

	private void parseQuill() {
        strokes = new List<QuillStroke>();

        foreach (JSONNode childNode in json["Sequence"]["RootLayer"]["Implementation"]["Children"]) {
            Debug.Log(childNode["Name"]);
            foreach (JSONNode drawingNode in childNode["Implementation"]["Drawings"]) {
                Debug.Log(drawingNode["DataFileOffset"]);
        
                numStrokes = BitConverter.ToInt32(bytes, 31104108);
                Debug.Log(numStrokes);
                /*
                int offset = 20;

                for (int i = 0; i < numStrokes; i++) {
                    int brushIndex = BitConverter.ToInt32(bytes, offset);

                    float r = BitConverter.ToSingle(bytes, offset + 4);
                    float g = BitConverter.ToSingle(bytes, offset + 8);
                    float b = BitConverter.ToSingle(bytes, offset + 12);
                    float a = BitConverter.ToSingle(bytes, offset + 16);
                    Color brushColor = new Color(r, g, b, a);

                    float brushSize = BitConverter.ToSingle(bytes, offset + 20);
                    UInt32 strokeMask = BitConverter.ToUInt32(bytes, offset + 24);
                    UInt32 controlPointMask = BitConverter.ToUInt32(bytes, offset + 28);

                    int offsetStrokeMask = 0;
                    int offsetControlPointMask = 0;

                    for (int j = 0; j < 4; j++) {
                        byte bb = (byte)(1 << j);
                        if ((strokeMask & bb) > 0) offsetStrokeMask += 4;
                        if ((controlPointMask & bb) > 0) offsetControlPointMask += 4;
                    }

                    offset += 28 + offsetStrokeMask + 4;

                    int numControlPoints = BitConverter.ToInt32(bytes, offset);

                    //parent.println("3. " + numControlPoints);

                    List<Vector3> positions = new List<Vector3>();

                    offset += 4;

                    for (int j = 0; j < numControlPoints; j++) {
                        float x = BitConverter.ToSingle(bytes, offset + 0);
                        float y = BitConverter.ToSingle(bytes, offset + 4);
                        float z = BitConverter.ToSingle(bytes, offset + 8);
                        positions.Add(new Vector3(x, y, z));

                        //float qw = BitConverter.ToSingle(bytes, offset + 12);
                        //float qx = BitConverter.ToSingle(bytes, offset + 16);
                        //float qy = BitConverter.ToSingle(bytes, offset + 20);
                        //float qz = BitConverter.ToSingle(bytes, offset + 24);

                        offset += 28 + offsetControlPointMask;
                    }

                    QuillStroke stroke = Instantiate(prefab, transform.position, transform.rotation).GetComponent<QuillStroke>();
                    stroke.transform.parent = transform;
                    stroke.init(positions, brushSize, brushColor);
                    strokes.Add(stroke);
                }
                */
            }
        }
    }

    private string formPath(string readFileName) {
        string url = "";
#if UNITY_ANDROID
		url = Path.Combine("jar:file://" + Application.streamingAssetsPath + "!/assets/", readFileName);
#endif

#if UNITY_IOS
		url = Path.Combine("file://" + Application.streamingAssetsPath + "/Raw", readFileName);
#endif

#if UNITY_EDITOR
        url = Path.Combine("file://" + Application.streamingAssetsPath, readFileName);
#endif

#if UNITY_STANDALONE_WIN
        url = Path.Combine("file://" + Application.streamingAssetsPath, readFileName);
#endif

#if UNITY_STANDALONE_OSX
        url = Path.Combine("file://" + Application.streamingAssetsPath, readFileName);
#endif

#if UNITY_WSA
		url = Path.Combine("file://" + Application.streamingAssetsPath, readFileName);		
#endif
		return url;
    }

}
