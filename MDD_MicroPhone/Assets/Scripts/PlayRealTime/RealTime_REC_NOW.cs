using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class RealTime_REC_NOW : MonoBehaviour
{
    [SerializeField]
    public Text stt;
    private AudioClip mic;
    private Text _buttonText;

    public ToggleGroup _toggleGroup_micList;
    public AudioSource source;

    private bool _isRecord;

    private int lastSample = 0;
    private int channels = 0;
    private int readUpdateId = 0;
    private int previousReadUpdateId = -1;

    private float[] samples = null;

    //private int cur = 0;
    private float time = 0;
    private void Awake()
    {
        this._buttonText = this.gameObject.GetComponentInChildren<Text>();
    }

    void Start()
    {
        this._isRecord = false;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(this._isRecord) // 녹음 중이 아니면 실행하지 않는다.
        {
            //Debug.Log("Time.deltaTime : " + Time.deltaTime);
            //ReadMic();
            //PlayMic();
            //cur++;
        }
    }

    private void FixedUpdate()
    {
        if (this._isRecord) // 녹음 중이 아니면 실행하지 않는다.
        {
            ReadMic();
            PlayMic();
        }
    }

    public void ShowSelectToggleName()
    {
        if (!Microphone.IsRecording(selectToggle.name))
        {
            this._buttonText.text = "녹음 종료";
            this._isRecord = true;
            DeviceManager.instance.SetAllToggleInteracable(false); // 녹음을 시작하면 Toggle을 조작할 수 없도록 모든 토글을 비활성화하는 함수를 호출
            this.mic = Microphone.Start(selectToggle.name, true, 100, 44100); //You can just give null for the mic name, it's gonna automatically detect the default mic of your system (k)
            this.channels = this.mic.channels; //mono or stereo, for me it's 1 (k)
        }
        else
        {
            this._buttonText.text = "녹음 시작";
            this._isRecord = false;
            DeviceManager.instance.SetAllToggleInteracable(true); // 녹음을 정지하면 다시 Toggle을 조작할 수 있도록 모든 토글을 활성화하는 함수를 호출
            Microphone.End(selectToggle.name);
            this.lastSample = 0; // 샘플 값 초기화
            this.readUpdateId = 0; // 업데이트 값 초기화
            this.previousReadUpdateId = -1; // 이전 업데이트 값 초기화
        }

    }

    private Toggle selectToggle
    {
        get { return this._toggleGroup_micList.ActiveToggles().FirstOrDefault(); }
        // C#의 구문(LINQ)
        // 활성화된 토글을 검색하면서, 가장 처음으로 활성화된 토글을 반환한다.
        // First와 FirstOrDefault의 차이는, 반환 객체의 존재 유무이다. 
        // First는 반환객체가 없으면 오류가 발생하고, FirstOrDefault는 반환이 없어도 됨.
    }
    int readcount = 0;
    private void ReadMic()
    {
        int t_pos = Microphone.GetPosition(selectToggle.name);
        int t_diff = t_pos - this.lastSample;

        if (t_diff > 0)
        {
            Debug.Log("Read time : " + time);
            this.samples = new float[t_diff * this.channels];
            this.mic.GetData(this.samples, this.lastSample);
            Debug.Log(readcount + "번째 Read! t_pos : " + t_pos);
            Debug.Log(readcount + "번째Read! t_diff : " + t_diff);
            Debug.Log(readcount + "번째Read! this.samples.Length : " + this.samples.Length);
            readcount++;
        }
        this.lastSample = t_pos;
    }
    int playcount = 0;
    private void PlayMic()
    {
        if (this.samples != null) // 맨 첫 프레임에서는 diff가 0이어서 float가 할당되지 않으므로, null 체크를 해줘야 함.
        {
            if (this.readUpdateId != this.previousReadUpdateId) // 렉으로 인한 중복 재생을 방지하기 위한 조건문
            {
                Debug.Log("Play time : " + time);
                this.previousReadUpdateId = this.readUpdateId; // 값 일치

                this.source.clip = AudioClip.Create("Real_time", this.samples.Length, this.channels, 44100, false);
                this.source.spatialBlend = 0; //2D sound

                this.source.clip.SetData(samples, 0);

                if (!this.source.isPlaying)
                {
                    Debug.Log(playcount + "번째 Play! sample.Length : " + this.samples.Length);

                    byte[] byteData = getByteFromAudioClip(this.source.clip);
                    StartCoroutine(CLOVAAPI.PostClova(byteData));
                    this.source.Play();
                    stt.text = CLOVAAPI._text;
                }
                playcount++;
                this.readUpdateId++; // 읽었다는 것을 표시하기 위한 readUpdateId 값 변경
            }
        }
    }

    
     
    private byte[] getByteFromAudioClip(AudioClip audioClip)
    {
        MemoryStream stream = new MemoryStream();
        const int headerSize = 44;
        ushort bitDepth = 16;

        int fileSize = audioClip.samples * WavUtility.BlockSize_16Bit + headerSize;

        // audio clip의 정보들을 file stream에 추가
        WavUtility.WriteFileHeader(ref stream, fileSize);
        WavUtility.WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
        WavUtility.WriteFileData(ref stream, audioClip, bitDepth);

        // stream을 array형태로 바꿈
        byte[] bytes = stream.ToArray();

        return bytes;
    }

}
