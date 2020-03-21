using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Wash : MonoBehaviour, IPointerDownHandler
{
#if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void OpenNHS();
    public void OnPointerDown(PointerEventData ped)
    {
        OpenNHS();
    }
#else
    public void OnPointerDown(PointerEventData ped)
    {
    }
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(()=> Application.OpenURL("https://www.nhs.uk/live-well/healthy-body/best-way-to-wash-your-hands/"));
    }
#endif
}
