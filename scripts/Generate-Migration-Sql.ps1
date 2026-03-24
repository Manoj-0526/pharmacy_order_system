param(
    [string]$OutputFile = ".\\scripts\\migration.sql"
)

dotnet tool restore
dotnet ef migrations script --project .\Pharmacy-order_system.csproj --output $OutputFile
Write-Host "SQL script generated at $OutputFile"
