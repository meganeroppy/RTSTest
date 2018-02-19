﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// RTSの影響を受けて動くオブジェクトの動きをシミュレートするクラス
/// 院生室以外で作業するときなど
/// RTSカメラ設備がない時用
/// </summary>
public class RtsMovementSample : MonoBehaviour
{
    [SerializeField]
    private bool enableMove = true;
    [SerializeField]
    private bool enableRotation = true;

    [SerializeField]
    private Transform rh;
    [SerializeField]
    private Transform lh;
    [SerializeField]
    private Transform rf;
    [SerializeField]
    private Transform lf;
    [SerializeField]
    private Transform head;
    [SerializeField]
    private Transform body;

    [SerializeField]
    private float moveSpeed = 1f;
    [SerializeField]
    private float rotSpeed = 1f;

    private Vector3 rhOriginPos = Vector3.right * 1f;
    private Vector3 lhOriginPos = Vector3.right * -1f;
    private Vector3 rfOriginPos = Vector3.right * 0.2f;
    private Vector3 lfOriginPos = Vector3.right * -0.2f;
    private Vector3 headOriginPos = Vector3.up * 1.5f;
    private Vector3 bodyOriginPos = new Vector3(0, 1.2f, -0.2f);

    private bool initialized = false;

    public void Init()
    {
        // 各オブジェクトのTrackerSettingを削除
        TrackerSettings component = null;
        component = rh.GetComponent<TrackerSettings>();
        if (component != null)
        {
            component.StopFlag();
            component.transform.localPosition = rhOriginPos;
            component.transform.localRotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        }
        component = lh.GetComponent<TrackerSettings>();
        if (component != null)
        {
            component.StopFlag();
            component.transform.localPosition = lhOriginPos;
            component.transform.localRotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        }
        component = rf.GetComponent<TrackerSettings>();
        if (component != null)
        {
            component.StopFlag();
            component.transform.localPosition = rfOriginPos;
            component.transform.localRotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        }
        component = lf.GetComponent<TrackerSettings>();
        if (component != null)
        {
            component.StopFlag();
            component.transform.localPosition = lfOriginPos;
            component.transform.localRotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        }
        component = head.GetComponent<TrackerSettings>();
        if (component != null)
        {
            component.StopFlag();
            component.transform.localPosition = headOriginPos;
            component.transform.localRotation = Quaternion.identity;
        }
        component = body.GetComponent<TrackerSettings>();
        if (component != null)
        {
            component.StopFlag();
            component.transform.localPosition = bodyOriginPos;
            component.transform.localRotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        if (!enableMove) return;

        // 手を上下にばたばた
        {
            rh.transform.localPosition = rhOriginPos + new Vector3(0, Mathf.PingPong(Time.time * moveSpeed, 1f), 0);
            lh.transform.localPosition = lhOriginPos + new Vector3(0, Mathf.PingPong(Time.time * moveSpeed, 1f), 0);
        }

        // 必要であれば頭も多少動かす
        {
            head.transform.localPosition = headOriginPos + new Vector3(0, Mathf.PingPong(Time.time * moveSpeed * 0.5f, 1f), 0);
        }
    }

    private void UpdateRotation()
    {
        if (!enableRotation) return;

        // 両手はZ軸でくるくる回す
        {
            rh.transform.Rotate(Vector3.right * rotSpeed * Time.deltaTime);
            lh.transform.Rotate(Vector3.up * rotSpeed * Time.deltaTime);
        }

        // 頭はY軸で若干回す
        {
            rh.transform.Rotate(Vector3.up * rotSpeed * Time.deltaTime);
        }
    }
}
