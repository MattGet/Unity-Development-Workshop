#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

[ExecuteAlways]
public class CakeCreator : MonoBehaviour
{

    public GameObject CakeTube;

    public Material TubeMaterial;

    public GameObject Pellet;

    public float BoxZ;

    public float BoxX;

    public float TubeHeight;




    public float NumberOfTubesZ;

    public float NumberOfTubesX;

    public bool SizeDirectionX;

    private GameObject CakeTubes;

    [Button("Update Cake Tubes")]
    public void UpdateSize()
    {
        if (CakeTube == null)
        {
            return;
        }
        if (NumberOfTubesX == 0 || NumberOfTubesZ == 0 || TubeHeight == 0)
        {
            return;
        }


        GameObject TubeToSpawn = CakeTube;
        float TubeZ = BoxZ / NumberOfTubesZ / 2;
        float TubeX = BoxX / NumberOfTubesX / 2;
        float TubeY = TubeHeight;
        if (SizeDirectionX)
        {
            TubeZ = TubeX;
        }
        else
        {
            TubeX = TubeZ;
        }

        TubeToSpawn.transform.localScale = new Vector3(TubeX, TubeY, TubeZ);
        SpawnTubes(TubeToSpawn);
    }

    public void SpawnTubes(GameObject Tube)
    {
        foreach (Transform T in this.gameObject.transform)
        {
            if (T.gameObject.name == "CakeTubes")
            {
                CakeTubes = T.gameObject;
            }
        }
        if (CakeTubes != null)
        {
            DestroyImmediate(CakeTubes);
        }
        CakeTubes = GameObject.Instantiate(new GameObject(), this.gameObject.transform);
        CakeTubes.name = "CakeTubes";
        GameObject TubeTemp = GameObject.Instantiate(Tube, CakeTubes.transform);
        if (TubeMaterial != null)
        {
            MeshRenderer temp = TubeTemp.GetComponent<MeshRenderer>();
            temp.material = TubeMaterial;
        }
        GameObject PelletImposter = GameObject.Instantiate(Pellet, TubeTemp.transform);
        PelletImposter.transform.localPosition = Vector3.zero;

        float TubeSizeZ = Tube.transform.localScale.z * 2;
            float TubeSizeX = Tube.transform.localScale.x * 2;

            float Zpos = (-(BoxZ / 2) + TubeSizeZ / 2);

            int id = 0;
            for (int i = 0; i < NumberOfTubesZ; i++)
            {
                float Xpos = (-(BoxX / 2) + TubeSizeX / 2);
                for (int k = 0; k < NumberOfTubesX; k++)
            {
                Debug.Log($"Spawning Tube: id = {id}, Xpos = {Xpos}, Zpos = {Zpos}");
                TubeTemp.name = $"Cake Tube ({id})";
                Selection.activeGameObject = TubeTemp;
                Unsupported.CopyGameObjectsToPasteboard();
                Unsupported.PasteGameObjectsFromPasteboard();
                var copy = Selection.activeGameObject;
                copy.transform.parent = CakeTubes.transform;
                copy.transform.localPosition = new Vector3(Xpos, 0, Zpos);
                    //Debug.Log(Tube.transform.localPosition.ToString("f4"));
                    
                Xpos = Xpos + TubeSizeX;
                id++;
            }
            Zpos = Zpos + TubeSizeZ;
        }
        DestroyImmediate(TubeTemp);
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 one = new Vector3(-BoxX / 2, 0, 0);
        Vector3 two = new Vector3(BoxX / 2, 0, 0);
        Vector3 three = new Vector3(0, 0, -BoxZ / 2);
        Vector3 four = new Vector3(0, 0, BoxZ / 2);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(one, two);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(three, four);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(BoxX, 0, BoxZ));
    }
}
#endif