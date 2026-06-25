$rawInput = $input | Out-String
$logPath = "C:\Users\marijan.varivoda\source\FantasyFootball\.github\hooks\agent_log.txt"

try {
    $obj = $rawInput | ConvertFrom-Json
    if ($obj.prompt) {
        $obj.prompt = ($obj.prompt -replace '<ide_opened_file>[\s\S]*?</ide_opened_file>\s*', '').Trim()
    }
    $json = $obj | ConvertTo-Json -Depth 10
    $json = $json -replace '\\u003c','<' -replace '\\u003e','>' -replace '\\u0027',"'"
    Add-Content -Path $logPath -Value $json
} catch {
    Add-Content -Path $logPath -Value $rawInput
}
Add-Content -Path $logPath -Value ""