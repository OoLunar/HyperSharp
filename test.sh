#!/bin/bash

# Parse JSON file and extract required information
caption=$(jq -r '.HostEnvironmentInfo.BenchmarkDotNetCaption' input.json)
version=$(jq -r '.HostEnvironmentInfo.BenchmarkDotNetVersion' input.json)
os_version=$(jq -r '.HostEnvironmentInfo.OsVersion' input.json)
processor_name=$(jq -r '.HostEnvironmentInfo.ProcessorName' input.json)
physical_core_count=$(jq -r '.HostEnvironmentInfo.PhysicalCoreCount' input.json)
logical_core_count=$(jq -r '.HostEnvironmentInfo.LogicalCoreCount' input.json)
runtime_version=$(jq -r '.HostEnvironmentInfo.RuntimeVersion' input.json)
architecture=$(jq -r '.HostEnvironmentInfo.Architecture' input.json)

# Generate Markdown result for each benchmark
echo "$caption v$version, $os_version"
echo "- $processor_name, 1 CPU, $logical_core_count logical and $physical_core_count physical cores"
echo "- $runtime_version, $architecture"

# Iterate over the benchmarks
jq -c '.Benchmarks[]' input.json | while read -r benchmark; do
    mean_ns=$(jq -r '.Statistics.Mean' <<< "$benchmark")
    error_ns=$(jq -r '.Statistics.ConfidenceInterval.Margin' <<< "$benchmark")
    std_dev_ns=$(jq -r '.Statistics.StandardDeviation' <<< "$benchmark")
    display_info=$(jq -r '.Method' <<< "$benchmark")
    max_http_requests_per_second=$(jq -r '1000000000 / .Statistics.Mean' <<< "$benchmark")

    # Convert time values from nanoseconds to microseconds
    mean=$(awk "BEGIN {printf \"%.2f\", $mean_ns / 1000}")
    error=$(awk "BEGIN {printf \"%.2f\", $error_ns / 1000}")
    std_dev=$(awk "BEGIN {printf \"%.2f\", $std_dev_ns / 1000}")

    # Format max_http_requests_per_second value to have no decimal places
    max_http_requests_per_second=$(printf "%'.2f" "$max_http_requests_per_second")

    # Generate Markdown result for the current benchmark
    echo
    echo "### $display_info:"
    echo "Mean: $mean μs"
    echo "Error: $error μs"
    echo "StdDev: $std_dev μs"
    echo "Max HTTP Requests per second: $max_http_requests_per_second (1,000,000 / $mean)"
done