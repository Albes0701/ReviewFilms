# PowerShell script to apply SQL migrations manually
# This is a workaround for the MySql.EntityFrameworkCore locking bug

$connectionString = "Server=localhost;Port=3306;Database=ReviewFilmsDb_Dev;User Id=root;"
$scriptPath = "D:\IT_K22\ReviewFilms\migrations_script.sql"

Write-Host "Applying migrations manually via SQL..."
Write-Host "Connection: $connectionString"
Write-Host "Script: $scriptPath"

# Try using MySqlConnector directly
try {
    Add-Type -Path "C:\Users\Magiauy\.nuget\packages\mysqlconnector\2.3.7\lib\net462\MySqlConnector.dll" -ErrorAction SilentlyContinue
    
    $connection = New-Object MySql.Data.MySqlClient.MySqlConnection($connectionString)
    $connection.Open()
    
    $sqlContent = Get-Content $scriptPath -Raw
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlContent
    $command.ExecuteNonQuery()
    
    Write-Host "✅ Migrations applied successfully!"
    $connection.Close()
}
catch {
    Write-Host "❌ Error: $_"
    Write-Host ""
    Write-Host "Alternative: Use MySQL Workbench or command line:"
    Write-Host "  mysql -h localhost -u root < migrations_script.sql"
}
