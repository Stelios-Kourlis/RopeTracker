using BepInEx.Configuration;
using BepInEx;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using HarmonyLib;
using System.Collections.Generic;

namespace MoveableUI;

public class MoveableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform = null!;
    private Canvas canvas = null!;
    private Vector2 dragOffset;
    public Action<Vector2> OnPositionChanged = null!;
    private static readonly HashSet<MoveableObject> instances = [];

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        Harmony _harmony = new("com.github.Stelios-Kourlis.MoveableUI");
        _harmony.PatchAll();
        instances.Add(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        rectTransform.localPosition = localPoint - dragOffset;
    }

    public void OnEndDrag(PointerEventData _)
    {
        OnPositionChanged?.Invoke(rectTransform.localPosition);
    }

    public static void PauseMenuEntered()
    {
        Canvas canvas = FindFirstObjectByType<GUIManager>().transform.Find("PauseMenu").GetComponent<Canvas>();
        foreach (MoveableObject instance in instances)
            instance.transform.SetParent(canvas.transform, false);
    }

    public static void PauseMenuExited()
    {
        FindFirstObjectByType<GUIManager>().StartCoroutine(ReparentNextFrame()); //Use GUIManager as coroutine runner

        static IEnumerator ReparentNextFrame()
        {
            yield return null; // Wait for the next frame
            Canvas canvas = FindFirstObjectByType<GUIManager>().transform.Find("Canvas_HUD").GetComponent<Canvas>();
            foreach (MoveableObject instance in instances)
                instance.transform.SetParent(canvas.transform, false);
        }
    }
}
