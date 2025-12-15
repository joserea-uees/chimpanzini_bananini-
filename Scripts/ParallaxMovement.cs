using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMovement : MonoBehaviour
{
    Transform cam; 
    Vector3 camStartPos;
    float distance; 

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;

    float farthestBack;

    [Range(0.01f, 1f)]
    public float parallaxSpeed;

    [Tooltip("Desplazamiento horizontal del parallax (positivo = más a la derecha, negativo = más a la izquierda)")]
    public float parallaxOffsetX = 2f;  // ¡Ajusta este valor en el Inspector!

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        farthestBack = 0f;  // ¡FIX: Inicializar para evitar errores!

        // Calcular la capa más lejana (mayor distancia Z)
        for (int i = 0; i < backCount; i++) 
        {
            float zDiff = backgrounds[i].transform.position.z - cam.position.z;
            if (zDiff > farthestBack)
            {
                farthestBack = zDiff;
            }
        }

        // Calcular velocidades relativas
        for (int i = 0; i < backCount; i++) 
        {
            float zDiff = backgrounds[i].transform.position.z - cam.position.z;
            backSpeed[i] = 1 - (zDiff / farthestBack);
        }
    }

    private void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;

        // ¡Posicionar el parallax con offset para más a la derecha!
        transform.position = new Vector3(cam.position.x + parallaxOffsetX, transform.position.y, 0f);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance * speed, 0));
        }
    }
}