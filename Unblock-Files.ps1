# Unblock-Files.ps1
# This script removes the "Mark of the Web" from all files in the repository.
# Run this script after cloning or downloading the repository if you encounter
# the error: "Couldn't process file due to its being in the Internet or Restricted
# zone or having the mark of the web on the file."

param(
    [string]$Path = $PSScriptRoot
)

Write-Host "Removing Mark of the Web from files in: $Path" -ForegroundColor Cyan

# Get all files recursively, excluding .git directory
$files = Get-ChildItem -Path $Path -Recurse -File | Where-Object { $_.FullName -notlike "*\.git\*" }

$unblockedCount = 0
$errorCount = 0

foreach ($file in $files) {
    try {
        # Check if file has Zone.Identifier stream (Mark of the Web)
        $zoneId = Get-Item -Path $file.FullName -Stream Zone.Identifier -ErrorAction SilentlyContinue
        if ($zoneId) {
            Unblock-File -Path $file.FullName
            Write-Host "  Unblocked: $($file.Name)" -ForegroundColor Green
            $unblockedCount++
        }
    }
    catch {
        Write-Host "  Error unblocking: $($file.Name) - $_" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host ""
Write-Host "Complete!" -ForegroundColor Cyan
Write-Host "  Files unblocked: $unblockedCount" -ForegroundColor Green
if ($errorCount -gt 0) {
    Write-Host "  Errors: $errorCount" -ForegroundColor Red
}

# Alternative: Unblock all files in one command (simpler approach)
# Get-ChildItem -Path $Path -Recurse | Unblock-File -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "You can also manually unblock files by:" -ForegroundColor Yellow
Write-Host "  1. Right-click the file -> Properties" -ForegroundColor Yellow
Write-Host "  2. Check 'Unblock' at the bottom of the General tab" -ForegroundColor Yellow
Write-Host "  3. Click OK" -ForegroundColor Yellow
