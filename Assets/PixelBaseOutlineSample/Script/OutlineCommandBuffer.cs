using UnityEngine;
using UnityEngine.Rendering;

public class OutlineCommandBuffer : MonoBehaviour
{
    public Color OutlineColor = Color.white;
    [Range(0, 2)]
    public float OutlineWidth = 1.026f;
    public bool isNormalExtrude = false;

    OutlineSelectObject selectObj;
    CommandBuffer buffer;
    Camera cam;
    public Material mat;
    Renderer target;

    bool isRunTime;
    void Start()
    {
        isRunTime = true;
        selectObj = GetComponent<OutlineSelectObject>();
        buffer = new CommandBuffer();

        //material set
        mat = new Material(Shader.Find("Custom/OutlineTwoPass"));
        OnValidate();

        //set render loop timing
        cam = GetComponent<Camera>();
        var camEvent = CameraEvent.AfterForwardOpaque;
        cam.AddCommandBuffer(camEvent, buffer);
    }

    void OnPreRender()
    {
        if (!selectObj.obj)
        {
            buffer.Clear();
            target = null;
            return;
        }

        //Only draw once
        if (target != selectObj.obj.GetComponent<Renderer>())
        {
            target = selectObj.obj.GetComponent<Renderer>();
            buffer.DrawRenderer(target, mat, 0, 4); //only draw outline pass
        }

    }

    void OnValidate()
    {
        if (!isRunTime) return;
        mat.SetColor("_OutlineColor", OutlineColor);
        mat.SetFloat("_Outline", OutlineWidth);
        mat.SetFloat("_isNormalExtrude", isNormalExtrude ? 1 : 0);
    }

}
