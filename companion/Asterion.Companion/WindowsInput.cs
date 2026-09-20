using System.Diagnostics;
using System.Runtime.InteropServices;
using Asterion.Core;
namespace Asterion.Companion;

public static class WindowsInput
{
    [StructLayout(LayoutKind.Sequential)] struct Input { public uint type; public Union data; }
    [StructLayout(LayoutKind.Explicit)] struct Union { [FieldOffset(0)] public Keyboard ki; [FieldOffset(0)] public Mouse mi; }
    [StructLayout(LayoutKind.Sequential)] struct Keyboard { public ushort vk, scan; public uint flags,time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Sequential)] struct Mouse { public int dx,dy; public uint data,flags,time; public UIntPtr extra; }
    [DllImport("user32.dll",SetLastError=true)] static extern uint SendInput(uint count,Input[] inputs,int size);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint pid);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr window);
    public static string ForegroundName()
    {
        GetWindowThreadProcessId(GetForegroundWindow(),out uint pid);
        try { using var p=Process.GetProcessById((int)pid); return p.ProcessName; } catch { return ""; }
    }
    public static async Task Send(KeyChord chord,int ms,bool restore)
    {
        string fg=ForegroundName();
        if(restore && (fg.Equals("iCUE",StringComparison.OrdinalIgnoreCase)||fg.Equals("QmlRenderer",StringComparison.OrdinalIgnoreCase)))
        {
            var processes=Process.GetProcessesByName("StarCitizen");
            try { if(processes.Length==1) SetForegroundWindow(processes[0].MainWindowHandle); }
            finally { foreach(var p in processes)p.Dispose(); }
            await Task.Delay(70);
        }
        if(!ForegroundName().Equals("StarCitizen",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Star Citizen must be in the foreground");
        List<Input> down=[],up=[];
        foreach(var scan in chord.ScanCodes)
        {
            uint extended=(scan&0x100)!=0?1u:0u;
            down.Add(new(){type=1,data=new(){ki=new(){scan=(ushort)(scan&0xff),flags=8|extended}}});
            up.Insert(0,new(){type=1,data=new(){ki=new(){scan=(ushort)(scan&0xff),flags=8|extended|2}}});
        }
        if(chord.MouseButton>0)
        {
            down.Add(new(){type=0,data=new(){mi=new(){data=(uint)chord.MouseButton,flags=0x80}}});
            up.Add(new(){type=0,data=new(){mi=new(){data=(uint)chord.MouseButton,flags=0x100}}});
        }
        try
        {
            if(SendInput((uint)down.Count,down.ToArray(),Marshal.SizeOf<Input>())!=down.Count) throw new InvalidOperationException("Windows refused input (focus or privilege level)");
            // Interrupt long presses when focus changes; always release all keys in finally.
            long end=Environment.TickCount64+Math.Clamp(ms,30,1500);
            while(Environment.TickCount64<end && ForegroundName().Equals("StarCitizen",StringComparison.OrdinalIgnoreCase)) await Task.Delay(15);
        }
        finally { SendInput((uint)up.Count,up.ToArray(),Marshal.SizeOf<Input>()); }
    }
}
