#!/bin/bash

# Pipeline Performance Benchmark Script (Cross-platform)
# Run this script to benchmark the performance improvements made to MiddlewarePipelineBuilder and NotificationPipelineBuilder

echo "==========================================================="
echo "    Blazing.Mediator Pipeline Performance Benchmarks     "
echo "   Measuring <50ns execution target optimizations        "
echo "==========================================================="
echo ""

# Change to the benchmark directory
cd "$(dirname "$0")"

echo "Building benchmarks in Release mode..."
dotnet build -c Release --no-restore

if [ $? -ne 0 ]; then
    echo "Build failed! Please fix compilation errors first."
    exit 1
fi

echo ""
echo "Available Benchmark Suites:"
echo "1. Pipeline Performance Benchmarks (Comprehensive)"
echo "2. Performance Bottleneck Benchmarks (Specific optimizations)"
echo "3. Nanosecond Precision Benchmarks (<50ns target validation)"
echo "4. Run All Benchmarks"
echo ""

read -p "Select benchmark suite (1-4): " choice

case $choice in
    1)
        echo "Running Pipeline Performance Benchmarks..."
        dotnet run -c Release -- --filter "*PipelinePerformanceBenchmarks*" --exporters github markdown --memory
        ;;
    2)
        echo "Running Performance Bottleneck Benchmarks..."
        dotnet run -c Release -- --filter "*PerformanceBottleneckBenchmarks*" --exporters github markdown --memory
        ;;
    3)
        echo "Running Nanosecond Precision Benchmarks..."
        dotnet run -c Release -- --filter "*NanosecondPrecisionBenchmarks*" --exporters github markdown --memory --job short
        ;;
    4)
        echo "Running All Pipeline Benchmarks..."
        echo "This will take several minutes to complete..."
        dotnet run -c Release -- --filter "*Pipeline*" --exporters github markdown --memory
        ;;
    *)
        echo "Invalid selection. Running Pipeline Performance Benchmarks..."
        dotnet run -c Release -- --filter "*PipelinePerformanceBenchmarks*" --exporters github markdown --memory
        ;;
esac

echo ""
echo "==========================================================="
echo "                    Benchmark Complete                     "
echo "==========================================================="
echo ""
echo "Performance Analysis Summary:"
echo "• Assembly scanning: Should show dramatic improvement (30s -> ms)"
echo "• Sorting algorithms: O(n²) -> O(1) optimization visible"
echo "• Memory allocations: Reduced due to LINQ elimination"
echo "• Pipeline execution: Overall faster end-to-end performance"
echo ""
echo "Look for benchmark results in the console output above and in generated markdown files."