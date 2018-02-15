using UnityEngine;

public class Chaperone : MonoBehaviour
{
    [SerializeField]
    GameObject player_;

   // Use this for initialization
    void Start()
    {
    //    GetComponent<Renderer>().material.SetVector("_PlayerPos", player_.transform.position);
    }

   // Update is called once per frame
    void Update()
    {
        if( player_ )
        GetComponent<Renderer>().material.SetVector("_PlayerPos", player_.transform.position);
   }
}