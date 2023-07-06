using UnityEngine;

public class ShieldModel : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer[] glass = new MeshRenderer[2];

    [SerializeField]
    private MeshRenderer[] frame = new MeshRenderer[2];

    [SerializeField]
    private Material[] glassMaterials = new Material[2];
    [SerializeField]
    private Material[] frameMaterials = new Material[2];

    [SerializeField]
    private int index = -1;

    private void Start()
    {
        SetStyle(index);
    }

    public void SetStyle(int i)
    {
        index = i;
        frame[0].material = frameMaterials[i];
        frame[1].material = frameMaterials[i];
        glass[0].material = glassMaterials[i];
        glass[1].material = glassMaterials[i];
    }
}
