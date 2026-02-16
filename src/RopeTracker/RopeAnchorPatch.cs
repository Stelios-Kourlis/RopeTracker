using HarmonyLib;
using RopeTracker;

[HarmonyPatch(typeof(Rope), nameof(Rope.AttachToAnchor_Rpc))]
class RopeAnchorPatch
{
    static void Postfix(float ropeLength)
    {
        Plugin.Log.LogInfo($"Rope.AttachToAnchor_Rpc postfix called with ropeLength: {ropeLength} units/{Rope.GetLengthInMeters(ropeLength)}m");
        Plugin.TotalRopeLengthInMeters += Rope.GetLengthInMeters(ropeLength);
    }
}