using MelonLoader;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using HarmonyLib;

namespace NeonBoostLogoMod
{
    public class RunnerLogoMod : MelonMod
    {
        public static MelonPreferences_Category Settings;
        public static MelonPreferences_Entry<float> LogoScaleFactor;
        public static MelonPreferences_Entry<float> LogoSpacing;
        public static MelonPreferences_Entry<float> LogoBorderSize;
        public static MelonPreferences_Entry<bool> ShowLogo;
        public static MelonPreferences_Entry<bool> UseSteamAvatar;
        public static MelonPreferences_Entry<string> BorderColor;
        public static MelonPreferences_Entry<float> LogoOpacity;
        public static MelonPreferences_Entry<RunnerLogo.LogoPositions> LogoPosition;

        private static int tryCreateRetries;

        public override void OnInitializeMelon()
        {
            Settings = MelonPreferences.CreateCategory("RunnerLogoMod", "Runner Logo");
            LogoScaleFactor = Settings.CreateEntry<float>("LogoScaleFactor", 1.0f, "Logo Scale Factor", null, false, false, null, null);
            LogoSpacing = Settings.CreateEntry<float>("LogoSpacing", 0.02f, "Logo Spacing", null, false, false, null, null);
            LogoBorderSize = Settings.CreateEntry<float>("LogoBorderSize", 0.05f, "Logo Border Size", null, false, false, null, null);
            ShowLogo = Settings.CreateEntry<bool>("ShowLogo", false, "Show Logo", null, false, false, null, null);
            UseSteamAvatar = Settings.CreateEntry<bool>("UseSteamAvatar", true, "Use Steam Avatar", null, false, false, null, null);
            BorderColor = Settings.CreateEntry<string>("BorderColor", "#FFFFFF", "Border Color", null, false, false, null, null);
            LogoOpacity = Settings.CreateEntry<float>("LogoOpacity", 1.0f, "Logo Opacity", null, false, false, null, null);
            LogoPosition = Settings.CreateEntry<RunnerLogo.LogoPositions>("LogoPosition", RunnerLogo.LogoPositions.TopRight, "Logo Position", null, false, false, null, null);

            var harmony = new HarmonyLib.Harmony("RunnerLogo");
            harmony.PatchAll(typeof(RunnerLogoMod).Assembly);
            RunnerLogoModResources.LoadResources();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex < 1)
                return;

            tryCreateRetries = 0;
            TryCreate();
        }

        private static void TryCreate()
        {
            if (RunnerLogo.EnsureCreated())
                return;

            if (tryCreateRetries++ < 30)
                MelonCoroutines.Start(TryAgainCoroutine());
        }

        private static IEnumerator TryAgainCoroutine()
        {
            yield return null;
            TryCreate();
        }
    }

    public static class RunnerLogoModResources
    {
        public static Sprite logoSprite;
        public static Sprite steamAvatarSprite;
        public static int steamAvatarRetries = 0;

        public static void LoadResources()
        {
            // load logo texture from the mod directory and create a sprite from it
            if (logoSprite == null)
            {
                try
                {
                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(typeof(RunnerLogoMod).Assembly.Location), "RunnerLogo.png")))
                    {
                        MelonLogger.Error("RunnerLogo.png not found in the mod directory.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error loading RunnerLogo.png: {ex.Message}");
                    logoSprite = null;
                    return;
                }

                byte[] logoData = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(typeof(RunnerLogoMod).Assembly.Location), "RunnerLogo.png"));
                Texture2D logoTexture = new Texture2D(2, 2);
                logoTexture.LoadImage(logoData);
                logoSprite = Sprite.Create(logoTexture, new Rect(0, 0, logoTexture.width, logoTexture.height), new Vector2(0.5f, 0.5f));
            }
        }

        public static void LoadSteamAvatar(Texture2D steamAvatarTexture)
        {
            // create steam avatar sprite from texture
            if (steamAvatarSprite == null)
            {
                try
                {
                    steamAvatarSprite = Sprite.Create(steamAvatarTexture, new Rect(0, 0, steamAvatarTexture.width, steamAvatarTexture.height), new Vector2(0.5f, 0.5f));
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error loading SteamAvatar: {ex.Message}");
                    steamAvatarSprite = null;
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(MenuManager), "Update")]
    public class MenuManagerUpdatePatch
    {
        public static void Postfix(MenuManager __instance)
        {
            // grab steam avatar from the menu if not already loaded, and retry a few times if it fails
            if (RunnerLogoModResources.steamAvatarSprite == null && RunnerLogoModResources.steamAvatarRetries < 5)
            {
                GameObject steamAvatarObject = GameObject.Find("Steam_Avatar");
                if (steamAvatarObject != null && steamAvatarObject.GetComponent<RawImage>() != null)
                    RunnerLogoModResources.LoadSteamAvatar(steamAvatarObject.GetComponent<RawImage>().texture as Texture2D);
                RunnerLogoModResources.steamAvatarRetries++;
            }
        }
    }

    public class RunnerLogo : MonoBehaviour
    {
        // make a border around the logo with 4 rectangles (default no sprite for an image is a white square)
        public static GameObject[] MakeBorder(float thickness, Vector2 size, Color color, Transform parent)
        {
            float borderMultiplier = 2 * thickness + 1.0f;
            string borderName = "RunnerLogoBorder";
            GameObject[] borderParts = new GameObject[4] {
                new GameObject(borderName + "Bottom"),
                new GameObject(borderName + "Top"),
                new GameObject(borderName + "Left"),
                new GameObject(borderName + "Right")
            };
            Vector2[] borderAnchors = new Vector2[4]
            {
                new Vector2(0.5f, 0f), // Bottom
                new Vector2(0.5f, 1f), // Top
                new Vector2(0f, 0.5f), // Left
                new Vector2(1f, 0.5f)  // Right
            };
            Vector2[] borderSizes = new Vector2[4]
            {
                new Vector2(borderMultiplier, thickness), // Bottom
                new Vector2(borderMultiplier, thickness), // Top
                new Vector2(thickness, 1f), // Left
                new Vector2(thickness, 1f)  // Right
            };

            for (int i = 0; i < borderParts.Length; i++)
            {
                GameObject part = borderParts[i];

                part.transform.SetParent(parent, false);

                part.AddComponent<Image>();
                part.GetComponent<Image>().color = color;

                part.AddComponent<RectTransform>();
                part.GetComponent<RectTransform>().pivot = part.GetComponent<RectTransform>().anchorMin = part.GetComponent<RectTransform>().anchorMax = borderAnchors[i];
                part.GetComponent<RectTransform>().sizeDelta = size * borderSizes[i];
            }

            return borderParts;
        }
        public static bool EnsureCreated()
        {
            if (GameObject.Find("RunnerLogo") != null || RunnerLogoMod.ShowLogo.Value == false || RunnerLogoModResources.logoSprite == null)
                return true;

            // get config values
            float scaleFactor = RunnerLogoMod.LogoScaleFactor.Value;
            float spacing = RunnerLogoMod.LogoSpacing.Value;
            float borderMultiplier = 2 * RunnerLogoMod.LogoBorderSize.Value + 1.0f;
            float borderSize = RunnerLogoMod.LogoBorderSize.Value;
            float logoOpacity = RunnerLogoMod.LogoOpacity.Value;
            float maxpos = 1.0f - spacing;
            float minpos = spacing;
            Color borderColor = ColorUtility.TryParseHtmlString(RunnerLogoMod.BorderColor.Value, out Color pc) ? new Color(pc.r, pc.g, pc.b, logoOpacity) : new Color(1f, 1f, 1f, logoOpacity);
            Sprite chosenSprite = RunnerLogoMod.UseSteamAvatar.Value ? RunnerLogoModResources.steamAvatarSprite : RunnerLogoModResources.logoSprite;

            // make logo and parent
            GameObject runnerlogoParent = new GameObject("RunnerLogoParent");
            GameObject runnerLogo = new GameObject("RunnerLogo");
            RectTransform runnerlogoParentRect = runnerlogoParent.AddComponent<RectTransform>();
            RectTransform runnerLogoRect = runnerLogo.AddComponent<RectTransform>();
            Image runnerLogoImage = runnerLogo.AddComponent<Image>();

            // configure logo and parent
            runnerlogoParent.transform.SetParent(GameObject.Find("Canvas").transform, false);
            runnerlogoParentRect.sizeDelta *= (scaleFactor * borderMultiplier);
            runnerLogo.transform.SetParent(runnerlogoParent.transform, false);
            runnerLogoRect.sizeDelta *= scaleFactor;
            runnerLogoRect.pivot = runnerLogoRect.anchorMin = runnerLogoRect.anchorMax = new Vector2(0.5f, 0.5f);
            runnerLogoImage.overrideSprite = chosenSprite;
            runnerLogoImage.color = new Color(1f, 1f, 1f, logoOpacity);
            MakeBorder(borderSize, runnerLogoRect.sizeDelta, borderColor, runnerlogoParent.transform);

            switch (RunnerLogoMod.LogoPosition.Value)
            {
                case LogoPositions.TopRight:
                    runnerlogoParentRect.anchorMin = runnerlogoParentRect.anchorMax = new Vector2(maxpos, maxpos);
                    runnerlogoParentRect.pivot = new Vector2(1.0f, 1.0f);
                    break;
                case LogoPositions.TopLeft:
                    runnerlogoParentRect.anchorMin = runnerlogoParentRect.anchorMax = new Vector2(minpos, maxpos);
                    runnerlogoParentRect.pivot = new Vector2(0.0f, 1.0f);
                    break;
                case LogoPositions.BottomRight:
                    runnerlogoParentRect.anchorMin = runnerlogoParentRect.anchorMax = new Vector2(maxpos, minpos);
                    runnerlogoParentRect.pivot = new Vector2(1.0f, 0.0f);
                    break;
                case LogoPositions.BottomLeft:
                    runnerlogoParentRect.anchorMin = runnerlogoParentRect.anchorMax = new Vector2(minpos, minpos);
                    runnerlogoParentRect.pivot = new Vector2(0.0f, 0.0f);
                    break;
            }

            // add itself to the gameover canvas aswell
            GameObject duplicateRunnerLogo = Instantiate(runnerlogoParent, GameObject.Find("_Canvas").transform.GetChild(1));
            for (int i = 0; i < duplicateRunnerLogo.transform.childCount; i++)
            {
                GameObject child = duplicateRunnerLogo.transform.GetChild(i).gameObject;
                if (child.name == "RunnerLogo")
                    child.GetComponent<Image>().overrideSprite = chosenSprite; // for some reason the sprite doesn't copy over when duplicating the parent, so we have to set it again
            }

            return true;
        }

        public enum LogoPositions
        {
            TopRight,
            TopLeft,
            BottomRight,
            BottomLeft
        }
    }
}
