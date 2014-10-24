using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameMain : MonoBehaviour
{
    public GameObject UpMarkerTemplate;
    public GameObject DownMarkerTemplate;
    Queue<MarkerInfo> Markers;
    bool IsMousePressing = false;

    public AudioSource Audio;
    public AudioLowPassFilter LPF;
    public AudioChorusFilter Chorus;

    MarkerInfo CurrentMarker;
    MarkerView CurrentUpMarker;
    MarkerView CurrentDownMarker;
    SlideLineView LineView;

    void Awake()
    {
        var bpm = 124f;

        UpMarkerTemplate.SetActive(false);
        DownMarkerTemplate.SetActive(false);

        Markers = new Queue<MarkerInfo>();

        Markers.Enqueue(new MarkerInfo()
            {
                Down = new MarkerDefinition(bpm, 1, 0, new Vector2(-100f, -200)),
                Up = new MarkerDefinition(bpm, 1, 4, new Vector2(100f, 200)),
                Effect = EffectType.HPF,
            }
        );

        Markers.Enqueue(new MarkerInfo()
            {
                Down = new MarkerDefinition(bpm, 2, 0, new Vector2(0f, 200)),
                Up = new MarkerDefinition(bpm, 2, 2, new Vector2(150f, -250f)),
                Effect = EffectType.Chorus,
            }
        );

        Markers.Enqueue(new MarkerInfo()
            {
                Down = new MarkerDefinition(bpm, 3, 0, new Vector2(0f, 100)),
                Up = new MarkerDefinition(bpm, 3, 2, new Vector2(150f, 150f)),
                Effect = EffectType.LPF,
            }
        );

        Markers.Enqueue(new MarkerInfo()
        {
            Down = new MarkerDefinition(bpm, 4, 0, new Vector2(0f, -100)),
            Up = new MarkerDefinition(bpm, 5, 0, new Vector2(150f, 150f)),
            Effect = EffectType.HPF,
        }
        );

        Markers.Enqueue(new MarkerInfo()
        {
            Down = new MarkerDefinition(bpm, 6, 0, new Vector2(0f, 100)),
            Up = new MarkerDefinition(bpm, 7, 0, new Vector2(150f, -150f)),
            Effect = EffectType.LPF,
        }
        );

        StopAllEffects();
    }

    void StopAllEffects()
    {
        LPF.enabled = false;
        Chorus.enabled = false;
    }

    IEnumerator Start()
    {
        Audio.Play();

        while (true)
        {
            if (CurrentDownMarker != null)
            {
                CurrentDownMarker.Fill.transform.localScale =
                    Vector2.one * Mathf.Clamp01((1 - ((CurrentMarker.Down.TimingSec - Audio.time) / 0.5f)));
            }

            if (CurrentUpMarker != null)
            {
                CurrentUpMarker.Fill.transform.localScale =
                    Vector2.one * Mathf.Clamp01((1 - ((CurrentMarker.Up.TimingSec - Audio.time) / 0.5f)));
            }

            if (Markers.Count != 0)
            {
                var nextMarker = Markers.Peek();
                if (nextMarker.Down.TimingSec - 0.5f <= Audio.time)
                {
                    CurrentMarker = Markers.Dequeue();

                    if (CurrentDownMarker != null)
                        Destroy(CurrentDownMarker.gameObject);
                    if (CurrentUpMarker != null)
                        Destroy(CurrentUpMarker.gameObject);

                    CurrentDownMarker = ((GameObject)Instantiate(DownMarkerTemplate)).GetComponent<MarkerView>();
                    CurrentDownMarker.transform.localPosition = CurrentMarker.Down.Position;
                    CurrentDownMarker.gameObject.SetActive(true);
                    CurrentDownMarker.Fill.transform.localScale = Vector2.zero;

                    CurrentUpMarker = ((GameObject)Instantiate(UpMarkerTemplate)).GetComponent<MarkerView>();
                    CurrentUpMarker.transform.localPosition = CurrentMarker.Up.Position;
                    CurrentUpMarker.gameObject.SetActive(true);
                    CurrentUpMarker.Fill.transform.localScale = Vector2.zero;
                }
            }

            yield return null;
        }
    }

    void OnMouseDown()
    {
        Debug.Log("マウスダウン");
        if (CurrentMarker == null) return;
        IsMousePressing = true;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (CurrentDownMarker.Frame.bounds.IntersectRay(ray))
            CurrentDownMarker.gameObject.SetActive(false);

        switch (CurrentMarker.Effect)
        {
        case EffectType.LPF:
            LPF.enabled = true;
            break;
        case EffectType.Chorus:
            Chorus.enabled = true;
            break;
        }
    }

    void OnMouseUp()
    {
        Debug.Log("マウスアップ");
        if (CurrentMarker == null) return;
        IsMousePressing = false;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (CurrentUpMarker.Frame.bounds.IntersectRay(ray))
            CurrentUpMarker.gameObject.SetActive(false);

        StopAllEffects();
    }

    void Update()
    {
        if (CurrentMarker == null || !IsMousePressing) return;

        switch (CurrentMarker.Effect)
        {
        case EffectType.LPF:
            DoLPF();
            break;
        case EffectType.Chorus:
            break;
        }
    }

    void DoLPF()
    {
        LPF.cutoffFrequency = Mathf.Lerp(0, 22000, Input.mousePosition.y / Screen.width);
    }
}
