Thank you for purchasing SensorToolkit 2!

The documentation is available online at: http://www.micosmo.com/sensortoolkit2
There are some example scenes inside Examples/ directory. I recommend first viewing the 'Fundamentals' scene.

If you have any questions, feature requests or if you have found a bug then please send me an email at micosmogames@gmail.com


[CHANGELOG]
2.5.12 - Fix CollectionModifiedException when a detection event handler causes the sensor to update its list of detections.
2.5.11 - Fix 'Specific GameObject' issue in new versions of playmaker. Remove deprecation warnings in Unity 2023. 'GetDetectedComponents' allocates a list of storeIn is supplied null.
2.5.10 - Fixes an issue on the NavMeshSensor where area ids are not all consecutive.
2.5.9 - SteeringSensor has configurable Up-Direction in circular mode. 'Test' button stays visible after testing sensor in editor. Tag filter runs before signal processors. Tweak some default gizmo colours.
2.5.8 - Update GC2 integration
2.5.7 - Fix for UserSignals displaying incorrectly in the inspector.
2.5.6 - Fix for error when changing pulse interval in response to a detection event.
2.5.5 - Fix GC2 integration for latest version
2.5.4 - Fix for LOS generating invalid points in rare cases when running Quality mode.
2.5.3 - Fix NRE when pulse mode is set before it has initialized.
2.5.2 - Sensor UpdateFunction exposed via C#. Boolean sensor supports XOR. Sensors link to their doc pages from inspector.
2.5.1 - Fix error when Examples directory removed. Renamed Examples/resources to Examples/assets so its not included in builds.
2.5.0 - Big update to SteeringSensor which adds Velocity Obstacles support (Breaking changes). Most sensors will take a list of SignalProcessors. Laid a foundation to jobify the sensors, currently only the steering sensor is jobified. Many small performance fixes and improvements.
2.4.7 - RaySensor ignores intersections at distance 0 for consistency between [...]Cast and [...]CastNonAlloc. Some small performance improvements. Functions that order detections by signal strength.
2.4.6 - Bugfix for Playmaker action SensorGetDetections.
2.4.5 - LOS test points generated with Sobol sequences to reduce variance.
2.4.4 - Increased minimum Unity version to 2020.3.
2.4.3 - Better list rendering in component editors.
2.4.2 - Behaviour Designer Actions converted to Conditionals so they support 'Conditional Abort'. Fixed some cases where duplicate Sensor.OnChange events might occur.
2.4.1 - Add some helper functions so visual scripting tools can dynamically set the shapes of the ray and range sensors.
2.4.0 - **Attention** delete old SensorToolkit folder before upgrading. Adds integration package for AdventureCreator. Adds assembly definitions and reorganises scripts. Some internal refactoring to support future work.
2.3.7 - Quickfix, prev update would have compilation error in projects using Odin.
2.3.6 - Bring in latest changes to Observables. No functional difference.
2.3.5 - Fixes an issue with the TriggerSensor when it uses multiple colliders for its sensing volume
2.3.4 - Added a new component 'SignalProxy' for cases where you want to detect objects composed of many Rigidbodies. Such as a ragdoll character.
2.3.3 - Remove compiler warnings
2.3.2 - New LOSColliderOwner component can instruct LOSSensor which colliders to ignore when testing an object for LOS
2.3.1 - Replaced FOVCollider.BaseSize with 'NearDistance'. Made widget colours configurable. Fix a bug where RaySensor.Clear() won't reset 'isObstructed'.
2.3.0 - Added integration for Game Creator 2
2.2.11 - Removed shaders causing compilation errors for some users
2.2.10 - Sensor has new events 'OnSomeDetection' and 'OnNoDetection'
2.2.9 - Sensor can be Cleared. Sensor.PulseAll will also pulse any input sensors.
2.2.8 - Simplified and improved the built-in locomotion used by the Steering Sensor.
2.2.7 - Small fix to previous update.
2.2.6 - PlayMaker actions will take either a GameObject owner or specific Sensor to target.
2.2.5 -  New 'Sensor' type in PlayMaker variable-type dropdown. PlayMaker actions all take 'BasePulsableSensor'.
2.2.4 - Small improvement to Observable<T> class.
2.2.3 - Small tweaks to filtering functions. Slightly improved performance.
2.2.2 - LOSSensor has new prop 'PointGenerationMethod', which can be 'fast' or 'quality'. The 'quality' mode will restrict the test points to the fov, 'fast' will not.
2.2.1 - Small fix for LOSSensor. It should detect a signal when it's inside its bounds.
2.2.0 - Big improvements to LOSSensor. It will now generate test points within its defined angle constraints.
2.1.3 - Sensor pulses can now optionally be run in FixedUpdate. Fixed issue causing sensor pulses not to be staggered.
2.1.2 - Small bugfix. Signal.Bounds no longer throws NRE when Signal.Object is null.
2.1.1 - Small bugfixes for Playmaker actions.
2.1.0 - Added integration for Behavior Designer.
2.0.3 - Bugfix for LOSSensor so it will generate proper test points on rotated objects.
2.0.2 - Removed a List.AddRange which had slipped through and caused GC.
2.0.1 - No functional changes. Improved formatting and comments of all the sensors code.