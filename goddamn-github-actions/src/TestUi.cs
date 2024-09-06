using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace goddamn_github_actions;

public class TestUi
{
    private const int BorderWidth = 8;
    private const int BorderHeight = BorderWidth;
    
    private const int TotalWindowWidth = 600;
    private const int TotalWindowHeight = 510;
    
    private const int RefreshFramesPerSecondWhenUnfocused = 100;
    private const int RefreshEventPollPerSecondWhenMinimized = 15;
    
    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize;
    private const ImGuiWindowFlags WindowFlagsNoCollapse = WindowFlags | ImGuiWindowFlags.NoCollapse;

    private Sdl2Window _window;
    private GraphicsDevice _gd;
    private CommandList _cl;

    private CustomImGuiController _controller;
    
    private readonly Vector3 _clearColor = new(0.45f, 0.55f, 0.6f);

    public void Run()
    {
        // Create window, GraphicsDevice, and all resources necessary for the demo.
        var width = TotalWindowWidth;
        var height = TotalWindowHeight;
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, width, height, WindowState.Normal, $"{GGAApp.AppTitle}"),
            new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
            out _window,
            out _gd);
        _window.Resized += () =>
        {
            _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
            _controller.WindowResized(_window.Width, _window.Height);
        };
        _cl = _gd.ResourceFactory.CreateCommandList();
        _controller = new CustomImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width,
            _window.Height);

        var timer = Stopwatch.StartNew();
        timer.Start();
        var stopwatch = Stopwatch.StartNew();
        var deltaTime = 0f;
        // Main application loop
        while (_window.Exists)
        {
            if (!_window.Focused)
            {
                Thread.Sleep(1000 / RefreshFramesPerSecondWhenUnfocused);
            }
            // else: Do not limit framerate.
            
            while (_window.WindowState == WindowState.Minimized)
            {
                Thread.Sleep(1000 / RefreshEventPollPerSecondWhenMinimized);
                
                // TODO: We need to know when the window is no longer minimized.
                // How to properly poll events while minimized?
                _window.PumpEvents();
            }
            
            deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            stopwatch.Restart();
            var snapshot = _window.PumpEvents();
            if (!_window.Exists) break;
            _controller.Update(deltaTime,
                snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

            SubmitUI();

            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
            _controller.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }

        // Clean up Veldrid resources
        _gd.WaitForIdle();
        _controller.Dispose();
        _cl.Dispose();
        _gd.Dispose();
    }

    private void SubmitUI()
    {
        var windowHeight = _window.Height - BorderHeight * 2;
        ImGui.SetNextWindowPos(new Vector2(BorderWidth, BorderHeight), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(_window.Width - BorderWidth * 2, windowHeight), ImGuiCond.Always);

        var flags = WindowFlagsNoCollapse;
        ImGui.Begin($"{GGAApp.AppTitleTab} {VERSION.version}", flags);
        ImGui.BeginTabBar("##tabs");
        if (ImGui.BeginTabItem("Test"))
        {
            ImGui.Text("Hello World!");
        }

        ImGui.End();
    }
}