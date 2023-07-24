#!/bin/bash

# Define the URL of the HTTP server you want to stress test
SERVER_URL="http://localhost:8080"

# Number of concurrent requests to simulate
CONCURRENT_REQUESTS=100

# Total number of requests to be made during the test
TOTAL_REQUESTS=10000

# Output directory to save individual response files
OUTPUT_DIR="responses"

# Maximum time to wait for a response (in seconds)
TIMEOUT=5

# Function to send concurrent requests and collect metrics
send_requests() {
  local url="$1"
  local request_count=$2
  local responses=()

  for ((i = 1; i <= request_count; i++)); do
    response_file="${OUTPUT_DIR}/response_$i.txt"
    curl -s --max-time "$TIMEOUT" -o "$response_file" -w "Request #$i | Http Status: %{http_code} | Time: %{time_starttransfer} seconds | Size Download: %{size_download} bytes | Speed Download: %{speed_download} bytes\n" "$url" &
    responses+=("$response_file")
  done
  wait

  if [ "$1" == "-v" ]; then
    for file in "${responses[@]}"; do
      cat "$file"
      printf "\n"
    done
  fi

  rm -rf "$OUTPUT_DIR"
}

# Function to run the stress test and measure time
run_stress_test() {
  echo "Running stress test with $CONCURRENT_REQUESTS concurrent requests..."
  mkdir -p "$OUTPUT_DIR"
  send_requests "$SERVER_URL" "$TOTAL_REQUESTS"
  duration=$SECONDS
  echo "Stress test completed in $duration seconds."
}

SECONDS=0

# Run the .NET program in the background
dotnet run --project tests/HyperSharp.Tests.csproj >/dev/null &
sleep 5

# Start the stress test
run_stress_test
