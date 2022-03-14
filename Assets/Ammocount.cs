using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammocount : MonoBehaviour
{
    // Start is called before the first frame update
    TMPro.TextMeshPro textMesh;
    public void ChangeAmmo(int number)
    {
        textMesh = GetComponent<TMPro.TextMeshPro>();
        textMesh.text = number.ToString();
    }
}
