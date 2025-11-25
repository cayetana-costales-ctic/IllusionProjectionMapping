using UnityEngine;

public class Instantiator : MonoBehaviour
{
    // Optional Parent
    public Transform parentTransform;

    public GameObject InstantiatePrimitive(PrimitiveType type, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.localScale = scale;

        if (parentTransform != null)
            obj.transform.SetParent(parentTransform);

        return obj;
    }


    public GameObject InstantiatePrimitive(PrimitiveType type)
    {
        return InstantiatePrimitive(type, Vector3.zero, Quaternion.identity, new Vector3(0.1f, 0.1f, 0.1f));
    }

    public void SpawnPrimitiveByType(int typeIndex)
    {
        PrimitiveType type = (PrimitiveType)typeIndex;
        InstantiatePrimitive(type, Vector3.zero, Quaternion.identity, new Vector3(0.1f, 0.1f, 0.1f));
    }

    public void SpawnCube()
    {
        InstantiatePrimitive(PrimitiveType.Cube, Vector3.zero, Quaternion.identity, Vector3.one);
    }

    public void SpawnSphere()
    {
        InstantiatePrimitive(PrimitiveType.Sphere, Vector3.zero, Quaternion.identity, Vector3.one);
    }

    public void SpawnPlane()
    {
        InstantiatePrimitive( PrimitiveType.Plane, Vector3.zero, Quaternion.identity, new Vector3(0.1f, 0.1f, 0.1f));
    }

}
