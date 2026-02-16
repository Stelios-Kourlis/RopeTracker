using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using UnityEngine;

namespace RopeTracker;

[BepInDependency(UIPlugin.Id)]
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony _harmony = null!;
    private static float _totalRopeLengthInMeters = 0f;
    private static PeakText _ropeLengthText = null!;

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
        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    public static void SpawnUI()
    {
        _ropeLengthText = MenuAPI.CreateText($"{_totalRopeLengthInMeters}m/100m", "RopeLengthCounterText")
                                .SetFontSize(24f)
                                .SetColor(new Color(0.8742f, 0.8567f, 0.7615f, 1));
        Canvas canvas = FindFirstObjectByType<GUIManager>().transform.Find("Canvas_HUD").GetComponent<Canvas>();
        _ropeLengthText.transform.SetParent(canvas.transform, false);
        RectTransform rect = _ropeLengthText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        _ropeLengthText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        TotalRopeLengthInMeters = 0f;

        Transform parent = _ropeLengthText.transform;
        try
        {
            while (true)
            {
                Log.LogInfo($"Name: {parent.name}");
                parent = parent.parent;
            }
        }
        catch (System.Exception)
        {
            Log.LogError($"End of chain");
        }
    }

    public static void DestroyUI()
    {
        if (_ropeLengthText != null) Destroy(_ropeLengthText.gameObject);
    }
}
