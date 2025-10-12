using Unity.VisualScripting;
using UnityEngine;

public class CortineMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InchideCopertine()
    {
        // Logica pentru a închide cortinele
        Debug.Log("Cortinele se închid!");
        // Aici poți adăuga animația sau logica specifică pentru închiderea cortinelor
        if (gameObject.name == "CortinaStanga")
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(-3, transform.position.y, transform.position.z), 1f);
        }
        else if (gameObject.name == "CortinaDreapta")
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(4, transform.position.y, transform.position.z), 1f);
        }
    }
}
