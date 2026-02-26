using HarmonyLib;

namespace MoveableUI;

[HarmonyPatch(typeof(PauseMenuHandler), nameof(PauseMenuHandler.OnEnable))]
class PauseMenuEnterPatch
{
    static void Postfix()
    {
        MoveableObject.PauseMenuEntered();
    }
}

[HarmonyPatch(typeof(PauseMenuHandler), nameof(PauseMenuHandler.OnDisable))]
class PauseMenuExitPatch
{
    static void Prefix()
    {
        MoveableObject.PauseMenuExited();
    }
}