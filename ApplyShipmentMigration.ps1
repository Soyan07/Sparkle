# PowerShell script to apply Shipment tables migration
# Run this script manually to add shipment tables to existing database

$scriptPath = "Sparkle.Infrastructure\Migrations\AddShipmentTablesManual.sql"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Sparkle Ecommerce - Shipment Tables Migration" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Read SQL script
$sql = Get-Content $scriptPath -Raw

# Create connection string
$connectionString = "Data Source=.\SQLEXPRESS;Initial Catalog=SparkleEcommerce;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"

Write-Host "Connecting to database..." -ForegroundColor Yellow

try {
    # Create SQL connection
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    Write-Host "Executing migration script..." -ForegroundColor Yellow
    
    # Execute SQL
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = $sql
    $command.CommandTimeout = 300 # 5 minutes
    
    $result = $command.ExecuteNonQuery()
    
    Write-Host ""
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host "Tables created: Shipments, ShipmentItems, ShipmentTrackingEvents" -ForegroundColor Green
    
    # Update migrations history
    Write-Host ""
    Write-Host "Updating __EFMigrationsHistory..." -ForegroundColor Yellow
    $migrationSql = @"
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251122070218_AddShipmentsTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251122070218_AddShipmentsTable', '8.0.11');
    PRINT 'Migration history updated';
END
ELSE
BEGIN
    PRINT 'Migration already recorded in history';
END
"@
    
    $command.CommandText = $migrationSql
    $command.ExecuteNonQuery() | Out-Null
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "SUCCESS! Shipment tables are ready to use." -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Cyan
}
catch {
    Write-Host ""
    Write-Host "ERROR: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "If tables already exist, this is expected." -ForegroundColor Yellow
    Write-Host "Check the error message above for details." -ForegroundColor Yellow
    
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
    }
}

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
