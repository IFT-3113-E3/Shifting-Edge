#!/bin/bash
set -e

# Config
METADATA_PARSER="./bash-json-parser.sh"
STUB_HEADER="-- UNITY STUB FILE --"

# Usage
if [ $# -lt 2 ]; then
    echo "❌ Usage: $0 <assets-root> <zip-archive-containing-metadata>"
    echo "Example: $0 Assets assets-full.zip"
    exit 1
fi

ASSET_ROOT="$1"
ARCHIVE="$2"

if [ ! -d "$ASSET_ROOT" ]; then
    echo "❌ Error: Asset root directory '$ASSET_ROOT' does not exist."
    exit 1
fi

if [ ! -f "$ARCHIVE" ]; then
    echo "❌ Error: Zip archive '$ARCHIVE' not found."
    exit 1
fi

if [ ! -f "$METADATA_PARSER" ]; then
    echo "❌ Error: Missing parser script '$METADATA_PARSER'."
    echo "Please place the bash-json-parser script in the current directory."
    exit 1
fi

echo "📦 Extracting metadata.json from '$ARCHIVE'..."
METADATA_JSON=$(unzip -p "$ARCHIVE" "metadata.json")
if [ -z "$METADATA_JSON" ]; then
    echo "❌ Error: metadata.json not found in archive."
    exit 1
fi

echo "🧾 Parsing metadata with bash-json-parser..."
KEYVALS=$(echo "$METADATA_JSON" | bash "$METADATA_PARSER")

# Parse metadata into indexed fields
declare -A PATHS
declare -A SHAS
declare -A SIZES

while IFS='=' read -r key value; do
    if [[ "$key" =~ ^assets\.([0-9]+)\.([^=]+)$ ]]; then
        index="${BASH_REMATCH[1]}"
        field="${BASH_REMATCH[2]}"
        case "$field" in
            path) PATHS["$index"]="$value" ;;
            sha256) SHAS["$index"]="$value" ;;
            size) SIZES["$index"]="$value" ;;
        esac
    fi
done <<< "$KEYVALS"

# Replace real files with stub files
for index in "${!PATHS[@]}"; do
    rel_path="${PATHS[$index]}"
    sha="${SHAS[$index]}"
    size="${SIZES[$index]}"

    # Normalize to full path
    if [[ "$rel_path" != "$ASSET_ROOT"* ]]; then
        full_path="$ASSET_ROOT/$rel_path"
    else
        full_path="$rel_path"
    fi

    if [ ! -f "$full_path" ]; then
        echo "⚠️ Warning: File not found: $full_path"
        continue
    fi

    if head -n 1 "$full_path" | grep -q -- "$STUB_HEADER"; then
        echo "ℹ️ Already stubbed: $full_path"
        continue
    fi

    echo "🔁 Replacing with stub: $full_path"
    echo "$STUB_HEADER" > "$full_path"
    echo "sha256: $sha" >> "$full_path"
    echo "size: $size" >> "$full_path"
done

echo "✅ Done replacing matching files with stubs."
