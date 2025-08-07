#!/bin/bash

# Function to get encoding of a file
get_encoding() {
    local file="$1"
    # Get encoding using the `file` command
    encoding=$(file -i "$file" | awk -F "=" '{print $2}')
    echo "${file#./} - Encoding: $encoding"
}

# Find all files in the current directory and subdirectories (recursively),
# excluding `bin` and `obj` directories and hidden directories
find . -type f ! -path '*/.*/*' ! -path '*/bin/*' ! -path '*/obj/*' | while read -r file; do
    # Check if the file is a text file
    if file "$file" | grep -q 'text'; then
        get_encoding "$file"
    fi
done