# Mockup scripts

All scripts resolve the repository root from their own path and use the
worker's `unity-editor` wrapper by default. Set `UNITY_BIN` when testing on a
different machine.

```bash
./scripts/bootstrap-project.sh
./scripts/test.sh
./scripts/visual-preview.sh
./scripts/build-android.sh
```

The A7 scripts select the authenticated wireless device from `adb devices` or
use `ADB_SERIAL` explicitly. A missing device is a clean skip unless
`A7_REQUIRED=1` is set. Screen recording is opt-in:

```bash
ADB_SERIAL=192.168.0.222:5555 A7_SCREENRECORD=1 ./scripts/test-a7.sh
```
