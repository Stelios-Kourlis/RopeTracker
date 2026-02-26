using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MoveableUI;
using PEAKLib.UI;
using PEAKLib.UI.Elements;
using Photon.Pun;
using UnityEngine;

namespace IsTheScoutmasterClose;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony _harmony = null!;
    private static PeakText _closestScoutDistance = null!;
    private static ConfigEntry<Vector2> uiPosition = null!;

    private void Awake()
    {
        Log = Logger;
        _harmony = new Harmony("com.github.Stelios-Kourlis.IsTheScoutmasterClose");
        _harmony.PatchAll();

        uiPosition = Config.Bind(
            "Is The Scoutmaster Close",
            "Text Position",
            new Vector2(0, 525),
            "Stored text position (RectTransform.localPosition)"
        );

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    public void Update()
    {
        if (PhotonNetwork.OfflineMode) return; //No other scouts present

        Character myCharacter = null!;
        List<Character> otherScouts = [];

        foreach (PhotonView view in FindObjectsByType<PhotonView>(FindObjectsSortMode.None))
        {
            if (view.TryGetComponent(out Character character))
            {
                if (view.IsMine) myCharacter = character;
                else otherScouts.Add(character);
            }
        }

        if (myCharacter == null)
        {
            Log.LogError("Could not find my character!");
            return;
        }

        if (false)//TODO check if dead
        {
            _closestScoutDistance.SetText($"Safe! (Dead :( )");
            _closestScoutDistance.SetColor(Color.green);
            return;
        }

        int myHeight = CharacterStats.UnitsToMeters(myCharacter.transform.position.y);

        if (myHeight > 1320)
        {
            _closestScoutDistance.SetText($"Safe! (Above 1320m)");
            _closestScoutDistance.SetColor(Color.green);
            return;
        }

        Character highestOtherScout = otherScouts.OrderByDescending(c => c.transform.position.y)
                                                 .FirstOrDefault();
        int highestOtherScoutHeight = CharacterStats.UnitsToMeters(highestOtherScout.transform.position.y);

        if (highestOtherScoutHeight > myHeight)
        {
            _closestScoutDistance.SetText($"Safe! ({highestOtherScoutHeight - myHeight}m below highest scout)");
            _closestScoutDistance.SetColor(Color.green);
            return;
        }

        if (otherScouts.Any(c => CharacterStats.UnitsToMeters(Vector3.Distance(c.transform.position, myCharacter.transform.position)) < 24))
        {
            _closestScoutDistance.SetText($"Safe! (within 24m of another scout)");
            _closestScoutDistance.SetColor(Color.green);
            return;
        }

        if (myHeight - highestOtherScoutHeight < 128)
        {
            _closestScoutDistance.SetText($"{myHeight - highestOtherScoutHeight}m above 2nd highest scout");
            _closestScoutDistance.SetColor(new Color(0.8742f, 0.8567f, 0.7615f, 1));
        }
        else
        {
            _closestScoutDistance.SetText($"{myHeight - highestOtherScoutHeight}m above 2nd highest scout");
            _closestScoutDistance.SetColor(Color.red);
        }
    }

    public static void SpawnUI()
    {
        _closestScoutDistance = MenuAPI.CreateText($"{_closestScoutDistance}m/100m", "RopeLengthCounterText")
                                .SetFontSize(24f)
                                .SetColor(new Color(0.8742f, 0.8567f, 0.7615f, 1));
        Canvas canvas = FindFirstObjectByType<GUIManager>().transform.Find("Canvas_HUD").GetComponent<Canvas>();
        _closestScoutDistance.transform.SetParent(canvas.transform, false);
        _closestScoutDistance.gameObject.AddComponent<MoveableObject>();
        RectTransform rect = _closestScoutDistance.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, 0);
        _closestScoutDistance.gameObject.GetComponent<MoveableObject>().OnPositionChanged += newPos =>
        {
            Log.LogInfo($"New position: {newPos}");
            uiPosition.Value = newPos;
        };
        rect.localPosition = uiPosition.Value;
    }

    public static void DestroyUI()
    {
        if (_closestScoutDistance != null) Destroy(_closestScoutDistance.gameObject);
    }
}
