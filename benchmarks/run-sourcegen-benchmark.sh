#!/bin/bash

# Blazing.Mediator Source Generator Benchmark Script
# This script runs benchmarks comparing reflection-based vs source generation performance

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}Blazing.Mediator Benchmark Runner${NC}"
echo -e "${CYAN}Reflection vs Source Generation${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""

# Get the root directory (where the .sln file is)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
MEDIATOR_PROJECT="$ROOT_DIR/src/Blazing.Mediator/Blazing.Mediator.csproj"
BENCHMARK_PROJECT="$ROOT_DIR/benchmarks/Blazing.Mediator.Benchmarks/Blazing.Mediator.Benchmarks.csproj"
RESULTS_DIR="$ROOT_DIR/BenchmarkDotNet.Artifacts"

echo -e "${GRAY}Root Directory: $ROOT_DIR${NC}"
echo -e "${GRAY}Mediator Project: $MEDIATOR_PROJECT${NC}"
echo -e "${GRAY}Benchmark Project: $BENCHMARK_PROJECT${NC}"
echo ""

# Function to run benchmarks
run_benchmark() {
    local config_name=$1
    local description=$2
    
    echo -e "${YELLOW}=======================================${NC}"
    echo -e "${YELLOW}Running: $description${NC}"
    echo -e "${YELLOW}Configuration: $config_name${NC}"
    echo -e "${YELLOW}=======================================${NC}"
    echo ""
    
    # Build Blazing.Mediator with the specific configuration
    echo -e "${CYAN}Step 1: Building Blazing.Mediator ($config_name)...${NC}"
    dotnet build "$MEDIATOR_PROJECT" -c "$config_name" --no-incremental
    
    echo -e "${GREEN}? Build successful${NC}"
    echo ""
    
    # Build the benchmark project
    echo -e "${CYAN}Step 2: Building benchmark project...${NC}"
    dotnet build "$BENCHMARK_PROJECT" -c Release
    
    echo -e "${GREEN}? Build successful${NC}"
    echo ""
    
    # Run the benchmarks
    echo -e "${CYAN}Step 3: Running benchmarks (this may take several minutes)...${NC}"
    dotnet run --project "$BENCHMARK_PROJECT" -c Release --no-build --framework net10.0
    
    echo ""
    echo -e "${GREEN}? Benchmark completed successfully${NC}"
    echo ""
}

# Check if user wants to run both or just one
echo -n "Run benchmarks for: [1] Reflection only, [2] SourceGen only, [3] Both (comparison) [default: 3]: "
read -r choice
choice=${choice:-3}

echo ""

case $choice in
    1)
        run_benchmark "Release" "Reflection-based (baseline)"
        ;;
    2)
        run_benchmark "SourceGen" "Source Generation (optimized)"
        ;;
    3)
        echo -e "${MAGENTA}Running FULL comparison benchmark...${NC}"
        echo -e "${MAGENTA}This will run both configurations for accurate comparison${NC}"
        echo ""
        
        run_benchmark "Release" "Reflection-based (baseline)"
        
        echo ""
        echo -e "${MAGENTA}=======================================${NC}"
        echo -e "${MAGENTA}Preparing for SourceGen benchmark...${NC}"
        echo -e "${MAGENTA}=======================================${NC}"
        echo ""
        
        run_benchmark "SourceGen" "Source Generation (optimized)"
        ;;
    *)
        echo -e "${RED}Invalid choice. Exiting.${NC}"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}All benchmarks completed!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

if [ -d "$RESULTS_DIR" ]; then
    echo -e "${CYAN}Results saved to: $RESULTS_DIR${NC}"
    echo ""
    echo -e "${YELLOW}Latest results:${NC}"
    
    # Find the most recent HTML report
    HTML_REPORT=$(find "$RESULTS_DIR" -name "*.html" -type f -printf '%T+ %p\n' 2>/dev/null | sort -r | head -1 | cut -d' ' -f2-)
    
    if [ -n "$HTML_REPORT" ]; then
        echo -e "  HTML Report: $HTML_REPORT"
        
        # Try to open the HTML report (works on most systems)
        echo -n "Open HTML report in browser? [Y/n]: "
        read -r open_report
        open_report=${open_report:-Y}
        
        if [[ "$open_report" =~ ^[Yy]$ ]]; then
            if command -v xdg-open &> /dev/null; then
                xdg-open "$HTML_REPORT" 2>/dev/null &
            elif command -v open &> /dev/null; then
                open "$HTML_REPORT" 2>/dev/null &
            else
                echo -e "${YELLOW}Could not auto-open report. Please open manually.${NC}"
            fi
        fi
    fi
fi

echo ""
echo -e "${YELLOW}Tip: Compare the 'Reflection-NET9' vs 'SourceGen-NET9' results${NC}"
echo -e "${YELLOW}Expected improvements: 15-35% faster, 30-50% less allocation${NC}"
echo ""
