using UnityEngine;

namespace UI
{
    public static class VideoSettingsHelper
    {
        public static void RefreshPixelPerfectRendering(GameCanvasScaler scaler)
        {
            if (scaler == null) return;
            
            // Force un recalcul via le mode d'affichage existant
            var currentMode = scaler.GetCurrentDisplayMode();
            scaler.SetDisplayMode(GameCanvasScaler.DisplayMode.Original);
            scaler.SetDisplayMode(currentMode);
        }

        public static GameCanvasScaler.DisplayMode GetCurrentDisplayMode(this GameCanvasScaler scaler)
        {
            // Solution alternative si vous ne pouvez pas accéder au champ
            return (GameCanvasScaler.DisplayMode)typeof(GameCanvasScaler)
                .GetField("displayMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(scaler);
        }
    }
}