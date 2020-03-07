using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Harmony;
using ICities;
using NetworkSkins.GUI;
using NetworkSkins.Locale;
using NetworkSkins.Persistence;
using NetworkSkins.Skins;
using NetworkSkins.TranslationFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.Object;

namespace NetworkSkins
{
    public class NetworkSkinsMod : ILoadingExtension, IUserMod
    {
        private const string HarmonyId = "boformer.NetworkSkins";

        public string Name => "Network Skins 2 Beta";
        public string Description => Translation.Instance.GetTranslation(TranslationID.MOD_DESCRIPTION);
        
        private HarmonyInstance harmony;

        private NetworkSkinPanel panel;
        private GameObject skinControllerGameObject;
        private GameObject persistenceServiceGameObject;


        #region Lifecycle
        public static bool InGame => (ToolManager.instance.m_properties.m_mode == ItemClass.Availability.Game);

        public static UITextureAtlas defaultAtlas;

        public void OnEnabled()
        {
            NetworkSkinManager.Ensure();

            InstallHarmony();

            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                Install();
            }
        }

        public void OnCreated(ILoading loading) {}

        public void OnLevelLoaded(LoadMode mode)
        {
            NetworkSkinManager.instance.OnLevelLoaded();

            Install();
        }

        public void OnLevelUnloading()
        {
            Uninstall();

            NetworkSkinManager.instance.OnLevelUnloading();
        }

        public void OnReleased() {}

        public void OnDisabled()
        {
            Uninstall();

            UninstallHarmony();

            NetworkSkinManager.Uninstall();
        }
        #endregion

        #region Harmony
        private void InstallHarmony()
        {
            if (harmony == null)
            {
                Debug.Log("NetworkSkins Patching...");

#if DEBUG
                HarmonyInstance.DEBUG = true;
#endif
                //HarmonyInstance.SELF_PATCHING = false; // only for custom build
                harmony = HarmonyInstance.Create(HarmonyId);
                ManualInstall();
                //harmony.PatchAll(GetType().Assembly);
            }
        }

        class HarmonyManualPatchData
        {
            internal MethodBase target;
            internal HarmonyMethod prefix = null;
            internal HarmonyMethod postfix = null;
            internal HarmonyMethod transpiler = null;

            internal HarmonyManualPatchData(MethodBase targetMethod, string m="")
            {
                NS2HelpersExtensions.Assert(targetMethod != null, $"target is nul for: " + m);
                target = targetMethod;
            }

            internal void SetPrefix<T>() => prefix = GetMethod<T>("Prefix");
            internal void SetPostfix<T>() => postfix = GetMethod<T>("Postfix");
            internal void SetTranspiler<T>() => transpiler = GetMethod<T>("Transpiler");

            internal static HarmonyMethod GetMethod<T>(string name)
            {
                Type t = typeof(T);
                MethodInfo m = t.GetMethod(name);
                NS2HelpersExtensions.Assert(m != null, $"{t}.GetMethod({name}) returned null");
                HarmonyMethod ret = new HarmonyMethod(m);
                NS2HelpersExtensions.Assert(m != null, $"new HarmonyMethod({t}.GetMethod({name})) returned null");
                return ret;
            }
        }


        private void ManualInstall()
        {
            List<HarmonyManualPatchData> manualList = new List<HarmonyManualPatchData>();
            {
                var data = new HarmonyManualPatchData(typeof(global::MonorailTrackAI).GetMethod("GetNodeBuilding"));
                data.SetPrefix<Patches.MonorailTrackAI.MonorailTrackAiGetNodeBuildingPatch>();
                data.SetPostfix<Patches.MonorailTrackAI.MonorailTrackAiGetNodeBuildingPatch>();
                manualList.Add(data);
            }
            
            {
                var data = new HarmonyManualPatchData(Patches.NetTool.NetToolCreateNode0Patch.TargetMethod());
                //data.SetPrefix<Patches.NetTool.NetToolCreateNode0Patch>();
                //data.SetPostfix<Patches.NetTool.NetToolCreateNode0Patch>();
                data.SetTranspiler<Patches.NetTool.NetToolCreateNode0Patch>();
                manualList.Add(data);
            }

            foreach (var data in manualList)
            {
                Debug.Log($"patching " + data.target + " ...");
                harmony.Patch(
                    original: data.target,
                    prefix: data.prefix,
                    postfix: data.postfix,
                    transpiler: data.transpiler);
                Debug.Log($"Success!");
            }

        }

        private void UninstallHarmony()
        {
            if (harmony != null)
            {
                UnpatchAll();
                harmony = null;

                Debug.Log("NetworkSkins Reverted...");
            }
        }

        // Copy of Harmony.UnpatchAll() with undo order fixed.
        private void UnpatchAll()
        {
            bool IDCheck(Patch patchInfo) => patchInfo.owner == HarmonyId;

            var originals = harmony.GetPatchedMethods().ToList();
            foreach (var original in originals)
            {
                var info = harmony.GetPatchInfo(original);
                info.Postfixes.DoIf(IDCheck, patchInfo => harmony.Unpatch(original, patchInfo.patch));
                info.Prefixes.DoIf(IDCheck, patchInfo => harmony.Unpatch(original, patchInfo.patch));
                info.Transpilers.DoIf(IDCheck, patchInfo => harmony.Unpatch(original, patchInfo.patch));
                //info. Finalizers.DoIf(IDCheck, patchInfo => harmony.Unpatch(original, patchInfo.PatchMethod)); // for harmony 2.0.0.8
            }
        }
        #endregion

        #region NetToolMonitor/GUI
        private void Install()
        {
            // try to get InGame atlas
            defaultAtlas = InGame ? UIView.GetAView().defaultAtlas : UIView.library?.Get<OptionsMainPanel>("OptionsPanel")?.GetComponent<UIPanel>()?.atlas;
            persistenceServiceGameObject = new GameObject(nameof(PersistenceService));
            skinControllerGameObject = new GameObject(nameof(NetworkSkinPanelController));
            persistenceServiceGameObject.transform.parent = NetworkSkinManager.instance.gameObject.transform;
            skinControllerGameObject.transform.parent = NetworkSkinManager.instance.gameObject.transform;
            PersistenceService.Instance = persistenceServiceGameObject.AddComponent<PersistenceService>();
            NetworkSkinPanelController.Instance = skinControllerGameObject.AddComponent<NetworkSkinPanelController>();
            NetworkSkinPanelController.Instance.EventToolStateChanged += OnNetToolStateChanged;
        }

        private void Uninstall()
        {
            if (NetworkSkinPanelController.Instance != null)
            {
                NetworkSkinPanelController.Instance.EventToolStateChanged -= OnNetToolStateChanged;
                if (skinControllerGameObject != null)
                {
                    Destroy(skinControllerGameObject);
                    skinControllerGameObject = null;
                }
            }

            if (PersistenceService.Instance != null) {
                if (persistenceServiceGameObject != null) {
                    Destroy(persistenceServiceGameObject);
                    persistenceServiceGameObject = null;
                }
            }

            if (panel != null && panel.gameObject != null)
            {
                Destroy(panel.gameObject);
                panel = null;
            }

            defaultAtlas = null;
        }

        private void OnNetToolStateChanged(bool isToolEnabled)
        {
            if (isToolEnabled)
            {
                panel = UIView.GetAView().AddUIComponent(typeof(NetworkSkinPanel)) as NetworkSkinPanel;
            }
            else
            {
                if (panel.gameObject != null)
                {
                    Destroy(panel.gameObject);
                    panel = null;
                }
            }
        }
        #endregion

        public static Type ResolveSerializedType(string type)
        {
            var assemblyName = typeof(NetworkSkinsMod).Assembly.GetName();
            var fixedType = Regex.Replace(type, $@"{assemblyName.Name}, Version=\d+.\d+.\d+.\d+", $"{assemblyName.Name}, Version={assemblyName.Version}");
            return Type.GetType(fixedType);
        }
    }
}
