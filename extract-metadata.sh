#!/bin/bash
set -e

if [ -z "$1" ]; then
    echo "❌ Usage: $0 <zip-archive>"
    echo "Example: $0 assets-full.zip"
    exit 1
fi

ZIPFILE="$1"
METADATA_PATH="metadata.json"

if [ ! -f "$ZIPFILE" ]; then
    echo "❌ Error: '$ZIPFILE' does not exist."
    exit 1
fi

echo "📦 Extracting $METADATA_PATH from $ZIPFILE..."
unzip -p "$ZIPFILE" "$METADATA_PATH"
