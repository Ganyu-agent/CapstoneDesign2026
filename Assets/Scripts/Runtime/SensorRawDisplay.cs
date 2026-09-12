using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CapstoneDesign.Runtime
{
    /// <summary>
    /// Displays raw values from the Android SensorManager. Android tablets do
    /// not expose the same sensors as phones, so every requested sensor starts
    /// as null and remains null when the hardware is absent.
    /// </summary>
    public sealed class SensorRawDisplay : MonoBehaviour
    {
        private const float RefreshSeconds = 0.1f;

        [SerializeField] public Text output;

        private readonly Dictionary<string, string> values = new Dictionary<string, string>();
        private readonly StringBuilder builder = new StringBuilder(512);
        private float nextRefresh;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject sensorManager;
        private AndroidSensorListener listener;
#endif

        private static readonly SensorSlot[] RequestedSensors =
        {
            new SensorSlot("gyro", 4),         // TYPE_GYROSCOPE
            new SensorSlot("pressure", 6),     // TYPE_PRESSURE
            new SensorSlot("temp", 13),        // TYPE_AMBIENT_TEMPERATURE
            new SensorSlot("light", 5),        // TYPE_LIGHT
            new SensorSlot("accel", 1),        // TYPE_ACCELEROMETER
            new SensorSlot("magnetic", 2),     // TYPE_MAGNETIC_FIELD
            new SensorSlot("rotation", 11),    // TYPE_ROTATION_VECTOR
            new SensorSlot("proximity", 8),    // TYPE_PROXIMITY
            new SensorSlot("humidity", 12)     // TYPE_RELATIVE_HUMIDITY
        };

        private void Awake()
        {
            for (int i = 0; i < RequestedSensors.Length; i++)
            {
                values[RequestedSensors[i].name] = "null";
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            RegisterAndroidSensors();
#endif

            RefreshText();
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + RefreshSeconds;
                RefreshText();
            }
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (sensorManager != null && listener != null)
            {
                sensorManager.Call("unregisterListener", listener);
            }

            listener = null;
            sensorManager = null;
#endif
        }

        private void RefreshText()
        {
            if (output == null)
            {
                return;
            }

            builder.Length = 0;
            builder.Append("SENSORS (raw)\n");
            for (int i = 0; i < RequestedSensors.Length; i++)
            {
                SensorSlot slot = RequestedSensors[i];
                builder.Append(slot.name);
                builder.Append(": ");
                builder.Append(values.TryGetValue(slot.name, out string value) ? value : "null");
                builder.Append('\n');
            }

#if UNITY_EDITOR || !UNITY_ANDROID
            builder.Append("editor/mock: true");
#else
            builder.Append("android/api: ");
            builder.Append(GetAndroidApiLevel());
#endif
            output.text = builder.ToString();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void RegisterAndroidSensors()
        {
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    sensorManager = activity.Call<AndroidJavaObject>("getSystemService", "sensor");
                }

                if (sensorManager == null)
                {
                    return;
                }

                listener = new AndroidSensorListener(SetSensorValue);
                for (int i = 0; i < RequestedSensors.Length; i++)
                {
                    SensorSlot slot = RequestedSensors[i];
                    using (AndroidJavaObject sensor = sensorManager.Call<AndroidJavaObject>("getDefaultSensor", slot.type))
                    {
                        if (sensor != null)
                        {
                            // SENSOR_DELAY_NORMAL = 3. The A7's Android 10
                            // bridge dispatches proxy callbacks on its UI
                            // thread, so game-rate sampling can starve input.
                            // Raw smoke-test telemetry only needs a few Hz.
                            sensorManager.Call<bool>("registerListener", listener, sensor, 3);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Sensor registration failed: " + exception.Message);
            }
        }

        private void SetSensorValue(int type, float[] rawValues)
        {
            string key = GetSensorKey(type);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            values[key] = FormatValues(rawValues);
        }

        private static string GetSensorKey(int type)
        {
            for (int i = 0; i < RequestedSensors.Length; i++)
            {
                if (RequestedSensors[i].type == type)
                {
                    return RequestedSensors[i].name;
                }
            }

            return string.Empty;
        }

        private static int GetAndroidApiLevel()
        {
            using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }

        private static string FormatValues(float[] rawValues)
        {
            if (rawValues == null || rawValues.Length == 0)
            {
                return "null";
            }

            StringBuilder result = new StringBuilder(rawValues.Length * 12 + 2);
            result.Append('[');
            for (int i = 0; i < rawValues.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(", ");
                }

                result.Append(rawValues[i].ToString("0.#####", CultureInfo.InvariantCulture));
            }

            result.Append(']');
            return result.ToString();
        }

        private sealed class AndroidSensorListener : AndroidJavaProxy
        {
            private readonly Action<int, float[]> onChanged;
            private bool didLogReadFailure;

            public AndroidSensorListener(Action<int, float[]> onChanged)
                : base("android.hardware.SensorEventListener")
            {
                this.onChanged = onChanged;
            }

            public void onSensorChanged(AndroidJavaObject eventObject)
            {
                if (eventObject == null)
                {
                    return;
                }

                int type = 0;
                float[] eventValues = null;
                try
                {
                    using (AndroidJavaObject sensor = eventObject.Get<AndroidJavaObject>("sensor"))
                    {
                        if (sensor != null)
                        {
                            type = sensor.Call<int>("getType");
                        }
                    }

                    eventValues = eventObject.Get<float[]>("values");
                }
                catch (Exception exception)
                {
                    // Sensor events arrive frequently. Keep a malformed or
                    // vendor-specific event from flooding logcat/the UI thread.
                    if (!didLogReadFailure)
                    {
                        didLogReadFailure = true;
                        Debug.LogWarning("Sensor event read failed: " + exception.Message);
                    }

                    return;
                }

                onChanged?.Invoke(type, eventValues);
            }

            public void onAccuracyChanged(AndroidJavaObject sensor, int accuracy)
            {
                // Raw values are the only data needed by this mockup.
            }
        }
#endif

        [Serializable]
        private struct SensorSlot
        {
            public string name;
            public int type;

            public SensorSlot(string name, int type)
            {
                this.name = name;
                this.type = type;
            }
        }
    }
}
