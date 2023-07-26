#!/bin/bash

SERVER_URL="http://localhost:8080"
NUM_TOTAL_REQUESTS=1000
WARM_UP_REQUESTS=100

# Function to make an HTTP request and record the response time
make_request() {
  local response_time
  response_time=$(curl -o /dev/null -s -w "%{time_total}\n" "$SERVER_URL")
  echo "$response_time"
}

# Run the .NET program in the background as an independent process
echo "Starting the .NET server..."
dotnet run --project tests/HyperSharp.Tests.csproj >/dev/null 2>&1 &
disown

# Wait a few seconds to ensure the .NET server is up and running
sleep 5

# Warm up the server with 100 HTTP requests
for ((i = 1; i <= WARM_UP_REQUESTS; i++)); do
  make_request >/dev/null
  printf "Warming up server: %d/%d\r" "$i" "$WARM_UP_REQUESTS"
done
echo

# Array to store the response times
response_times=()

# Loop to make the requests
for ((i = 1; i <= NUM_TOTAL_REQUESTS; i++)); do
  printf "Http Requests: %d/%d\r" "$i" "$NUM_TOTAL_REQUESTS"
  response_times+=($(make_request &))
done
echo

# Wait for any remaining background jobs to finish
wait

# Calculate the sum of response times
sum=0
for time in "${response_times[@]}"; do
  sum=$(awk "BEGIN {print $sum + $time}")
done

# Calculate the average response time in milliseconds
average_response_time_ms=$(awk "BEGIN {printf \"%.3f\", $sum / $NUM_TOTAL_REQUESTS * 1000}")

# Display the results
echo "Average Response Time: ${average_response_time_ms} milliseconds"
