# Unity Mixed Reality Accessibility

This package provides Accessibility Nodes to the Android Accessibility Services, which are utilized by Quest OS Accessibility features. This package enables UGUI compatibility with Android's accessibility features, such as font scaling, text-to-speech, and screen readers.

## Requirements

This package is confirmed to support Unity 2022.2.0f1 or newer, older versions may work but are not tested.

## Installation

You can find the latest version of the package in Assets/UnityAccessibility.unitypackage
To import, download the unity package and import it into your project by going to Assets > Import Package > Custom Package > select the downloaded unity package.

## Usage

* Add the AccessibilityManager component to your scene.
* Add an AccessibilityNode component to the root canvas object(s) in your scene.
* (Optional), add an AccessibilityData component to any object you want to specific the Accessibility Label of.

See the [CONTRIBUTING](CONTRIBUTING.md) file for how to help out.

## License

Unity Mixed Reality Accessibility is Apache License 2.0 licensed, as found in the LICENSE file.
