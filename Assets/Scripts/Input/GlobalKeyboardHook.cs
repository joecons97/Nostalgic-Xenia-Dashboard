using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class GlobalKeyboardHook : MonoBehaviour
{
    #region Windows API
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    
    // Virtual key codes
    private const int VK_HOME = 0x24; // Home key
    #endregion
    
    [Header("Settings")]
    [SerializeField] private KeyCode guideKey = KeyCode.Home;
    
    private LowLevelKeyboardProc hookProc;
    private IntPtr hookID = IntPtr.Zero;
    private bool isHookActive = false;
    
    // Event that fires when the guide button is pressed
    public event Action OnGuideButtonPressed;
    
    void Start()
    {
        // Keep the delegate alive
        hookProc = HookCallback;
    }
    
    public void StartGlobalHook()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (isHookActive) return;
        
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            hookID = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, GetModuleHandle(curModule.ModuleName), 0);
        }
        
        if (hookID == IntPtr.Zero)
        {
            UnityEngine.Debug.LogError("Failed to set keyboard hook!");
        }
        else
        {
            isHookActive = true;
            UnityEngine.Debug.Log("Global keyboard hook activated");
        }
        #else
        UnityEngine.Debug.LogWarning("Global keyboard hook only works in Windows builds");
        #endif
    }
    
    public void StopGlobalHook()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (!isHookActive) return;
        
        if (hookID != IntPtr.Zero)
        {
            UnhookWindowsHookEx(hookID);
            hookID = IntPtr.Zero;
            isHookActive = false;
            UnityEngine.Debug.Log("Global keyboard hook deactivated");
        }
        #endif
    }
    
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            
            // Check if Home key was pressed
            if (vkCode == VK_HOME)
            {
                // Fire event on main thread
                UnityMainThreadDispatcher.Instance.Enqueue(() => 
                {
                    OnGuideButtonPressed?.Invoke();
                });
            }
        }
        
        return CallNextHookEx(hookID, nCode, wParam, lParam);
    }
    
    void OnDestroy()
    {
        StopGlobalHook();
    }
    
    void OnApplicationQuit()
    {
        StopGlobalHook();
    }
    
    // Fallback for editor testing
    void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(guideKey))
        {
            OnGuideButtonPressed?.Invoke();
        }
        #endif
    }
}

// Helper class to execute actions on the main Unity thread
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private readonly System.Collections.Generic.Queue<Action> _executionQueue = new System.Collections.Generic.Queue<Action>();
    
    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("MainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
    
    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue()?.Invoke();
            }
        }
    }
}