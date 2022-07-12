using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Fireworks Mania/Gui/ComponentBridge")]
public class ComponentBridge : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> objects = new List<GameObject>();

    /// <summary>
    /// Returns the Component on the GamoObject at the given index
    /// </summary>
    public T Get<T>(int index) where T : Component
    {
        if (index >= objects.Count) return null;
        T o = objects[index].GetComponent<T>();
        if (o == null)
        {
            Debug.LogError("Could not find type on object " + index);
            return null;
        }
        return o;
    }
}