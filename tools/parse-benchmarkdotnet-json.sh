#!/bin/bash

# Ensure JQ is installed
if ! command -v jq &>/dev/null; then
    (xbps-install -Syu >/dev/null && xbps-install -Sy jq >/dev/null) || {
        echo "Error: JQ installation failed." >&2
        exit 1
    }
fi

# Parse JSON file and extract required information
caption=$(jq -r '.HostEnvironmentInfo.BenchmarkDotNetCaption' $1)
version=$(jq -r '.HostEnvironmentInfo.BenchmarkDotNetVersion' $1)
os_version=$(jq -r '.HostEnvironmentInfo.OsVersion' $1)
processor_name=$(jq -r '.HostEnvironmentInfo.ProcessorName' $1)
physical_processor_count=$(jq -r '.HostEnvironmentInfo.PhysicalProcessorCount' $1)
physical_core_count=$(jq -r '.HostEnvironmentInfo.PhysicalCoreCount' $1)
logical_core_count=$(jq -r '.HostEnvironmentInfo.LogicalCoreCount' $1)
runtime_version=$(jq -r '.HostEnvironmentInfo.RuntimeVersion' $1)
architecture=$(jq -r '.HostEnvironmentInfo.Architecture' $1)

# Generate Markdown result for each benchmark
echo "### Machine Information:"
echo "$caption v$version, $os_version"
echo "- $processor_name, $physical_processor_count CPU, $logical_core_count logical and $physical_core_count physical cores"
echo "- $runtime_version, $architecture"

# Iterate over the benchmarks
jq -c '.Benchmarks[]' $1 | while read -r benchmark; do
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
    echo "### $display_info:"
    echo "Mean: $mean μs"
    echo "Error: $error μs"
    echo "StdDev: $std_dev μs"
    echo "Max HTTP Requests per second: $max_http_requests_per_second (1,000,000 / $mean)"
done