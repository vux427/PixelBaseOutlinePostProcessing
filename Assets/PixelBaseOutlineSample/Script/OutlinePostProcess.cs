using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutlinePostProcess : MonoBehaviour {
    static private OutlinePostProcess instance;
    static public OutlinePostProcess Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(OutlinePostProcess)) as OutlinePostProcess;

                if (!instance)
                    instance = Camera.main.gameObject.AddComponent<OutlinePostProcess>();
                var c = (Component)instance;
                instance.Init();
            }
            return instance;
        }
        private set
        {
            if (value == null)
                Destroy(instance);
            instance = value;
        }
    }
    
    public bool enable,pixelBase,occluder,alphaDepth;
    Camera postProcessCam, maskCam;

    [SerializeField]
    private Color outlineColor = new Color(1,.2f,0,1);
    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            postMat.SetColor("_OutlineColor", value);
        }
    }
    [Range(1,8)]
    public int resolutionReduce = 1;


    //if need ingore some layer,just edit this list.
    public string[] ignoreLayerName = new string[] {
        "Outline"
        , "Water"
        , "TransparentFX"
        ,"UI"
    };
    int[] ignoreLayerIndex;


    [SerializeField, Header("Debug")]
    private RenderTexture maskTexture;
    [SerializeField]
    private RenderTexture tempRT1, tempRT2;
    private Material postMat,flatColor,grabDepth;
    [SerializeField]
    private RawImage mask, temp1, temp2;

    void OnValidate()
    {
        if (!isRuntime) return;
        OutlineColor = outlineColor;
    }
    void Start()
    {
        Init();
    }

    bool isRuntime;
    void Init()
    {
        if (Instance != this) Destroy(this);
        if (isRuntime) return;
        isRuntime = true;

        //set ignore layer
        ignoreLayerIndex = new int[ignoreLayerName.Length];
        for (int i = 0; i < ignoreLayerName.Length; i++)
        {
            ignoreLayerIndex[i] = (1 << LayerMask.NameToLayer(ignoreLayerName[i]));
        }

        postProcessCam = Camera.main;
        postMat = new Material(Shader.Find("Hide/OutlinePostprocess"));
        flatColor = new Material(Shader.Find("Hide/FlatColor"));
        grabDepth = new Material(Shader.Find("Hide/GrabDepth"));

        //set up outline camera
        maskCam = new GameObject().AddComponent<Camera>();
        maskCam.transform.SetParent(postProcessCam.transform);
        maskCam.gameObject.name = "OutlineRenderCamera";
        maskCam.enabled = false;
        maskCam.CopyFrom(postProcessCam);
        maskCam.clearFlags = CameraClearFlags.Nothing;
        maskCam.backgroundColor = Color.black;
        maskCam.renderingPath = RenderingPath.Forward;
        maskCam.cullingMask = 1 << LayerMask.NameToLayer("Outline");
        maskCam.allowHDR = false;

        maskTexture = RenderTexture.GetTemporary(Screen.width / resolutionReduce, Screen.height / resolutionReduce, 16, RenderTextureFormat.RGB565);
        maskCam.targetTexture = maskTexture;

        tempRT1 = RenderTexture.GetTemporary(Screen.width/ resolutionReduce, Screen.height/ resolutionReduce, 0, RenderTextureFormat.R8);
        tempRT2 = RenderTexture.GetTemporary(Screen.width/ resolutionReduce, Screen.height/ resolutionReduce, 0, RenderTextureFormat.R8);

        AttachToRawImage();
        OnValidate();
    }

    void CopyCameraSetting(Camera form,Camera to) {
        to.fieldOfView = form.fieldOfView;
        to.nearClipPlane = form.nearClipPlane;
        to.farClipPlane = form.farClipPlane;
        to.rect = form.rect;
    }

    void AttachToRawImage() {
        if (!mask || !temp1 ||!temp2 ) return;

        mask.texture = maskTexture;
        temp1.texture = tempRT1;
        temp2.texture = tempRT2;
    }


    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        OnValidate();
        Graphics.Blit(source, destination);

        if (!enable) return;
        //GL Clear to reset temporary texture content.
        Graphics.SetRenderTarget(tempRT1);
        GL.Clear(true, true, Color.black);
        Graphics.SetRenderTarget(tempRT2);
        GL.Clear(true, true, Color.black);

        //this way to clear camera target temporary texture.  
        maskCam.clearFlags = CameraClearFlags.Color;
        maskCam.backgroundColor = Color.black;

        CopyCameraSetting(postProcessCam,maskCam);

        //render mask map
        if (occluder)
        {
            int total = 0;
            System.Array.ForEach(ignoreLayerIndex, t => total += t);
            maskCam.cullingMask = ~(total);
            maskCam.RenderWithShader(grabDepth.shader,alphaDepth ? "" : "RenderType");
            maskCam.clearFlags = CameraClearFlags.Nothing;
        }

        maskCam.cullingMask = 1 << LayerMask.NameToLayer("Outline");
        if (pixelBase)
        {
            maskCam.RenderWithShader(null, "");
            Graphics.Blit(maskTexture, tempRT1, flatColor, 0);
            Graphics.Blit(tempRT1, tempRT2, flatColor, 1);
            Graphics.Blit(tempRT2, destination, postMat);
        }
        else {
            maskCam.RenderWithShader(flatColor.shader, "RenderType");
            Graphics.Blit(maskTexture, destination, postMat);
        }

    }
}