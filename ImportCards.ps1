# Grand Archive Card Importer
# This script fetches all cards from the Grand Archive API and saves them to the Cards folder

$ErrorActionPreference = "Stop"

# Configuration
$BaseUrl = "https://api.gatcg.com"
$CardsFolder = Join-Path $PSScriptRoot "Cards"
$PageSize = 50  # Maximum allowed by API

# Create Cards folder if it doesn't exist
if (-not (Test-Path $CardsFolder)) {
    New-Item -ItemType Directory -Path $CardsFolder -Force | Out-Null
    Write-Host "Created Cards folder at: $CardsFolder" -ForegroundColor Green
}

# Function to fetch a page of cards
function Get-CardsPage {
    param (
        [int]$Page
    )
    
    $url = "$BaseUrl/cards/search?page=$Page&page_size=$PageSize"
    try {
        $response = Invoke-RestMethod -Uri $url -Method Get -ContentType "application/json"
        return $response
    }
    catch {
        Write-Host "Error fetching page $Page : $_" -ForegroundColor Red
        return $null
    }
}

# Function to save a card to a JSON file
function Save-Card {
    param (
        [PSObject]$Card
    )
    
    $slug = $Card.slug
    if (-not $slug) {
        Write-Host "Card has no slug, skipping..." -ForegroundColor Yellow
        return
    }
    
    $filePath = Join-Path $CardsFolder "$slug.json"
    $Card | ConvertTo-Json -Depth 20 | Set-Content -Path $filePath -Encoding UTF8
}

# Main import logic
Write-Host "Starting Grand Archive card import..." -ForegroundColor Cyan
Write-Host "Fetching cards from: $BaseUrl/cards/search" -ForegroundColor Cyan
Write-Host ""

$currentPage = 1
$totalCards = 0
$importedCards = 0

# Fetch first page to get total count
$firstResponse = Get-CardsPage -Page 1
if (-not $firstResponse) {
    Write-Host "Failed to fetch initial data. Exiting." -ForegroundColor Red
    exit 1
}

$totalPages = $firstResponse.total_pages
$totalCards = $firstResponse.total_cards

Write-Host "Found $totalCards total cards across $totalPages pages" -ForegroundColor Green
Write-Host ""

# Process all pages
while ($currentPage -le $totalPages) {
    Write-Host "Processing page $currentPage of $totalPages..." -ForegroundColor Yellow
    
    $response = if ($currentPage -eq 1) { $firstResponse } else { Get-CardsPage -Page $currentPage }
    
    if (-not $response) {
        Write-Host "Failed to fetch page $currentPage, retrying in 2 seconds..." -ForegroundColor Red
        Start-Sleep -Seconds 2
        continue
    }
    
    foreach ($card in $response.data) {
        Save-Card -Card $card
        $importedCards++
        
        # Progress indicator
        if ($importedCards % 50 -eq 0) {
            $percent = [math]::Round(($importedCards / $totalCards) * 100, 1)
            Write-Host "  Imported $importedCards / $totalCards cards ($percent%)" -ForegroundColor Gray
        }
    }
    
    $currentPage++
    
    # Small delay to be nice to the API
    if ($currentPage -le $totalPages) {
        Start-Sleep -Milliseconds 200
    }
}

Write-Host ""
Write-Host "Import complete!" -ForegroundColor Green
Write-Host "Total cards imported: $importedCards" -ForegroundColor Green
Write-Host "Cards saved to: $CardsFolder" -ForegroundColor Green
