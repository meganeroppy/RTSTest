using UnityEngine;

public class Chaperone : MonoBehaviour
{
    public static Chaperone instance;
    public GameObject player{get; set;}

   // Use this for initialization
    void Start()
    {
        instance = this;
    }

   // Update is called once per frame
    void Update()
    {
        if( player )
        GetComponent<Renderer>().material.SetVector("_PlayerPos", player.transform.position);
   }
}