using UnityEngine;
using UnityEngine.SceneManagement;

public class ToC2 : MonoBehaviour
{
     private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("C2_Commerical");// Change with scene name, will add spiter locations later
        }
    }
}