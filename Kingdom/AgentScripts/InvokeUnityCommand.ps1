param([string]$CodeFile,[string]$ToolName='Unity_RunCommand',[string]$ArgumentsJson)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$instance=Get-Content -LiteralPath (Join-Path $projectRoot 'Library/EditorInstance.json') -Raw | ConvertFrom-Json
$connection=Get-ChildItem -LiteralPath (Join-Path $env:USERPROFILE '.unity/mcp/connections') -Filter ('*-'+$instance.process_id+'.json') | Select-Object -First 1
$details=Get-Content -LiteralPath $connection.FullName -Raw | ConvertFrom-Json
$pipeName=($details.connection_path -split '\\')[-1]
$pipe=[System.IO.Pipes.NamedPipeClientStream]::new('.',$pipeName,[System.IO.Pipes.PipeDirection]::InOut,[System.IO.Pipes.PipeOptions]::Asynchronous)
try {
    $pipe.Connect(5000)
    $reader=[System.IO.StreamReader]::new($pipe)
    $writer=[System.IO.StreamWriter]::new($pipe,[System.Text.UTF8Encoding]::new($false))
    $writer.AutoFlush=$true
    $read=$reader.ReadLineAsync()
    if(!$read.Wait(15000)){throw 'Unity handshake timed out.'}
    $hello=$read.Result | ConvertFrom-Json
    if($hello.type -ne 'handshake'){throw ('Unexpected Unity response: '+$hello.type)}
    if($ArgumentsJson) {$arguments=$ArgumentsJson | ConvertFrom-Json -AsHashtable}
    else {$code=Get-Content -LiteralPath $CodeFile -Raw; $arguments=@{code=$code;title='Update and verify GlowKeeper forest'}}
    $requestId='forest-revision-'+[Guid]::NewGuid().ToString('N')
    $payload=@{type=$ToolName;requestId=$requestId;params=$arguments} | ConvertTo-Json -Depth 8 -Compress
    $writer.WriteLine($payload)
    $deadline=[DateTime]::UtcNow.AddSeconds(100)
    while([DateTime]::UtcNow -lt $deadline) {
        $read=$reader.ReadLineAsync()
        if(!$read.Wait(20000)){throw 'Unity command response timed out.'}
        if($null -eq $read.Result){throw 'Unity connection closed, possibly during script reload. Retry after compilation.'}
        $response=$read.Result | ConvertFrom-Json
        if($response.type -eq 'command_in_progress'){continue}
        if($response.type -eq 'approval_denied'){throw ('Unity denied the connection: '+$response.reason)}
        if($response.type -eq 'approval_pending'){Write-Output 'Unity connection approval is pending.';continue}
        Write-Output $read.Result
        if($response.requestId -eq $requestId -or $response.status){break}
    }
} finally {$pipe.Dispose()}
