#!/usr/bin/env pwsh

# Test script to verify ECommerce.Api notification endpoints and trigger all BackgroundService warnings

$baseUrl = "https://localhost:54336"

Write-Host "🔔 Testing ECommerce.Api notification endpoints..." -ForegroundColor Green
Write-Host "Watch console output for notification logs with 📧 (email) and 📦 (inventory) emojis" -ForegroundColor Yellow

try {
    # Test health check first
    Write-Host "`n🏥 Testing health check..." -ForegroundColor Yellow
    try {
        $healthResponse = Invoke-RestMethod -Uri "$baseUrl/swagger/index.html" -Method GET -SkipCertificateCheck
        Write-Host "✅ ECommerce.Api is running" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ ECommerce.Api is not running or not accessible at $baseUrl" -ForegroundColor Red
        Write-Host "Please start the ECommerce.Api project first" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "`n📋 NOTIFICATION WORKFLOW DEMONSTRATION" -ForegroundColor Cyan
    Write-Host "=" * 50 -ForegroundColor Cyan

    # Step 1: Create a product (triggers ProductCreatedNotification)
    Write-Host "`n🆕 Step 1: Creating test product (triggers ProductCreatedNotification)..." -ForegroundColor Yellow
    $productData = @{
        name = "Notification Demo Product"
        description = "Product created to demonstrate notification workflow"
        price = 149.99
        stockQuantity = 20
    } | ConvertTo-Json

    $productResponse = Invoke-RestMethod -Uri "$baseUrl/api/products" -Method POST -Body $productData -ContentType "application/json" -SkipCertificateCheck
    $productId = $productResponse
    Write-Host "✅ Created product with ID: $productId" -ForegroundColor Green

    # Step 2: Create an order (triggers OrderCreatedNotification + inventory notifications)
    Write-Host "`n📦 Step 2: Creating order (triggers OrderCreatedNotification + inventory notifications)..." -ForegroundColor Yellow
    $orderData = @{
        customerId = 1001
        customerEmail = "demo@notifications.com"
        shippingAddress = "123 Test St, Test City, TS 12345"
        items = @(
            @{
                productId = $productId
                quantity = 15
                unitPrice = 149.99
            }
        )
    } | ConvertTo-Json -Depth 3

    $orderResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method POST -Body $orderData -ContentType "application/json" -SkipCertificateCheck
    $orderId = $orderResponse.data
    Write-Host "✅ Created order with ID: $orderId" -ForegroundColor Green

    # Step 3: Process the order through complete workflow (triggers multiple OrderStatusChangedNotifications)
    Write-Host "`n⚙️ Step 3: Processing order through complete workflow (triggers multiple OrderStatusChangedNotifications)..." -ForegroundColor Yellow
    $workflowResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders/$orderId/process-workflow" -Method POST -SkipCertificateCheck
    Write-Host "✅ Order workflow completed: $($workflowResponse.message)" -ForegroundColor Green

    # Step 4: Create another order to push stock to low levels (triggers ProductStockLowNotification)
    Write-Host "`n⚠️ Step 4: Creating order to trigger low stock alert (triggers ProductStockLowNotification)..." -ForegroundColor Yellow
    $lowStockOrderData = @{
        customerId = 1002
        customerEmail = "demo2@notifications.com"
        shippingAddress = "456 Test Ave, Test City, TS 12345"
        items = @(
            @{
                productId = $productId
                quantity = 3
                unitPrice = 149.99
            }
        )
    } | ConvertTo-Json -Depth 3

    $lowStockOrderResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method POST -Body $lowStockOrderData -ContentType "application/json" -SkipCertificateCheck
    $lowStockOrderId = $lowStockOrderResponse.data
    Write-Host "✅ Created low stock trigger order with ID: $lowStockOrderId" -ForegroundColor Green

    # Step 5: Reduce stock further to trigger out of stock (triggers ProductOutOfStockNotification)
    Write-Host "`n🚨 Step 5: Reducing stock to trigger out of stock alert (triggers ProductOutOfStockNotification)..." -ForegroundColor Yellow
    $reduceStockResponse = Invoke-RestMethod -Uri "$baseUrl/api/products/$productId/reduce-stock?quantity=5" -Method POST -SkipCertificateCheck
    Write-Host "✅ Stock reduced: $($reduceStockResponse.message)" -ForegroundColor Green

    Write-Host "`n📊 ADVANCED NOTIFICATION TESTING SCENARIOS" -ForegroundColor Cyan
    Write-Host "=" * 50 -ForegroundColor Cyan

    # Test 1: Simulate bulk order to trigger multiple notifications
    Write-Host "`n🎯 Test 1: Creating product for bulk order test..." -ForegroundColor Yellow
    $bulkProductData = @{
        name = "Bulk Order Test Product"
        description = "Product for bulk order notification testing"
        price = 79.99
        stockQuantity = 25
    } | ConvertTo-Json

    $bulkProductResponse = Invoke-RestMethod -Uri "$baseUrl/api/products" -Method POST -Body $bulkProductData -ContentType "application/json" -SkipCertificateCheck
    $bulkProductId = $bulkProductResponse
    Write-Host "✅ Created bulk test product with ID: $bulkProductId" -ForegroundColor Green

    Write-Host "   🎯 Simulating bulk order (triggers OrderCreatedNotification + inventory notifications)..." -ForegroundColor Yellow
    $bulkOrderResponse = Invoke-RestMethod -Uri "$baseUrl/api/products/$bulkProductId/simulate-bulk-order?orderQuantity=10" -Method POST -SkipCertificateCheck
    Write-Host "✅ Bulk order simulated: $($bulkOrderResponse.message)" -ForegroundColor Green

    # Test 2: Create another product for additional testing
    Write-Host "`n🆕 Test 2: Creating second test product..." -ForegroundColor Yellow
    $product2Data = @{
        name = "Test Product 2"
        description = "Second product for notification testing"
        price = 99.99
        stockQuantity = 8
    } | ConvertTo-Json

    $product2Response = Invoke-RestMethod -Uri "$baseUrl/api/products" -Method POST -Body $product2Data -ContentType "application/json" -SkipCertificateCheck
    $product2Id = $product2Response
    Write-Host "✅ Created second product with ID: $product2Id" -ForegroundColor Green

    # Test 3: Test order status progression manually
    Write-Host "`n📈 Test 3: Testing order status progression (triggers OrderStatusChangedNotifications)..." -ForegroundColor Yellow
    
    # Create order for status testing
    $statusOrderData = @{
        customerId = 1003
        customerEmail = "status@notifications.com"
        shippingAddress = "789 Test Blvd, Test City, TS 12345"
        items = @(
            @{
                productId = $product2Id
                quantity = 2
                unitPrice = 99.99
            }
        )
    } | ConvertTo-Json -Depth 3

    $statusOrderResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method POST -Body $statusOrderData -ContentType "application/json" -SkipCertificateCheck
    $statusOrderId = $statusOrderResponse.data
    Write-Host "✅ Created status test order with ID: $statusOrderId" -ForegroundColor Green

    # Progress through order statuses
    Write-Host "   📋 Updating order to Processing status..." -ForegroundColor White
    $processingStatusData = @{
        orderId = $statusOrderId
        status = 2
        notes = "Order processing started"
    } | ConvertTo-Json

    $processingResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders/$statusOrderId/status" -Method PUT -Body $processingStatusData -ContentType "application/json" -SkipCertificateCheck
    Write-Host "   ✅ Order status updated to Processing" -ForegroundColor Green

    Write-Host "   📋 Updating order to Shipped status..." -ForegroundColor White
    $shippedStatusData = @{
        orderId = $statusOrderId
        status = 3
        notes = "Order shipped via test notification script"
    } | ConvertTo-Json

    $shippedResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders/$statusOrderId/status" -Method PUT -Body $shippedStatusData -ContentType "application/json" -SkipCertificateCheck
    Write-Host "   ✅ Order status updated to Shipped" -ForegroundColor Green

    Write-Host "   📋 Updating order to Delivered status..." -ForegroundColor White
    $deliveredStatusData = @{
        orderId = $statusOrderId
        status = 4
        notes = "Order delivered - notification test completed"
    } | ConvertTo-Json

    $deliveredResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders/$statusOrderId/status" -Method PUT -Body $deliveredStatusData -ContentType "application/json" -SkipCertificateCheck
    Write-Host "   ✅ Order status updated to Delivered" -ForegroundColor Green

    # Test 4: Complete order workflow using the complete endpoint
    Write-Host "`n🏁 Test 4: Using complete order endpoint (triggers multiple quick notifications)..." -ForegroundColor Yellow
    $completeOrderData = @{
        customerId = 1004
        customerEmail = "complete@notifications.com"
        shippingAddress = "101 Test Rd, Test City, TS 12345"
        items = @(
            @{
                productId = $product2Id
                quantity = 1
                unitPrice = 99.99
            }
        )
    } | ConvertTo-Json -Depth 3

    $completeOrderResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method POST -Body $completeOrderData -ContentType "application/json" -SkipCertificateCheck
    $completeOrderId = $completeOrderResponse.data
    Write-Host "✅ Created order for completion test with ID: $completeOrderId" -ForegroundColor Green

    # Complete the order
    $completionResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders/$completeOrderId/complete" -Method POST -SkipCertificateCheck
    Write-Host "✅ Order completed: $($completionResponse.message)" -ForegroundColor Green

    # Test 5: Cancel order to trigger cancellation notification
    Write-Host "`n❌ Test 5: Testing order cancellation (triggers OrderStatusChangedNotification)..." -ForegroundColor Yellow
    $cancelOrderData = @{
        customerId = 1005
        customerEmail = "cancel@notifications.com"
        shippingAddress = "202 Test Ln, Test City, TS 12345"
        items = @(
            @{
                productId = $product2Id
                quantity = 1
                unitPrice = 99.99
            }
        )
    } | ConvertTo-Json -Depth 3

    $cancelOrderResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method POST -Body $cancelOrderData -ContentType "application/json" -SkipCertificateCheck
    $cancelOrderId = $cancelOrderResponse.data
    Write-Host "✅ Created order for cancellation test with ID: $cancelOrderId" -ForegroundColor Green

    # Cancel the order
    $cancellationData = @{
        orderId = $cancelOrderId
        reason = "Test cancellation via notification script"
    } | ConvertTo-Json

    $cancellationResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders/$cancelOrderId/cancel" -Method POST -Body $cancellationData -ContentType "application/json" -SkipCertificateCheck
    Write-Host "✅ Order cancelled: $($cancellationResponse.message)" -ForegroundColor Green

    # Test 6: Test concurrent bulk orders to stress test notifications
    Write-Host "`n🚀 Test 6: Testing concurrent bulk orders (stress test notifications)..." -ForegroundColor Yellow
    $concurrent1 = Start-Job -ScriptBlock {
        param($baseUrl, $productId)
        Invoke-RestMethod -Uri "$baseUrl/api/products/$productId/simulate-bulk-order?orderQuantity=5" -Method POST -SkipCertificateCheck
    } -ArgumentList $baseUrl, $product2Id

    $concurrent2 = Start-Job -ScriptBlock {
        param($baseUrl, $productId)
        Invoke-RestMethod -Uri "$baseUrl/api/products/$productId/simulate-bulk-order?orderQuantity=3" -Method POST -SkipCertificateCheck
    } -ArgumentList $baseUrl, $product2Id

    # Wait for concurrent jobs to complete
    $job1Result = Receive-Job -Job $concurrent1 -Wait
    $job2Result = Receive-Job -Job $concurrent2 -Wait
    
    Remove-Job -Job $concurrent1, $concurrent2
    Write-Host "✅ Concurrent bulk orders completed" -ForegroundColor Green

    Write-Host "`n📊 FINAL SYSTEM STATUS CHECK" -ForegroundColor Cyan
    Write-Host "=" * 50 -ForegroundColor Cyan

    # Check low stock products
    Write-Host "`n📦 Checking low stock products..." -ForegroundColor Yellow
    $lowStockProducts = Invoke-RestMethod -Uri "$baseUrl/api/products/low-stock?threshold=10" -Method GET -SkipCertificateCheck
    Write-Host "✅ Found $($lowStockProducts.Count) low stock products" -ForegroundColor Green

    # Check order statistics
    Write-Host "`n📈 Checking order statistics..." -ForegroundColor Yellow
    $orderStats = Invoke-RestMethod -Uri "$baseUrl/api/orders/statistics" -Method GET -SkipCertificateCheck
    Write-Host "✅ Total orders: $($orderStats.totalOrders)" -ForegroundColor Green
    Write-Host "✅ Delivered orders: $($orderStats.deliveredOrders)" -ForegroundColor Green
    Write-Host "✅ Cancelled orders: $($orderStats.cancelledOrders)" -ForegroundColor Green

    Write-Host "`n🎉 NOTIFICATION TESTING COMPLETED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "=" * 50 -ForegroundColor Green
    Write-Host "All notification endpoints have been tested and should have triggered:" -ForegroundColor White
    Write-Host "📧 Email notifications (OrderCreatedNotification, OrderStatusChangedNotification)" -ForegroundColor White
    Write-Host "📦 Inventory notifications (ProductStockLowNotification, ProductOutOfStockNotification)" -ForegroundColor White
    Write-Host "🆕 Product notifications (ProductCreatedNotification)" -ForegroundColor White
    Write-Host "" -ForegroundColor White
    Write-Host "Check the ECommerce.Api console output for detailed notification logs!" -ForegroundColor Yellow

}
catch {
    Write-Host "❌ Error testing notification endpoints: $_" -ForegroundColor Red
    Write-Host "Make sure ECommerce.Api is running on $baseUrl" -ForegroundColor Yellow
    Write-Host "Start the project with: dotnet run --project src/samples/ECommerce.Api" -ForegroundColor Yellow
    exit 1
}
