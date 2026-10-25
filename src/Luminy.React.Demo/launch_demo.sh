#!/usr/bin/env bash
set -e

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
cd "$DIR"

echo "=========================================="
echo " Launching Luminy.React Interactive Demo "
echo "=========================================="

if [ ! -d "node_modules" ]; then
    echo "Installing dependencies..."
    npm install --no-bin-links
fi

echo "Starting Vite development server on http://localhost:3000..."
npm run dev -- --open
