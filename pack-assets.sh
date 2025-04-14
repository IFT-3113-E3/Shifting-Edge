#!/bin/bash
set -e

EXTENSIONS=("*.wav" "*.mp3" "*.fbx" "*.png" "*.jpg" "*.ogg" "*.tga" "*.exr" "*.hdr" "*.mp4" "*.mov")
OUTPUT_ZIP="assets-full.zip"

if [ -z "$1" ]; then
    echo "❌ Error: Specify the Assets folder"
    echo "Usage: $0 <path-to-assets>"
    exit 1
fi

ASSET_ROOT="$1"
ASSET_ROOT="${ASSET_ROOT%/}" # Remove trailing slash if any

if [ ! -d "$ASSET_ROOT" ]; then
    echo "❌ Error: '$ASSET_ROOT' does not exist."
    exit 1
fi

TMP_DIR="assets-full-temp"
rm -rf "$TMP_DIR"
mkdir -p "$TMP_DIR"

# Collect matching files
declare -a FILES_TO_ZIP=()

echo "📁 Collecting files..."
for ext in "${EXTENSIONS[@]}"; do
    while IFS= read -r file; do
        FILES_TO_ZIP+=("$file")
    done < <(find "$ASSET_ROOT" -type f -iname "$ext")
done

# Copy files to temp folder and build metadata entries
METADATA_ENTRIES=()
for file in "${FILES_TO_ZIP[@]}"; do
    # Remove root path prefix
    rel_path="${file#$ASSET_ROOT/}"
    dest_path="$TMP_DIR/$ASSET_ROOT/$rel_path"

    mkdir -p "$(dirname "$dest_path")"
    cp "$file" "$dest_path"

    sha=$(sha256sum "$file" | cut -d ' ' -f 1)

    # macOS `stat` compatibility
    if stat --version &>/dev/null; then
        size=$(stat -c%s "$file")
    else
        size=$(stat -f%z "$file")
    fi

    METADATA_ENTRIES+=("    {
      \"path\": \"$rel_path\",
      \"sha256\": \"$sha\",
      \"size\": $size
    }")
done

# Join entries into JSON
metadata_json="{
  \"assets\": [
$(IFS=,$'\n'; echo "${METADATA_ENTRIES[*]}")
  ]
}"

# Write to metadata.json
echo "$metadata_json" > "$TMP_DIR/metadata.json"

# Create ZIP (cross-platform)
echo "📦 Creating archive: $OUTPUT_ZIP"
rm -f "$OUTPUT_ZIP"

if command -v zip > /dev/null; then
    (cd "$TMP_DIR" && zip -r "../$OUTPUT_ZIP" .)
elif command -v powershell.exe > /dev/null; then
    powershell.exe -NoProfile -Command "& {
        Compress-Archive -Path '$TMP_DIR/*' -DestinationPath '$OUTPUT_ZIP' -Force
    }"
else
    echo "❌ Error: Could not find zip or PowerShell."
    exit 1
fi

rm -rf "$TMP_DIR"
echo "✅ Done! Created '$OUTPUT_ZIP' with relative metadata paths"
