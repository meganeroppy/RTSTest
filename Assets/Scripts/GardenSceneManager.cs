using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガーデンシーンの制御
/// 現状ほぼ床崩壊イベント管理のみ
/// </summary>
public class GardenSceneManager : MonoBehaviour
{
    public static GardenSceneManager instance;

    public ExplosionSource expSource;

    // 崩れ力
	public float force = 0f;

	public float startScale = 0;
	public float endScale = 100f;
	private float currentScale;

	public float gainScaleRate = 0.1f;

	public FracturedObject fractuedObject;
	public float upperForce = 15f;

	public Transform propsParent;

    [SerializeField]
    private AudioSource collapseSound;

    [SerializeField]
    private GameObject terrainOutside;

    [SerializeField]
    private Transform bottomWall;

    [SerializeField]
    private float bottomWallRotSpeed = 300;

    [SerializeField]
    private float fogEndDistance = 50f;

    [SerializeField]
    private float gainFogDensityRate = 0.2f;

    [SerializeField]
    private float terrainEndPosOffset = -60f;

    // Use this for initialization
    void Start ()
    {
        instance = this;

		currentScale = startScale;
	}

    private void Update()
    {
        bottomWall.Rotate(Vector3.up * bottomWallRotSpeed * Time.deltaTime);
    }

    public void PlayEvent()
	{
		StartCoroutine( ExecEvent() );
	//	StartCoroutine( AscendChunks() );

	}

    private IEnumerator ExecEvent()
    {
        // TODO: 地鳴りみたいな音あってもいいかも

        // フォグを徐々に強くし、テラインの位置を下げる
        {
            var originFogDensity = RenderSettings.fogEndDistance;
            var fogDiff = Mathf.Abs( fogEndDistance - originFogDensity );

            var terrainOriginPos = terrainOutside.transform.position;

            float progress = 0;
            while (progress < 1)
            {
                progress += Time.deltaTime * gainFogDensityRate;

                Debug.Log(RenderSettings.fogEndDistance);

                RenderSettings.fogEndDistance = originFogDensity - fogDiff * progress;

                terrainOutside.transform.position = terrainOriginPos + Vector3.up * terrainEndPosOffset * progress;

                yield return null;
            }

        }

        // テラインを非表示にする
        terrainOutside.SetActive(false);

        // 地面の上の置物のIsKinematicを無効にする
        if (propsParent != null)
        {
            var props = propsParent.gameObject.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in props)
            {
                rb.isKinematic = false;
            }
        }

        // 地面が崩れる効果音を鳴らす
        collapseSound.Play();

        // 地面が崩れる
        {
            expSource.enabled = true;
            expSource.Force = force;

            var diff = endScale - startScale;

            while (currentScale < endScale)
            {
                currentScale += diff * gainScaleRate * Time.deltaTime;

                expSource.InfluenceRadius = currentScale;

                yield return null;
            }
        }

        // 崩れる効果音停止
        collapseSound.Stop();

        Debug.Log("CollapseFloor::ExecEvent End");
	}

	private IEnumerator AscendChunks()
	{
		var chunks = fractuedObject.ListFracturedChunks;
		chunks.ForEach( o => 
			o.gameObject.AddComponent<UpperMove>()
		 );

		bool pushAll = false;
		while(!pushAll)
		{
			pushAll = chunks.Count > 0;
			foreach( FracturedChunk o in chunks){
				var rb = o.GetComponent<Rigidbody>();
				if( rb.isKinematic )
				{
					pushAll = false;
					continue;
				}
				var um = rb.GetComponent<UpperMove>();
				if( !um.pushed )
				{
					pushAll = false;
					rb.AddForce( Vector3.up * upperForce );
					um.pushed = true;
				}

			}
			yield return null;
		}
		Debug.Log("CollapseFloor::AscendChunks End");

	}
}

public class UpperMove : MonoBehaviour
{
	public bool pushed = false;
}