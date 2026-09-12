#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_bin="${UNITY_BIN:-unity-editor}"

echo "Generating Unity mockup in ${repo_root}"
exec "${unity_bin}" \
  -batchmode \
  -quit \
  -projectPath "${repo_root}" \
  -executeMethod CapstoneDesign.EditorTools.MockupProjectBuilder.BuildMockup \
  -logFile -
