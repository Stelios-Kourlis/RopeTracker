using HarmonyLib;
using Photon.Pun;
using RopeTracker;
using UnityEngine;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
class IslandEnterPatch
{
    static void Postfix()
    {
        Character? myCharacter = FindPlayerCharacter();
        if (myCharacter == null) return;
        if (myCharacter.inAirport)
        {
            Plugin.Log.LogInfo($"Character loaded in airport. Despawning UI.");
            Plugin.DestroyUI();
        }
        else
        {
            Plugin.Log.LogInfo($"Character loaded in island. Resetting total rope length. Spawning UI");
            Plugin.SpawnUI();
        }
    }

    private static Character? FindPlayerCharacter()
    {
        foreach (PhotonView view in Object.FindObjectsByType<PhotonView>(FindObjectsSortMode.None))
        {
            if (view.IsMine && view.TryGetComponent(out Character character))
            {
                Plugin.Log.LogInfo($"Found my character: {character.name}");
                return character;
            }
        }
        Plugin.Log.LogError("Could not find my character!");
        return null;
    }
}