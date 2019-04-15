﻿using ColossalFramework.UI;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace NetworkSkins
{
    public class Resources
    {
        public static Resources Atlas { get; private set; } = new Resources();
        public static string DragHandle =       "DragHandle";
        public static string Star =             "Star";
        public static string StarOutline =      "StarOutline";
        public static string Locked =           "Locked";
        public static string LockedPressed =    "LockedPressed";
        public static string LockedHovered =    "LockedHovered";
        public static string Unlocked =         "Unlocked";
        public static string UnlockedPressed =  "UnlockedPressed";
        public static string UnlockedHovered =  "UnlockedHovered";
        public static string Tree =             "Tree";
        public static string TreePressed =      "TreePressed";
        public static string TreeHovered =      "TreeHovered";
        public static string TreeFocused =      "TreeFocused";
        public static string Light =            "Light";
        public static string LightPressed =     "LightPressed";
        public static string LightHovered =     "LightHovered";
        public static string LightFocused =     "LightFocused";
        public static string Pillar =           "Pillar";
        public static string PillarPressed =    "PillarPressed";
        public static string PillarHovered =    "PillarHovered";
        public static string PillarFocused =    "PillarFocused";
        public static string Catenary =         "Catenary";
        public static string CatenaryPressed =  "CatenaryPressed";
        public static string CatenaryHovered =  "CatenaryHovered";
        public static string CatenaryFocused =  "CatenaryFocused";
        public static string Color =            "Color";
        public static string ColorPressed =     "ColorPressed";
        public static string ColorHovered =     "ColorHovered";
        public static string ColorFocused =     "ColorFocused";
        public static string Surface =          "Surface";
        public static string SurfacePressed =   "SurfacePressed";
        public static string SurfaceHovered =   "SurfaceHovered";
        public static string SurfaceFocused =   "SurfaceFocused";
        public static string Eyedropper =       "Eyedropper";
        public static string EyedropperPressed ="EyedropperPressed";
        public static string EyedropperHovered ="EyedropperHovered";
        public static string EyedropperFocused ="EyedropperFocused";
        public static string Settings =         "Settings";
        public static string SettingsPressed =  "SettingsPressed";
        public static string SettingsHovered =  "SettingsHovered";
        public static string SettingsFocused =  "SettingsFocused";
        public static string Swatch = "Swatch";

        private UITextureAtlas UITextureAtlas { get; set; }

        private static readonly string[] spriteNames = new string[] {
            "DragHandle",
            "DragHandle",
            "Star",
            "StarOutline",
            "Locked",
            "LockedPressed",
            "LockedHovered",
            "Unlocked",
            "UnlockedPressed",
            "UnlockedHovered",
            "Tree",
            "TreePressed",
            "TreeHovered",
            "TreeFocused",
            "Light",
            "LightPressed",
            "LightHovered",
            "LightFocused",
            "Pillar",
            "PillarPressed",
            "PillarHovered",
            "PillarFocused",
            "Catenary",
            "CatenaryPressed",
            "CatenaryHovered",
            "CatenaryFocused",
            "Color",
            "ColorPressed",
            "ColorHovered",
            "ColorFocused",
            "Surface",
            "SurfacePressed",
            "SurfaceHovered",
            "SurfaceFocused",
            "Eyedropper",
            "EyedropperPressed",
            "EyedropperHovered",
            "EyedropperFocused",
            "Settings",
            "SettingsPressed",
            "SettingsHovered",
            "SettingsFocused",
            "Swatch"
    };

        public Resources() {
            CreateAtlas();
        }

        public static implicit operator UITextureAtlas(Resources atlas) {
            return atlas.UITextureAtlas;
        }

        private void CreateAtlas() {
            UITextureAtlas textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();

            Texture2D[] textures = new Texture2D[spriteNames.Length];
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);

            for (int i = 0; i < spriteNames.Length; i++)
                textures[i] = GetTextureFromAssemblyManifest(spriteNames[i] + ".png");

            int maxSize = 1024;
            Rect[] regions = new Rect[spriteNames.Length];
            regions = texture2D.PackTextures(textures, 2, maxSize);

            Material material = Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            material.mainTexture = texture2D;
            textureAtlas.material = material;
            textureAtlas.name = "NetworkSkinsAtlas";

            for (int i = 0; i < spriteNames.Length; i++) {
                UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo {
                    name = spriteNames[i],
                    texture = textures[i],
                    region = regions[i],
                };

                textureAtlas.AddSprite(item);
            }

            UITextureAtlas = textureAtlas;
        }

        private Texture2D GetTextureFromAssemblyManifest(string file) {
            string path = string.Concat(GetType().Namespace, ".Resources.", file);
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path)) {
                byte[] array = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(array, 0, array.Length);
                texture2D.LoadImage(array);
            }
            texture2D.Apply();
            return texture2D;
        }
    }
}
