using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CLOVAAPI : MonoBehaviour
{

    public static String _text = "";
    public static IEnumerator PostClova(byte[] data)
    {
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post("https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor", form);

        // 헤더 설정 
        request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", "");
        request.SetRequestHeader("X-NCP-APIGW-API-KEY", "");
        request.SetRequestHeader("Content-Type", "application/octet-stream");

        // 바디
        request.uploadHandler = new UploadHandlerRaw(data);

        // response 대기
        yield return request.SendWebRequest();
        if (request == null)
        {
            Debug.LogError(request.error);
        } else
        {
            string message = request.downloadHandler.text;
            JSONDATA jsonData = JsonUtility.FromJson<JSONDATA>(message);
            _text += jsonData.text;
            Debug.Log("Voice Server responded: " + _text);
        }

    }
}

[Serializable]
public class JSONDATA
{
    public string text;
}