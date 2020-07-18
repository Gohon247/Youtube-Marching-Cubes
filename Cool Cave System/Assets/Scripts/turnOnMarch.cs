using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class turnOnMarch : MonoBehaviour
{

    public Color off;
    public Color on;
    Sphere[] sphere;

    private void Start()
    {
        sphere = new Sphere[8];
        sphere[1].position = new Vector3(10, 0, 0);
        sphere[2].position = new Vector3(10, 0, 10);
        sphere[3].position = new Vector3(0, 0, 10);
        sphere[4].position = new Vector3(0, 10, 0);
        sphere[5].position = new Vector3(10, 10, 0);
        sphere[6].position = new Vector3(10, 10, 10);
        sphere[7].position = new Vector3(0, 10, 10);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 500.0f))
            {
                if (hit.transform)
                {
                    //Debug.Log(hit.transform.gameObject);
                    GameObject oSphere = hit.transform.gameObject;
                    SetOnOff(oSphere);
                    RunCubes();
                }
            }
        }
    }

    void SetOnOff(GameObject oSphere)
    {
        int name = Int32.Parse(oSphere.name);
        if (sphere[name].onSphere == true)
        {
            oSphere.GetComponent<MeshRenderer>().material.color = off;
            sphere[name].onSphere = false;
        }
        else
        {
            oSphere.GetComponent<MeshRenderer>().material.color = on;
            sphere[name].onSphere = true;
        }
    }

    void RunCubes()
    {
        DisplayMap display = FindObjectOfType<DisplayMap>();
        display.DrawMesh(MeshGen.GenerateTerrainMesh(sphere));
    }

    public struct Sphere
    {
        public bool onSphere;
        public Vector3 position;
    }
}
