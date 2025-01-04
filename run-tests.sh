#!/usr/bin/env bash
# Runs every input/output test in iotests/ against a Release build of the game.
# Each iotests/NN-name.in is fed to the program, with iotests/ as the working directory,
# and its output is compared, byte by byte, with iotests/NN-name.out.
set -u
cd "$(dirname "$0")"

dotnet build src/Project/Project.csproj -c Release -nologo -v q > /dev/null || {
    echo "Build failed"; exit 1;
}
app="$PWD/src/Project/bin/Release/net8.0/Project.dll"

passed=0; failed=0
for input in iotests/*.in; do
    expected="${input%.in}.out"
    actual="$(mktemp)"
    (cd iotests && dotnet "$app" < "$(basename "$input")" > "$actual")
    if cmp -s "$expected" "$actual"; then
        echo "PASS  $(basename "${input%.in}")"
        passed=$((passed + 1))
    else
        echo "FAIL  $(basename "${input%.in}")"
        diff "$expected" "$actual" | head -20
        failed=$((failed + 1))
    fi
    rm -f "$actual"
done

# The tests write save files with relative names; remove everything that is not a test file.
find iotests -maxdepth 1 -name '*.txt' -type f -delete

echo "$passed passed, $failed failed"
[ "$failed" -eq 0 ]
