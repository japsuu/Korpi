using ImGuiNET;
using Korpi.Client.Debugging.Profiling;

namespace Korpi.Client.UI.Windows;

public class ProfilerWindow : ImGuiWindow
{
    private struct FrameTime
    {
        public DateTime Timestamp;
        public float FrameDurationMillis;


        public FrameTime(float frameDurationMillis)
        {
            Timestamp = DateTime.Now;
            FrameDurationMillis = frameDurationMillis;
        }
    }


    private readonly Queue<Profile> _frameProfiles = new(60);
    private readonly Queue<FrameTime> _frameTimes = new();
    private Profile? _selectedProfile;
    private bool _paused;
    private bool _wasAutoPaused;
    private float _maxFrameTime;
    private float _autoPauseMillisThreshold = 33.3f;
    private int _maxFrameProfilesCount = 60;
    private TimeSpan _avgFrameTimeBufferSeconds = TimeSpan.Zero;
    private System.Numerics.Vector2 _histogramSize = new(500, 200);
    
    public override string Title => "Profiler";


    public ProfilerWindow(bool autoRegister = true) : base(autoRegister)
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    protected override void DrawContent()
    {
        ImGui.Checkbox("Enabled", ref KorpiProfiler.IsProfilingEnabled);
        
        Profile? lastFrame = KorpiProfiler.GetLastFrame();
        if (lastFrame == null || !KorpiProfiler.IsProfilingEnabled)
            return;

        if (!_paused)
            UpdateData(lastFrame);

        DrawHistogram();

        // Draw the profile tree.
        if (_selectedProfile != null)
            DrawProfileInfo(_selectedProfile);

        DrawResumeButton();

        DrawExportButton();

        DrawSettings();
    }


    private void UpdateData(Profile lastFrame)
    {
        _frameProfiles.Enqueue(lastFrame);
        if (_frameProfiles.Count > _maxFrameProfilesCount)
            _frameProfiles.Dequeue();
        _selectedProfile = lastFrame;
        
        _frameTimes.Enqueue(new FrameTime((float)lastFrame.DurationMillis));
        if (_avgFrameTimeBufferSeconds.Seconds < 1)
        {
            if (_frameTimes.Count > _maxFrameProfilesCount)
                _frameTimes.Dequeue();
        }
        else
        {
            while (_frameTimes.Peek().Timestamp < DateTime.Now - _avgFrameTimeBufferSeconds)
            {
                _frameTimes.Dequeue();
            }
        }
            
        // Calculate the maximum frame time.
        _maxFrameTime = _frameTimes.Max(t => t.FrameDurationMillis);
        
        if (GameTime.TotalTime < 5)
            return;
        
        // Auto-pause if the frame time exceeds the threshold.
        int sampleIndex = _maxFrameProfilesCount / 2;
        if (_frameProfiles.Count <= sampleIndex)
            return;
        
        Profile profile = _frameProfiles.ElementAt(sampleIndex);
        if (_autoPauseMillisThreshold <= 0 || profile.DurationMillis < _autoPauseMillisThreshold)
            return;
        
        _paused = true;
        _wasAutoPaused = true;
        
        // Select the profile that has the highest frame time.
        _selectedProfile = _frameProfiles.Aggregate((p1, p2) => p1.DurationMillis > p2.DurationMillis ? p1 : p2);
    }


    private void DrawHistogram()
    {
        // Convert the durations of the frame profiles to an array of floats.
        float[] durations = _frameProfiles.Select(p => (float)p.DurationMillis).ToArray();

        // Draw a histogram of the frame profile durations.
        if (durations.Length <= 0)
            return;
        
        ImGui.Text("Frame times (ms)");
        string overlayText;
        if (_paused)
            overlayText = _wasAutoPaused ? "auto-paused" : "paused";
        else
            overlayText = "";
        ImGui.PlotHistogram("", ref durations[0], durations.Length, 0, overlayText, 0.0f, _maxFrameTime, _histogramSize);
            
        // Show a tooltip when hovering over the histogram.
        if (ImGui.IsItemHovered())
        {
            System.Numerics.Vector2 mousePos = ImGui.GetMousePos();
            System.Numerics.Vector2 itemMin = ImGui.GetItemRectMin();
            System.Numerics.Vector2 itemMax = ImGui.GetItemRectMax();

            int hoveredIndex = (int)((mousePos.X - itemMin.X) / (itemMax.X - itemMin.X) * durations.Length);
            hoveredIndex = Math.Clamp(hoveredIndex, 0, durations.Length - 1);

            Profile hoveredProfile = _frameProfiles.ElementAt(hoveredIndex);
            ImGui.SetTooltip($"Frame {hoveredIndex + 1}\nDuration: {hoveredProfile.DurationMillis:F4} ms");
        }

        // Select a frame profile when clicking on the histogram.
        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            System.Numerics.Vector2 mousePos = ImGui.GetMousePos();
            System.Numerics.Vector2 itemMin = ImGui.GetItemRectMin();
            System.Numerics.Vector2 itemMax = ImGui.GetItemRectMax();

            int clickedIndex = (int)((mousePos.X - itemMin.X) / (itemMax.X - itemMin.X) * durations.Length);
            clickedIndex = Math.Clamp(clickedIndex, 0, durations.Length - 1);

            _selectedProfile = _frameProfiles.ElementAt(clickedIndex);
            _paused = true;
        }
    }


    private static void DrawProfileInfo(Profile profile)
    {
        // Calculate the color based on the duration of the profile.
        float t = Math.Clamp((float)profile.DurationMillis / 33.3f, 0.0f, 1.0f);
        System.Numerics.Vector4 color = System.Numerics.Vector4.Lerp(
            new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f),  // Green
            new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f),  // Red
            t
        );

        // Draw the profile name and duration with the calculated color.
        ImGui.TextColored(color, profile.Name);
        ImGui.SameLine();
        ImGui.TextColored(color, $"{profile.DurationMillis:F4} ms");

        if (profile.Children.Count <= 0)
            return;

        ImGui.Indent();
        foreach (Profile child in profile.Children)
            DrawProfileInfo(child);
        ImGui.Unindent();
    }


    private void DrawResumeButton()
    {
        bool isResumePressed = _paused && ImGui.Button("Resume");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Resumes the profiling session.");
            ImGui.EndTooltip();
        }

        if (_wasAutoPaused)
        {
            // Draw button to disable auto-pause.
            ImGui.SameLine();
            bool isDisableAutoPausePressed = ImGui.Button("Disable Auto-Pause");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Disables auto-pause.");
                ImGui.EndTooltip();
            }
            
            if (isDisableAutoPausePressed)
            {
                isResumePressed = true;
                _autoPauseMillisThreshold = 0;
            }
        }

        if (!isResumePressed)
            return;
        
        _paused = false;
        _wasAutoPaused = false;
        _frameProfiles.Clear();
        _frameTimes.Clear();
    }


    private void DrawExportButton()
    {
        ImGui.Separator();
        bool exportPressed = ImGui.Button("Export");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Exports the currently selected profile to a text file.");
            ImGui.EndTooltip();
        }
        if (exportPressed)
        {
            const string filePath = "profiler.txt";
            string directory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            using StreamWriter writer = new(filePath);
            ExportProfileData(_selectedProfile, writer);
            Logger.Info($"Exported profiler data to {directory}");
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", directory);
            }
            catch (Exception)
            {
                // Ignored, because windows specific
            }
        }
    }


    private void DrawSettings()
    {
        if (ImGui.Button("Open Settings"))
        {
            ImGui.OpenPopup("Settings");
        }

        bool isOpen = true;
        if (ImGui.BeginPopupModal("Settings", ref isOpen))
        {
            ImGui.Text("Settings");
            ImGui.Separator();
            ImGui.BeginGroup();

            ImGui.Checkbox("Pause", ref _paused);
            
            ImGui.Text("Histogram Width");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.SliderFloat("##settings_histogram_width", ref _histogramSize.X, 100, 2000);

            ImGui.Text("Histogram Height");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.SliderFloat("##settings_histogram_height", ref _histogramSize.Y, 50, 1000);

            ImGui.Text("Profile buffer size");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.DragInt("##settings_fb_size", ref _maxFrameProfilesCount, 1, 1, 200))
            {
                _frameProfiles.Clear();
                _frameTimes.Clear();
            }

            ImGui.Text("Avg frame time buffer (s)");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            float currentAvgFrameTimeBufferSeconds = (float)_avgFrameTimeBufferSeconds.TotalSeconds;
            if (ImGui.DragFloat("##settings_aft_size", ref currentAvgFrameTimeBufferSeconds, 0.5f, 0, 10))
            {
                _avgFrameTimeBufferSeconds = TimeSpan.FromSeconds(currentAvgFrameTimeBufferSeconds);
                _frameProfiles.Clear();
                _frameTimes.Clear();
            }

            ImGui.Text("Auto-pause threshold (ms)");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.DragFloat("##settings_auto_pause_threshold", ref _autoPauseMillisThreshold, 0.5f, 0, 100);

            ImGui.EndGroup();

            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
    
    
    private static void ExportProfileData(Profile? profile, StreamWriter writer, int indentLevel = 0)
    {
        if (profile == null)
            return;

        string indent = new('\t', indentLevel);
        writer.WriteLine($"{indent}{profile.Name}: {profile.DurationMillis:F4} ms");

        foreach (Profile child in profile.Children)
            ExportProfileData(child, writer, indentLevel + 1);
    }
}
