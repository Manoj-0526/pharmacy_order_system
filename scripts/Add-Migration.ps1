param(
    [Parameter(Mandatory = $true)]
    [string]$MigrationName
)

dotnet tool restore
dotnet ef migrations add $MigrationName --project .\Pharmacy-order_system.csproj
