using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

public class PtWrapper
{
    #region DLLIMPORTS
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

    [DllImport("PanoramaTrackerDLL", EntryPoint = "InitializeTracker", CallingConvention = CallingConvention.Cdecl)]
    private static extern void InitializeTracker(byte[] image, int height, int width);

    [DllImport("PanoramaTrackerDLL", EntryPoint = "CalculateOrientation", CallingConvention = CallingConvention.Cdecl)]
    private static extern void CalculateOrientation(byte[] image);

    [DllImport("PanoramaTrackerDLL", EntryPoint = "pluginTest", CallingConvention = CallingConvention.Cdecl)]
    private static extern int pluginTest();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "ViewMap", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ViewMap(int mapSize, bool viewKeypoints, bool viewCells);

    [DllImport("PanoramaTrackerDLL", EntryPoint = "SetRotationInvariance", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SetRotationInvariance(bool inv);

    [DllImport("PanoramaTrackerDLL", EntryPoint = "GetRotationX", CallingConvention = CallingConvention.Cdecl)]
    private static extern float GetRotationX();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "GetRotationY", CallingConvention = CallingConvention.Cdecl)]
    private static extern float GetRotationY();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "GetRotationZ", CallingConvention = CallingConvention.Cdecl)]
    private static extern float GetRotationZ();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "GetTrackingQuality", CallingConvention = CallingConvention.Cdecl)]
    private static extern float GetTrackingQuality();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "GetDebugString", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetDebugString();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "GetMapHeight", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetMapHeight();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "GetMapWidth", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetMapWidth();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "GetMapImage", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetMapImage();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "Reset", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Reset();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "SetCustomOrientation", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SetCustomOrientation(float x, float y, float z);

    [DllImport("PanoramaTrackerDLL", EntryPoint = "CalcOrientationTime", CallingConvention = CallingConvention.Cdecl)]
    private static extern float CalcOrientationTime();

    [DllImport("PanoramaTrackerDLL", EntryPoint = "Set", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Set(int setting, double value);

#endif
#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern void InitializeTracker(byte[] image, int height, int width, bool mirrored);
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern void CalculateOrientation(byte[] image);
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern int pluginTest();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern bool GetRotationInvariance();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern void SetRotationInvariance(bool inv);
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern float GetRotationX();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern float GetRotationY();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern float GetRotationZ();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern void ViewMap();    
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern IntPtr GetDebugString();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern int GetMapWidth();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern int GetMapHeight();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern float GetTrackingQuality();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern IntPtr GetMapImage();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern IntPtr GetMapImageCustomSize();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern void Reset();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern void SetCustomOrientation(float x, float y, float z);
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern float CalcOrientationTime();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern float Set(int setting, double value);
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern bool TestCamera();
    [DllImport("libPanoramaTrackerAndroid")]
    private static extern bool OpenCLAvailable();
#endif
    #endregion

    //SettingValue list of enums from PanoramaTracker project
    public enum SettingValue
    {
        PT_CAMERA_WIDTH, PT_CAMERA_HEIGHT, PT_CAMERA_FOV_HORIZONTAL, PT_CAMERA_FOV_VERTICAL,
        PT_SUPPORT_AREA_SIZE, PT_SUPPORT_AREA_SEARCH_SIZE_FULL, PT_SUPPORT_AREA_SEARCH_SIZE_HALF,
        PT_SUPPORT_AREA_SEARCH_SIZE_QUARTER, PT_FAST_KEYPOINT_THRESHOLD, PT_MAX_KEYPOINTS_PER_CELL,
        PT_USE_COLORED_MAP, PT_USE_ORB, PT_MIN_TRACKING_QUALITY, PT_MAX_DEVIATION, PT_MIN_RELOC_QUALITY,
        PT_CELLS_X, PT_CELLS_Y, PT_USE_ANDROID_SHIELD, PT_WARPER_SCALE, PT_PYRAMIDICAL, PT_ROTATION_INVARIANT,
        PT_MAX_DEV_FILTERING_FULL, PT_MAX_DEV_FILTERING_HALF, PT_MAX_DEV_FILTERING_QUARTER
    }

    private Texture2D _mapTexture;
    private bool _textureInit = false;
    public bool _reinitTexture = false;

    private static PtWrapper _instance;
    public static PtWrapper Instance {
        get
        {
            if (_instance == null) _instance = new PtWrapper();
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    //Private constructor, use only through Instance
    private PtWrapper()
    {

    }

    public bool TestCameraAndroid()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        return TestCamera();
#endif
        return false;
    }

    public void ReinitTexture()
    {
        _reinitTexture = true;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public bool HasOpenCL(){
        return OpenCLAvailable();
    }
#endif
    public Texture2D GetMapTexture()
    {
        //For colored map image:
        IntPtr ptr = GetMapImage();
        //The .dll returns a quarter sized map for better performance
        byte[] map = new byte[((GetMapWidth() / 4) * (GetMapHeight() / 4))*3];
        Marshal.Copy(ptr, map, 0, ((GetMapHeight() / 4) * (GetMapWidth() / 4))*3);
        Color32[] map_col = ColoredByteToC32(map);

        //Only init the texture once to avoid memory leaking
        if (!_textureInit || _reinitTexture)
        {
            _textureInit = true;
            _mapTexture = new Texture2D((GetMapWidth()/4), (GetMapHeight()/4));
            _reinitTexture = false;
        }
        _mapTexture.SetPixels32(map_col);
        _mapTexture.Apply();
        return _mapTexture;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public Texture2D GetMapTextureFull(){
        //For colored map image:
        IntPtr ptr = GetMapImageCustomSize();
        //The .dll returns a quarter sized map for better performance
        byte[] map = new byte[GetMapWidth() * GetMapHeight()*3];
        Marshal.Copy(ptr, map, 0, (GetMapWidth() * GetMapHeight()*3));
        Color32[] map_col = ColoredByteToC32(map);

        //Only init the texture once to avoid memory leaking
        if (!_textureInit || _reinitTexture)
        {
            _textureInit = true;
            _mapTexture = new Texture2D(GetMapWidth(), GetMapHeight());
            _reinitTexture = false;
        }
        _mapTexture.SetPixels32(map_col);
        _mapTexture.Apply();
        return _mapTexture;
    }
#endif

    public void InitializeTracking(Color32[] image, int imageWidth, int imageHeight)
    {
        byte[] img = C32ToByte(image);
        //Initialization on editor/pc
#if UNITY_EDITOR
        InitializeTracker(img, imageHeight, imageWidth);
#endif
        //On some devices, the image gets mirrored, so use different initialization call
#if UNITY_ANDROID && !UNITY_EDITOR
        if(!CameraControl.Instance.UsingAndroidPlugin()) InitializeTracker(img, imageHeight, imageWidth, false);
        else InitializeTracker(img, imageHeight, imageWidth, AndroidCamLib.instance().UsingJpeg());
#endif
    }

    public void ResetTracking()
    {
        Reset();
        GameObject.Find("Scripts").GetComponent<SettingsMenu>().ResetTracker();
    }

    public void Track(Color32[] image)
    {
        CalculateOrientation(C32ToByte(image));
    }

    public void Trackt()
    {
        CalculateOrientation(AndroidCamLib.instance().GetImageRgb());
    }

    //Return the orientation in a float[3] array
    public float[] GetOrientation()
    {
        float[] orientation = new float[3];
        orientation[0] = GetRotationX();
        orientation[1] = GetRotationY();
        orientation[2] = GetRotationZ();
        return orientation;
    }

    public int MapHeight()
    {
        return GetMapHeight();
    }

    public int MapWidth()
    {
        return GetMapWidth();
    }

    public string GetDebugInfo()
    {
        IntPtr ptr = GetDebugString();
        string info = Marshal.PtrToStringAnsi(ptr);
        return info;
    }

    public float GetQuality()
    {
        return GetTrackingQuality();
    }

    public void SetOrientation(float x, float y, float z)
    {
        SetCustomOrientation(x,y,z);
    }


    public float GetCalculationTime()
    {
        return CalcOrientationTime();
    }


    private static byte[] C32ToByte(Color32[] colors)
    {
        byte[] bytes = new byte[colors.Length * 3];
        for (int i = 0, j = 0;  i < bytes.Length; j++)
        {
            bytes[i++] = colors[j].b;
            bytes[i++] = colors[j].g;
            bytes[i++] = colors[j].r;
        }
        return bytes;
    }

    private static Color32[] GrayByteToC32(byte[] bytes)
    {
        Color32[] col = new Color32[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            col[i].b = bytes[i];
            col[i].g = bytes[i];
            col[i].r = bytes[i];
        }
        return col;
    }

    private static Color32[] ColoredByteToC32(byte[] bytes)
    {
        Color32[] col = new Color32[bytes.Length / 3];
        for (int i = 0, j = 0; i < bytes.Length; i = i + 3, j++)
        {
            col[j].b = bytes[i];
            col[j].g = bytes[i + 1];
            col[j].r = bytes[i + 2];
        }
        return col;
    }

    public void ShowMap()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ViewMap(1, true, true);
#endif
    }

    public void SetValue(SettingValue setting, double value)
    {
        Set((int)setting, value);
    }
}
