using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using UnityEngine.XR;
using UnityEngine.XR.InteractionSubsystems;
using UnityEngine.XR.MagicLeap.Meshing;
using UnityEngine.XR.Management;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.MagicLeap.Remote;
using UnityEditor.XR.Management;
using UnityEngine.Rendering;
#endif //UNITY_EDITOR

#if UNITY_2020_1_OR_NEWER
using XRTextureLayout = UnityEngine.XR.XRDisplaySubsystem.TextureLayout;
#endif // UNITY_2020_1_OR_NEWER

namespace UnityEngine.XR.MagicLeap
{
    public sealed class MagicLeapLoader : XRLoaderHelper
    {
        static class Graphics
        {
#if PLATFORM_LUMIN
            const string k_Library = "ml_graphics";
            [DllImport(k_Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLGraphicsEnableBlobCacheGL")]
            static extern uint EnableBlobCacheGL(string path, uint max_entry_bytes, uint max_total_bytes);
#else
            static uint EnableBlobCacheGL(string path, uint max_entry_bytes, uint max_total_bytes) => 0;
#endif // PLATFORM_LUMIN

            internal static void SetupGLCache(MagicLeapSettings.GLCache cacheSettings)
            {
                var result = EnableBlobCacheGL(cacheSettings.cachePath, cacheSettings.maxBlobSizeInBytes, cacheSettings.maxFileSizeInBytes);
                MagicLeapLogger.AssertError(result == 0, kLogTag, $"Failed to initialize blob cache ({cacheSettings.cachePath} / {cacheSettings.maxBlobSizeInBytes} / {cacheSettings.maxFileSizeInBytes})");
            }
        }
        enum Privileges : uint
        {
            ControllerPose = 263,
            GesturesConfig = 269,
            GesturesSubscribe = 268,
            LowLatencyLightwear = 59,
            WorldReconstruction = 33
        }
        const string kLogTag = "MagicLeapLoader";
        static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
        static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();
        static List<XRMeshSubsystemDescriptor> s_MeshSubsystemDescriptor = new List<XRMeshSubsystemDescriptor>();
        static List<XRGestureSubsystemDescriptor> s_GestureSubsystemDescriptors = new List<XRGestureSubsystemDescriptor>();

        public XRDisplaySubsystem displaySubsystem => GetLoadedSubsystem<XRDisplaySubsystem>();
        public XRInputSubsystem inputSubsystem => GetLoadedSubsystem<XRInputSubsystem>();
        public XRMeshSubsystem meshSubsystem => GetLoadedSubsystem<XRMeshSubsystem>();
        public XRGestureSubsystem gestureSubsystem => GetLoadedSubsystem<XRGestureSubsystem>();

#if UNITY_EDITOR
        public static MagicLeapLoader assetInstance => (MagicLeapLoader)AssetDatabase.LoadAssetAtPath("Packages/com.unity.xr.magicleap/XR/Loaders/Magic Leap Loader.asset", typeof(MagicLeapLoader));
#endif // UNITY_EDITOR

        private bool m_DisplaySubsystemRunning = false;
        private bool m_GestureSubsystemRunning = false;
        private int m_MeshSubsystemRefcount = 0;

        public override bool Initialize()
        {
#if UNITY_EDITOR
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore)
            {
                Debug.LogWarning(
                    "To use Magic Leap Zero Iteration mode, the editor must be running under OpenGL.\n" +
                    " 1) Go to Edit -> Project Settings -> Player, and select the first tab there (Standalone Settings)...\n" +
                    " 2) Open the 'Other Settings' section, and uncheck 'Auto Graphics API for Windows' An API list will appear.\n" +
                    " 3) Use the + button to add 'OpenGLCore' to the API list.\n" +
                    " 4) Drag it to the top of the list. The editor will now switch to using OpenGL.\n");
            }

            if (!MagicLeapRemoteManager.Initialize())
                return false;
#endif // UNITY_EDITOR
            MagicLeapPrivileges.Initialize();

            ApplySettings();

            // Display Subsystem depends on Input Subsystem, so initialize that first.
            CheckForInputRelatedPermissions();
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "MagicLeap-Input");
            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "MagicLeap-Display");
            if (MagicLeapSettings.currentSettings != null && MagicLeapSettings.currentSettings.enableGestures)
                CreateSubsystem<XRGestureSubsystemDescriptor, XRGestureSubsystem>(s_GestureSubsystemDescriptors, "MagicLeap-Gesture");

            if (CanCreateMeshSubsystem())
            {
                CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(s_MeshSubsystemDescriptor, "MagicLeap-Mesh");
                if (meshSubsystem != null)
                {
                    MeshingSettings.meshingSettings = MLSpatialMapper.GetDefaultMeshingSettings();
                    MeshingSettings.batchSize = MLSpatialMapper.Defaults.batchSize;
                    MeshingSettings.density = MLSpatialMapper.Defaults.density;
                    MeshingSettings.SetBounds(Vector3.zero, Quaternion.identity, MLSpatialMapper.Defaults.boundsExtents);
                }
            }

            return true;
        }

        public override bool Start()
        {
            StartSubsystem<XRInputSubsystem>();
            if (MagicLeapSettings.currentSettings != null && MagicLeapSettings.currentSettings.enableGestures)
            {
                StartSubsystem<XRGestureSubsystem>();
                m_GestureSubsystemRunning = true;
            }

            if (!isLegacyDeviceActive)
            {
                var settings = MagicLeapSettings.currentSettings;
#if UNITY_2020_1_OR_NEWER
                if (settings != null && settings.forceMultipass)
                    displaySubsystem.textureLayout = XRTextureLayout.SeparateTexture2Ds;
                else
                    displaySubsystem.textureLayout = XRTextureLayout.Texture2DArray;
#else
                if (settings != null && settings.forceMultipass)
                    displaySubsystem.singlePassRenderingDisabled = true;
                else
                    displaySubsystem.singlePassRenderingDisabled = false;
#endif // UNITY_2020_1_OR_NEWER
                StartSubsystem<XRDisplaySubsystem>();
                m_DisplaySubsystemRunning = true;
            }
            return true;
        }

        public override bool Stop()
        {
            if (m_DisplaySubsystemRunning)
            {
                StopSubsystem<XRDisplaySubsystem>();
                m_DisplaySubsystemRunning = false;
            }
            if (m_GestureSubsystemRunning)
            {
                StopSubsystem<XRGestureSubsystem>();
                m_GestureSubsystemRunning = false;
            }
            if (CanCreateMeshSubsystem() && m_MeshSubsystemRefcount > 0)
            {
                m_MeshSubsystemRefcount = 0;
                StopSubsystem<XRMeshSubsystem>();
            }
            StopSubsystem<XRInputSubsystem>();
            return true;
        }

        public override bool Deinitialize()
        {
            if (CanCreateMeshSubsystem())
                DestroySubsystem<XRMeshSubsystem>();
            DestroySubsystem<XRDisplaySubsystem>();
            DestroySubsystem<XRGestureSubsystem>();
            DestroySubsystem<XRInputSubsystem>();
            MagicLeapPrivileges.Shutdown();
            return true;
        }

        internal static bool isLegacyDeviceActive
        {
            get { return XRSettings.enabled && (XRSettings.loadedDeviceName == "Lumin"); }
        }

        internal void StartMeshSubsystem()
        {
            if (!CanCreateMeshSubsystem())
                return;
            //MagicLeapLogger.Debug(kLogTag, "m_MeshSubsystemRefcount: {0}", m_MeshSubsystemRefcount);
            m_MeshSubsystemRefcount += 1;
            //MagicLeapLogger.Debug(kLogTag, "m_MeshSubsystemRefcount: {0}", m_MeshSubsystemRefcount);
            if (m_MeshSubsystemRefcount == 1)
            {
                MagicLeapLogger.Debug(kLogTag, "Starting Mesh Subsystem");
                StartSubsystem<XRMeshSubsystem>();
            }
        }

        internal void StopMeshSubsystem()
        {
            if (!CanCreateMeshSubsystem())
                return;
            //MagicLeapLogger.Debug(kLogTag, "m_MeshSubsystemRefcount: {0}", m_MeshSubsystemRefcount);
            if (m_MeshSubsystemRefcount == 0)
                return;

            m_MeshSubsystemRefcount -= 1;
            //MagicLeapLogger.Debug(kLogTag, "m_MeshSubsystemRefcount: {0}", m_MeshSubsystemRefcount);
            if (m_MeshSubsystemRefcount == 0)
            {
                MagicLeapLogger.Debug(kLogTag, "Stopping Mesh Subsystem");
                StopSubsystem<XRMeshSubsystem>();
            }
        }

        private void ApplySettings()
        {
            var settings = MagicLeapSettings.currentSettings;
            if (settings != null)
            {
                // set depth buffer precision
                MagicLeapLogger.Debug(kLogTag, $"Setting Depth Precision: {settings.depthPrecision}");
                Rendering.RenderingSettings.depthPrecision = settings.depthPrecision;
                // set frame timing hint
                MagicLeapLogger.Debug(kLogTag, $"Setting Frame Timing Hint: {settings.frameTimingHint}");
                Rendering.RenderingSettings.frameTimingHint = settings.frameTimingHint;

#if PLATFORM_LUMIN && !UNITY_EDITOR
                if (settings.glCacheSettings.enabled)
                {
                    Graphics.SetupGLCache(settings.glCacheSettings);
                }
#endif // PLATFORM_LUMIN && !UNITY_EDITOR
            }
        }

        [Conditional("DEVELOPMENT_BUILD")]
        private void CheckForInputRelatedPermissions()
        {
            if (!MagicLeapPrivileges.IsPrivilegeApproved((uint)Privileges.ControllerPose))
                Debug.LogWarning("No controller privileges specified; Controller data will not be available via XRInput Subsystem!");
            if (!(MagicLeapPrivileges.IsPrivilegeApproved((uint)Privileges.GesturesConfig) && MagicLeapPrivileges.IsPrivilegeApproved((uint)Privileges.GesturesSubscribe)))
                Debug.LogWarning("No gestures privileges specified; Gesture and Hand data will not be available via XRInput Subsystem!");
        }

        private bool CanCreateMeshSubsystem()
        {
            if (MagicLeapPrivileges.IsPrivilegeApproved((uint)Privileges.WorldReconstruction))
                return true;
#if DEVELOPMENT_BUILD
            Debug.LogError("Unable to create Mesh Subsystem due to missing 'WorldReconstruction' privilege. Please add to manifest");
#endif // DEVELOPMENT_BUILD
            return false;
        }
    }
#if UNITY_EDITOR
    internal static class XRMangementEditorExtensions
    {
        internal static bool IsEnabledForPlatform(this XRLoader loader, BuildTargetGroup group)
        {
            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(group);
            return settings?.Manager?.loaders?.Contains(loader) ?? false;
        }

        internal static bool IsEnabledForPlatform(this XRLoader loader, BuildTarget target)
        {
            return loader.IsEnabledForPlatform(BuildPipeline.GetBuildTargetGroup(target));
        }
    }
#endif // UNITY_EDITOR
}
