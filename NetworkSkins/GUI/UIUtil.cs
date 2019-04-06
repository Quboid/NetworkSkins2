﻿using ColossalFramework.UI;
using UnityEngine;

namespace NetworkSkins.GUI
{
    public class UIUtil
    {
        public static ItemType PanelToItemType(PanelType panelType) {
            switch (panelType) {
                case PanelType.Trees: return ItemType.Trees;
                case PanelType.Lights: return ItemType.Lights;
                case PanelType.Surfaces: return ItemType.Surfaces;
                case PanelType.Pillars: return ItemType.Pillars;
                case PanelType.Color: return ItemType.Colors;
                case PanelType.Catenary: return ItemType.Catenary;
                default: return ItemType.None;
            }
        }

        public static UIButton CreateButton(Vector2 size, string text = "", string tooltip = "", string foregroundSprite = "", string backgroundSprite = "ButtonSmall", bool isFocusable = false, UITextureAtlas atlas = null, UIComponent parentComponent = null) {
            UIButton button = parentComponent.AddUIComponent<UIButton>();
            button.size = size;
            button.text = text;
            button.tooltip = tooltip;
            button.textPadding = new RectOffset(0, 0, 3, 0);
            button.normalBgSprite = backgroundSprite;
            button.hoveredBgSprite = string.Concat(backgroundSprite, "Hovered");
            button.pressedBgSprite = string.Concat(backgroundSprite, "Pressed");
            button.focusedBgSprite = string.Concat(backgroundSprite, isFocusable ? "Focused" : "");
            button.normalFgSprite = foregroundSprite;
            button.hoveredFgSprite = string.Concat(foregroundSprite, "Hovered");
            button.pressedFgSprite = string.Concat(foregroundSprite, "Pressed");
            button.focusedFgSprite = string.Concat(foregroundSprite, isFocusable ? "Focused" : "");
            if (atlas != null) button.atlas = atlas;

            return button;
        }
    }
}