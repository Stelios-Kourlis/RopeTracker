using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using UnityEngine;
using MoveableUI;
using RunTracker;

namespace RopeTracker;

[BepInDependency(UIPlugin.Id)]
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony _harmony = null!;
    private static float _totalRopeLengthInMeters = 0f;
    private static PeakText _ropeLengthText = null!;
    private static ConfigEntry<Vector2> uiPosition = null!;

    public static float TotalRopeLengthInMeters
    {
        set
        {
            _totalRopeLengthInMeters = value;
            if (_ropeLengthText != null)
            {
                _ropeLengthText.SetText($"{_totalRopeLengthInMeters:F2}m/100m");
                if (_totalRopeLengthInMeters > 100f)
                    _ropeLengthText.SetColor(Color.green);
            }
            Log.LogInfo($"Total rope length in meters (SS) updated: {_totalRopeLengthInMeters}");
        }
        get => _totalRopeLengthInMeters;
    }

    private void Awake()
    {
        Log = Logger;
        _harmony = new Harmony("com.github.Stelios-Kourlis.RopeTracker");
        _harmony.PatchAll();

        uiPosition = Config.Bind(
            "Rope Tracker",
            "Text Position",
            new Vector2(0, 525),
            "Stored text position (RectTransform.localPosition)"
        );

        RunTracker.Plugin.isInAirport += DestroyUI;
        RunTracker.Plugin.isInIsland += SpawnUI;

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    public static void SpawnUI()
    {
        if (_ropeLengthText != null) return; //Already spawned
        _ropeLengthText = MenuAPI.CreateText($"{_totalRopeLengthInMeters}m/100m", "RopeLengthCounterText")
                                .SetFontSize(24f)
                                .SetColor(new Color(0.8742f, 0.8567f, 0.7615f, 1));
        Canvas canvas = FindFirstObjectByType<GUIManager>().transform.Find("Canvas_HUD").GetComponent<Canvas>();
        _ropeLengthText.transform.SetParent(canvas.transform, false);
        _ropeLengthText.gameObject.AddComponent<MoveableObject>();
        RectTransform rect = _ropeLengthText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, 0);
        TotalRopeLengthInMeters = 0f;
        _ropeLengthText.gameObject.GetComponent<MoveableObject>().OnPositionChanged += newPos =>
        {
            Log.LogInfo($"New position: {newPos}");
            uiPosition.Value = newPos;
        };
        rect.localPosition = uiPosition.Value;
    }

    public static void DestroyUI()
    {
        if (_ropeLengthText != null) Destroy(_ropeLengthText.gameObject);
        _ropeLengthText = null!;
    }
}
