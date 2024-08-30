
using com.playbux.minimap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Zenject;

public class FullMiniMapController : MonoBehaviour
{
    [SerializeField]
    private MiniMapLocator miniMapLocator;

    private Vector3 targetScale;

    public MiniMapLocator MiniMapLocator { get => miniMapLocator; set => miniMapLocator = value; }

    private void Start()
    {

        this.transform.localScale = Vector3.zero;
    }

    public void Show()
    {
        this.targetScale = Vector3.one * 0.83f;

    }

    public void Hide()
    {

        this.targetScale = Vector3.zero;

    }

    void Update()
    {
        this.transform.localScale += (targetScale - this.transform.localScale) / 8f;
        if ((this.transform.localScale - targetScale).magnitude < 0.005 && targetScale == Vector3.zero)
        {
            Hide();
        }
    }


}