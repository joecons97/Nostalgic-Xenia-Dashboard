using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparentOverlayWindow : MonoBehaviour
{
    #region Windows API
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    
    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
    
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
    
    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    
    // Constants
    private const int GWL_EXSTYLE = -20;
    private const int GWL_STYLE = -16;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;
    #endregion
    
    [Header("Settings")]
    [SerializeField] private bool enableOnStart = false; // Changed to false - we'll enable manually when launching a game
    [SerializeField] private Camera overlayCamera;
    
    private IntPtr windowHandle;
    private bool isTransparent = false;
    
    void Start()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (enableOnStart)
        {
            SetupTransparentWindow();
        }
        #endif
        
        // Set camera to render only UI with transparent background
        if (overlayCamera == null)
        {
            overlayCamera = Camera.main;
        }
        
        if (overlayCamera != null)
        {
            overlayCamera.clearFlags = CameraClearFlags.SolidColor;
            overlayCamera.backgroundColor = new Color(0, 0, 0, 0);
            Debug.Log($"Camera background set to transparent: {overlayCamera.backgroundColor}");
        }
        else
        {
            Debug.LogError("No overlay camera found! Transparency won't work properly.");
        }
    }
    
    public void SetupTransparentWindow()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        windowHandle = GetActiveWindow();
        Debug.Log($"Window handle: {windowHandle}");
        
        // Set window to be frameless
        SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        Debug.Log("Set window style to popup");
        
        // Make window layered (required for transparency)
        uint exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
        Debug.Log($"Current extended style: {exStyle}");
        SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
        Debug.Log("Set layered window attribute");
        
        // Extend frame to make the window transparent
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        uint result = DwmExtendFrameIntoClientArea(windowHandle, ref margins);
        Debug.Log($"DwmExtendFrameIntoClientArea result: {result} (0 = success)");
        
        // Set window to topmost
        SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        Debug.Log("Set window to topmost");
        
        isTransparent = true;
        Debug.Log("Transparent overlay window initialized");
        #else
        Debug.LogWarning("SetupTransparentWindow only works in Windows builds");
        #endif
    }
    
    public void EnableClickthrough()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (windowHandle == IntPtr.Zero)
            windowHandle = GetActiveWindow();
            
        uint exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
        SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
        Debug.Log("Clickthrough enabled");
        #endif
    }
    
    public void DisableClickthrough()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (windowHandle == IntPtr.Zero)
            windowHandle = GetActiveWindow();
            
        uint exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
        SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);
        Debug.Log("Clickthrough disabled");
        #endif
    }
    
    void OnApplicationQuit()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // Restore normal window style
        if (windowHandle != IntPtr.Zero)
        {
            uint exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
            SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle & ~WS_EX_LAYERED & ~WS_EX_TRANSPARENT);
        }
        #endif
    }
}