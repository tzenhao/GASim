# Grand Archive Card Image Importer
# This script downloads card images from the Grand Archive API based on the card JSON files

$ErrorActionPreference = "Stop"

# Configuration
$BaseUrl = "https://api.gatcg.com"
$CardsFolder = Join-Path $PSScriptRoot "Cards"
$ImagesFolder = Join-Path $PSScriptRoot "CardImages"

# Create Images folder if it doesn't exist
if (-not (Test-Path $ImagesFolder)) {
    New-Item -ItemType Directory -Path $ImagesFolder -Force | Out-Null
    Write-Host "Created CardImages folder at: $ImagesFolder" -ForegroundColor Green
}

# Check if Cards folder exists
if (-not (Test-Path $CardsFolder)) {
    Write-Host "Cards folder not found at: $CardsFolder" -ForegroundColor Red
    Write-Host "Please run ImportCards.ps1 first to download card data." -ForegroundColor Yellow
    exit 1
}

# Get all card JSON files
$cardFiles = Get-ChildItem -Path $CardsFolder -Filter "*.json"
$totalCards = $cardFiles.Count

if ($totalCards -eq 0) {
    Write-Host "No card JSON files found in: $CardsFolder" -ForegroundColor Red
    Write-Host "Please run ImportCards.ps1 first to download card data." -ForegroundColor Yellow
    exit 1
}

Write-Host "Starting Grand Archive card image import..." -ForegroundColor Cyan
Write-Host "Found $totalCards card files to process" -ForegroundColor Cyan
Write-Host "Images will be saved to: $ImagesFolder" -ForegroundColor Cyan
Write-Host ""

$processedCards = 0
$downloadedImages = 0
$skippedImages = 0
$failedImages = 0

# Process each card JSON file
foreach ($cardFile in $cardFiles) {
    $processedCards++
    
    try {
        # Read and parse the card JSON
        $cardJson = Get-Content -Path $cardFile.FullName -Raw | ConvertFrom-Json
        $cardName = $cardJson.name
        $cardSlug = $cardJson.slug
        
        # Process each edition
        if ($cardJson.editions) {
            foreach ($edition in $cardJson.editions) {
                $imagePath = $edition.image
                $editionSlug = $edition.slug
                
                if (-not $imagePath) {
                    continue
                }
                
                # Extract filename from the image path (e.g., "/cards/images/fcng06fi6r.jpg" -> "fcng06fi6r.jpg")
                $imageFilename = Split-Path -Leaf $imagePath
                
                # Create a more descriptive filename using the edition slug
                $outputFilename = "$editionSlug.jpg"
                $outputPath = Join-Path $ImagesFolder $outputFilename
                
                # Skip if image already exists
                if (Test-Path $outputPath) {
                    $skippedImages++
                    continue
                }
                
                # Build the full image URL
                $imageUrl = "$BaseUrl$imagePath"
                
                try {
                    # Download the image
                    Invoke-WebRequest -Uri $imageUrl -OutFile $outputPath -UseBasicParsing
                    $downloadedImages++
                    
                    # Progress indicator
                    if ($downloadedImages % 50 -eq 0) {
                        $percent = [math]::Round(($processedCards / $totalCards) * 100, 1)
                        Write-Host "  Downloaded $downloadedImages images (processed $processedCards / $totalCards cards, $percent%)" -ForegroundColor Gray
                    }
                }
                catch {
                    $failedImages++
                    Write-Host "  Failed to download: $editionSlug - $_" -ForegroundColor Red
                }
                
                # Small delay to be nice to the API
                Start-Sleep -Milliseconds 100
            }
        }
    }
    catch {
        Write-Host "Error processing $($cardFile.Name): $_" -ForegroundColor Red
    }
    
    # Progress update every 100 cards
    if ($processedCards % 100 -eq 0) {
        $percent = [math]::Round(($processedCards / $totalCards) * 100, 1)
        Write-Host "Processed $processedCards / $totalCards cards ($percent%)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Image import complete!" -ForegroundColor Green
Write-Host "  Cards processed: $processedCards" -ForegroundColor Green
Write-Host "  Images downloaded: $downloadedImages" -ForegroundColor Green
Write-Host "  Images skipped (already exist): $skippedImages" -ForegroundColor Yellow
Write-Host "  Images failed: $failedImages" -ForegroundColor $(if ($failedImages -gt 0) { "Red" } else { "Green" })
Write-Host "  Images saved to: $ImagesFolder" -ForegroundColor Green
