#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_bin="${UNITY_BIN:-unity-editor}"

exec "${unity_bin}" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "${repo_root}" \
  -executeMethod CapstoneDesign.EditorTools.BuildTools.ValidateProject \
  -logFile -
