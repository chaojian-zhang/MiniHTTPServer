# End-to-end test: start the real server, then send it raw requests over a socket.
# HTTP clients like curl clean up URLs before sending them, which would hide the requests we most want to test.

$Port = 19080
$Temp = Join-Path ([IO.Path]::GetTempPath()) "MiniHTTPServerTest_$PID"
$Root = Join-Path $Temp "site"
$Failures = 0

function Send-Request($RequestLine) {
    $Client = [Net.Sockets.TcpClient]::new("127.0.0.1", $Port)
    $Stream = $Client.GetStream()
    $Bytes = [Text.Encoding]::ASCII.GetBytes("$RequestLine`r`nHost: localhost`r`n`r`n")
    $Stream.Write($Bytes, 0, $Bytes.Length)

    $Reply = [IO.MemoryStream]::new()
    $Stream.CopyTo($Reply)
    $Client.Close()
    return , $Reply.ToArray() # The comma stops PowerShell from turning an empty reply into $null
}

# Latin-1 maps every byte to one char, so the reply can be searched as text while staying byte-accurate.
function Get-Text($Bytes) { return [Text.Encoding]::GetEncoding("iso-8859-1").GetString($Bytes) }

function Test-Reply($Description, $RequestLine, $ExpectedStatus, $ExpectedText = "") {
    $Text = Get-Text (Send-Request $RequestLine)
    $Passed = $Text.StartsWith("HTTP/1.1 $ExpectedStatus") -and $Text.Contains($ExpectedText) -and -not $Text.Contains("TOP SECRET")
    Write-Host "$(if ($Passed) { 'PASS' } else { 'FAIL' })  $Description"
    if (-not $Passed) {
        Write-Host "      Sent: $RequestLine"
        Write-Host "      Got:  $($Text.Split("`r")[0])"
        $script:Failures++
    }
}

# Build a test site, with files outside of the root that must never be served
New-Item -ItemType Directory -Force -Path "$Root\docs", "$Temp\site-private" | Out-Null
Copy-Item "$PSScriptRoot\Example\*" $Root
Set-Content "$Root\docs\index.html" "<h1>Docs</h1>"
Set-Content "$Root\my file.txt" "A file with a space"
Set-Content "$Temp\secret.txt" "TOP SECRET"
Set-Content "$Temp\site-private\secret.txt" "TOP SECRET"
$AllBytes = [byte[]](0..255)
[IO.File]::WriteAllBytes("$Root\all-bytes.png", $AllBytes)

dotnet build "$PSScriptRoot\MiniHTTPServer\MiniHTTPServer.csproj" --output "$Temp\bin" --verbosity quiet
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed."; Exit 1 }
$Server = Start-Process dotnet -ArgumentList "`"$Temp\bin\MiniHTTPServer.dll`" $Port `"$Root`"" -PassThru -NoNewWindow -RedirectStandardOutput "$Temp\server.log"

try {
    Start-Sleep -Seconds 2

    Write-Host "--- Serving files"
    Test-Reply "Folder serves its index.html" "GET / HTTP/1.1" "200" "Content-Type: text/html"
    Test-Reply "CSS gets its own type" "GET /styles.css HTTP/1.1" "200" "Content-Type: text/css"
    Test-Reply "JavaScript gets its own type" "GET /scripts.js HTTP/1.1" "200" "Content-Type: text/javascript"
    Test-Reply "Query string is ignored" "GET /styles.css?v=2 HTTP/1.1" "200"
    Test-Reply "URL is decoded" "GET /my%20file.txt HTTP/1.1" "200" "A file with a space"
    Test-Reply "Subfolder serves its index.html" "GET /docs/ HTTP/1.1" "200" "<h1>Docs</h1>"
    Test-Reply "Subfolder without a slash redirects" "GET /docs HTTP/1.1" "301" "Location: docs/"

    $Binary = Send-Request "GET /all-bytes.png HTTP/1.1"
    $Body = $Binary[($Binary.Length - 256)..($Binary.Length - 1)]
    $Passed = (Get-Text $Binary).Contains("Content-Length: 256") -and -not (Compare-Object $AllBytes $Body -SyncWindow 0)
    Write-Host "$(if ($Passed) { 'PASS' } else { 'FAIL' })  Binary file arrives unchanged"
    if (-not $Passed) { $Failures++ }

    Write-Host "--- Status codes"
    Test-Reply "Missing file" "GET /nope.html HTTP/1.1" "404"
    Test-Reply "POST isn't allowed" "POST / HTTP/1.1" "405" "Allow: GET, HEAD"
    Test-Reply "Malformed request" "NONSENSE" "400"
    $Head = Get-Text (Send-Request "HEAD /styles.css HTTP/1.1")
    $Passed = $Head.StartsWith("HTTP/1.1 200") -and $Head.Contains("Content-Length: ") -and $Head.EndsWith("`r`n`r`n")
    Write-Host "$(if ($Passed) { 'PASS' } else { 'FAIL' })  HEAD sends the header only"
    if (-not $Passed) { $Failures++ }

    Write-Host "--- Requests trying to leave the root"
    $AbsolutePath = "$Temp\secret.txt" -replace '\\', '/'
    @(
        "/../secret.txt"
        "/docs/../../secret.txt"
        "/..\secret.txt"
        "/%2e%2e/secret.txt"
        "/..%2fsecret.txt"
        "/..%5csecret.txt"
        "/%2E%2E%2Fsecret.txt"
        "/%252e%252e/secret.txt"
        "/../site-private/secret.txt"
        "//$AbsolutePath"
        "/$AbsolutePath"
        "/styles.css::`$DATA"
        "/styles.css%00.png"
    ) | ForEach-Object { Test-Reply "Blocked: $_" "GET $_ HTTP/1.1" "404" }
}
finally {
    Stop-Process -Id $Server.Id -Force -ErrorAction SilentlyContinue
    $Server.WaitForExit()
    Remove-Item $Temp -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "$(if ($Failures) { "$Failures test(s) failed." } else { 'All tests passed.' })"
Exit $Failures
