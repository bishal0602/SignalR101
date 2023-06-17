param (
    [Parameter(HelpMessage = "Indicates whether to run the console client")]
    [Alias("console")]
    [switch]$RunConsoleClient
)

function DotnetRun($projectPath) {
    try {
        $arguments = @(
            "run",
            "--project",
            $projectPath
        )
        Start-Process -FilePath "dotnet" -ArgumentList $arguments -PassThru
    } catch {
        Write-Error "Failed to run project '$projectPath'. Error: $_"
    }
}

$identityServer = ".\IdentityProvider\"
$mainServer = ".\SignalRServer\"
$consoleClient = ".\DotnetClient\"

DotnetRun $identityServer
DotnetRun $mainServer
if($RunConsoleClient){
    DotnetRun $consoleClient
}