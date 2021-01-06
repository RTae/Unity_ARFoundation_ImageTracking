# Changelog

## [4.1.3] - 2020-04-07
- Conditionally compile out XR Management related classes that depend on XR Management `3.2.x`
- Revert dependency on XR Management `3.0.6`
- Add conditional code to support XR Management `3.2.X` workflows
- Conditionally compile samples to support lack of Legacy Input Helpers dependency.

## [4.1.2] - 2020-04-06
- Upgrade XR Management dependency to version `3.2.4`

## [4.1.1] - 2020-03-17
- Upgrade AR Subsystems dependency to version `2.1.2`

## [4.1.0] - 2020-03-13
- Update to non-preview release
- Upgrade XR Management dependency to non-preview version 3.2.0

## [4.1.0-preview.1] - 2020-02-26
- Upgrade XR Management dependency to the 3.2.0 series

## [4.0.6] - 2020-02-20
- Update to 2019.3 Verified Release

## [4.0.6-preview.5] - 2020-02-06
- Fix an issue with a malformed meta file causing an instability on CI
- Update LICENSE.md to year 2020
- Fixed Hand Tracking so that the underlying tracker doesn't need to be recreated on configuration changes

## [4.0.6-preview.4] - 2020-01-07
- Fix an issue causing CI not to trigger automatically
- Properly handle a couple edge cases where the Lumin SDK is missing

## [4.0.6-preview.3] - 2020-01-07
- Bump XR Management dependency to 3.0.5
- Remove old test assets that were conflicting with the Windows MR XR Plugin

## [4.0.6-preview.2] - 2019-12-13
- Correctly report whether or not we should render the gameview

## [4.0.6-preview.1] - 2019-11-22
- Fix some some bugs around the Display subsystem reinitializing
- Re-add a warning message when using ML Remote with an invalid graphics API
- Bump XR Management to 3.0.4
- Fix some issues with the MagicLeap Manifest Editor UI

## [4.0.5] - 2019-09-27
- Update to 2019.3 Verified Release
- Update NPM ignore list to accomdate new repo layout

## [4.0.4] - 2019-09-27
- Update to 2019.3 Verified Release

## [4.0.4-preview.1] - 2019-09-27
- Fix an issue on OSX that caused ML Remote to intermittenly fail to initialize
- Improve the Input Provider Samples with some callbacks for handling button presses
- Add some additional checks around meshing shutdown
- Update dependencies on AR Subsystems, Interaction Subsystems, and XR Management

## [4.0.3] - 2019-08-21
- Update to 2019.3 Verified Release

## [4.0.3-preview.2] - 2019-08-20
- Source tests from the XR SDK Tests repo via a submodule
- Add some missing meta files back to the test directories
- Update package dependencies to verified version
- Fix an issue where the ML Remote check was wrong on OSX

## [4.0.3-preview.1] - 2019-08-13
- Update the Manifest editor to work with new trunk changes
- Fix Issue 1174014: Play in Editor in PC mode with Magic Leap loader and AR gestures will crash Unity Editor
- Add an explicit binary check when looking to launch ML Remote
- Allow for loading of gesture subsystem via XR Management

## [4.0.2] - 2019-08-01
- Update to 2019.3 Verified Release

## [4.0.2-preview.1] - 2019-07-31
- Make package name consistent with other XR provider plugins
- Add an additional define constraint for the rendering samples tests so they're not built by default
- Support both LIH 1.x and 2.x BasePoseProvider APIs
- Don't scale the near clipping plane
- Use updated clipping extents API function
- Add support for setting meshing density

## [4.0.1] - 2019-07-29
- Update to 2019.3 Verified Release

## [4.0.1-preview.1] - 2019-07-29
- Bump LIH to 1.3.4, which is the actual verified version in 2019.3
- Remove custom Input usages from the Experimental namespace
- Bump ARSubsystems to 2.1.0, which is the actual verified version in 2019.3
- Update XRDisplaySubstem to no longer use the Experimental namespace
- Update release field to 0a12, to indicate new required version

## [4.0.0] - 2019-07-17
- Initial 2019.3 Verified Release

## [4.0.0-preview.3] - 2019-07-16
- Update package description with note about disabled legacy XR
- Update XR Display provider header to latest from trunk
- Update Lumin SDK to 0.21.0
- Require Unity 2019.3.0a10 to ensure compatibility with XR Display headers
- Set the default frame timing hint to 60Hz
- Move StabilizationComponent into a Rendering sample

## [4.0.0-preview.2] - 2019-07-12
- Add some sample Base Pose Provider implementations based on XR Input
- Update XR Display provider header to latest from trunk
- Update XR Management to version 3

## [4.0.0-preview.1] - 2019-07-07
- Upgrade XR SDK to require Unity 2019.3
- Update package name
- Fix some issues preventing CI from completing successfully
- Update to latest version of XR Management package
- Update to latest version of Interaction Subsystems package

## [3.0.0-preview.7] - 2019-06-27
- Update documentation for 2019.2
- Robustify Meshing

## [3.0.0-preview.6] - 2019-06-20
- Fix an issue with meshing causing settings values to use garbage data
- Add support for determining the origin controller of a touchpas gesture event
- Fix a couple issues around proper handling of multiple controllers
- Properly support standalone subsystems that depend on the perception system
- Add support for standalone Planes, Raycast, and ReferencePoint subsystems
- Update Gestures documentation
- Fix a type collision with MagicLeap's Unity framework
- Add initial support for custom MagicLeap settings when using XR SDK
- Fix a couple issues that arise when using XR SDK, ML Remote, and repeatedly going in and out of playmode
- Bump Legacy Input Helpers to 1.3.2
- Fix an issue where timeouts from the ML Graphics API would cause the XR Display subsystem to shutdown
- Add support for multipass rendering on Lumin hardware and on ML Remote on Windows
- Fix an issue where XRSettings.renderViewportScale wasn't being propagated to ML's Graphics API

## [3.0.0-preview.5] - 2019-06-11
- Fix the native controller api loader to properly reference `ml_perception_client` instead of `ml_input`
- Fix an issue that prevented the Display provider from properly initializing in Editor using ML Remote
- Disable some old testing menu items
- Fix a couple cases where the UnityMagicLeap plugin would crash because it couldn't load the ML Remote libraries
- Add Multipass support for ML Remote on OSX
- Fix a bug where ML Remote / Zero Iteration on device would silently fail when using the XR SDK implementation
- Add some native support for managing controller feedback

## [3.0.0-preview.4] - 2019-05-20
- Update yamato configuration
- Improve how various ML input devices are handled via XR Input
- Simplify ML Remote library loading in the native plugin

## [3.0.0-preview.3] - 2019-05-18
- Update third party notices

## [3.0.0-preview.2] - 2019-05-17

## [3.0.0-preview.1] - 2019-05-17
- Add support for Unity 2019.2
- Add support for XR Display Subsystem
- Remove disabled clipping plane enforcement toggles
- Add support for hand tracking
- Add Manifest Editor UI
- Update package to build against 0.20.0 MLSDK
- Add support for starting / stopping ML Remote server headlessly via the Unity TestRunner
- Add standalone Gestures subsystem
- Do not fail when requesting confidence for a zero-vertex mesh
- Don't generate colliders for point cloud style meshes

## [2.0.0-preview.14] - 2019-03-05
- Initial Production release
- Fix a number of issues causing instabilty when using ML Remote
