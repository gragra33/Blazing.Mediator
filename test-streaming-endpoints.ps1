#!/usr/bin/env pwsh

# Test script to verify Streaming.Api endpoints

$baseUrl = "https://localhost:7021"

Write-Host "Testing Streaming.Api endpoints..." -ForegroundColor Green

try {
    # Test contact count endpoint
    Write-Host "`nTesting GET /api/contacts/count..." -ForegroundColor Yellow
    $countResponse = Invoke-RestMethod -Uri "$baseUrl/api/contacts/count" -Method GET -SkipCertificateCheck
    Write-Host "Contact count: $($countResponse.count)" -ForegroundColor Cyan

    # Test get all contacts endpoint (non-streaming)
    Write-Host "`nTesting GET /api/contacts/all..." -ForegroundColor Yellow
    $allContactsResponse = Invoke-RestMethod -Uri "$baseUrl/api/contacts/all" -Method GET -SkipCertificateCheck
    Write-Host "Retrieved $($allContactsResponse.Length) contacts via bulk endpoint" -ForegroundColor Cyan

    # Test streaming endpoint (JSON)
    Write-Host "`nTesting GET /api/contacts/stream..." -ForegroundColor Yellow
    $streamResponse = Invoke-RestMethod -Uri "$baseUrl/api/contacts/stream" -Method GET -SkipCertificateCheck
    Write-Host "Retrieved $($streamResponse.Length) contacts via streaming endpoint" -ForegroundColor Cyan

    Write-Host "`nAll endpoint tests completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error testing endpoints: $_" -ForegroundColor Red
    Write-Host "Make sure Streaming.Api is running on $baseUrl" -ForegroundColor Yellow
}
