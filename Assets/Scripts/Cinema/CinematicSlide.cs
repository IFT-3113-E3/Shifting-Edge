// This script is part of the Cinematic system for the game.
// It defines a class that represents a single slide in the cinematic sequence. 
// Each slide consists of an image and a text description.
// The class is marked as serializable so that it can be used in Unity's inspector.
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[System.Serializable]
public class CinematicSlide {
    public Sprite image;
    [TextArea(3,10)] public string TMPro;
}
