using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GIFMaker : MonoBehaviour
{
    public uint width = 320, height = 180;
    public bool useScreenSize;
    [Min(1)]
    public int fps = 1, seconds = 1;
    public int QueueCount;
    public bool recorderRunning = false;
    private int amountOfFrames => fps * seconds;
    private float frameInterval => (float)1 / (float)fps;
    private float currentTime;
    public Queue<Texture2D> frameQueue;
    private Coroutine recorder;
    private Camera cam;
    private RenderTexture rt,defaultTexture;
    private void OnValidate()
    {
        if (useScreenSize)
        {
            width = (uint)Screen.width;
            height = (uint)Screen.height;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        frameQueue = new Queue<Texture2D>(amountOfFrames);
        rt = new RenderTexture((int)width, (int)height, 24);
        cam = Camera.main;
        defaultTexture = cam.targetTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)&&recorderRunning)
        {
            SavePNGs();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            recorderRunning = !recorderRunning;
            currentTime = 0;
            if (!recorderRunning)
            {
                StopCoroutine(recorder);
            }
            else
            {
                frameQueue = new Queue<Texture2D>(amountOfFrames);
                recorder = StartCoroutine(RecordFrameBuffer());
            }
        }
        if (recorderRunning)
        {
            currentTime += Time.deltaTime;
        }
    }

    private IEnumerator RecordFrameBuffer()
    {
        Debug.Log("Started Recording");
        while (true)
        {
            if(currentTime>frameInterval)
            {
                currentTime = 0;
                cam.targetTexture = rt;
                Texture2D screenshot = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                cam.Render();
                RenderTexture.active = rt;
                screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                cam.targetTexture = defaultTexture;
                RenderTexture.active = null;
                
                if (frameQueue.Count>=amountOfFrames)
                {
                    frameQueue.Dequeue();
                }
                frameQueue.Enqueue(screenshot);
                QueueCount=frameQueue.Count;
            }
            yield return null;
        }
    }

    async void SavePNGs()
    {
        StopCoroutine(recorder);
        Task[] tasks = new Task[frameQueue.Count];
        var date = DateTime.Now.Date;
        var time = DateTime.Now.TimeOfDay;
        var dir = Application.persistentDataPath + $"/{date.Year}-{date.Month}-{date.Day}_{time.Hours}-{time.Minutes}-{time.Seconds}";
        GUIUtility.systemCopyBuffer = dir;
        int count = frameQueue.Count;
        for (int i = 0; i < count; i++)
        {
            var texture = frameQueue.Dequeue();
            byte[] bytes = texture.EncodeToPNG();
            await WriteFile(i, bytes, dir);
        }
        //await Task.WhenAll(tasks);
        StartCoroutine(RecordFrameBuffer());
    }

    private async Task WriteFile(int i, byte[] bytes, string dir)
    {
        //await Task.Yield();
        if (!Directory.Exists(dir))Directory.CreateDirectory(dir);
        File.WriteAllBytes($"{dir}/SavedScreen{i}.png",bytes);
        await Task.Delay(100);
    }
}
