#Requires -Version 7.2
<#
.SYNOPSIS
  Builds the container image, pushes it to Amazon ECR and rolls it out to an existing
  Amazon ECS service, then verifies the public /health endpoint reports the new build.

.DESCRIPTION
  Generic on purpose: every account-specific value is a parameter, so the same script serves
  any AWS account. The service must already exist (see docs/infrastructure.md for the
  one-time setup). The script:
    1. refuses to run with uncommitted changes (the deployed commit must be reproducible);
    2. builds the image from src/KodingTask.Api/Dockerfile, tagged <version>-<commit>;
    3. logs in to ECR and pushes the image;
    4. clones the service's current task definition, changing only the image, the
       BUILD_VERSION, BUILD_COMMIT and BUILD_DATE variables, and the variables and secrets
       given in -EnvironmentFile / -SecretsFile (JSON objects: { "NAME": "value" } and
       { "NAME": "arn-of-secret-or-parameter" }), which are added or replaced;
    5. updates the service and waits until the deployment completes (the ECS circuit
       breaker rolls back automatically if the new tasks never become healthy);
    6. polls the health URL until it reports the expected version and commit.

.EXAMPLE
  ./deploy/deploy.ps1 -Region eu-central-1 -Cluster my-cluster -Service kodingtask `
      -Repository kodingtask -HealthUrl https://kodingtask.example.com/health
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Region,
    [Parameter(Mandatory)] [string] $Cluster,
    [Parameter(Mandatory)] [string] $Service,
    [Parameter(Mandatory)] [string] $Repository,
    [Parameter(Mandatory)] [string] $HealthUrl,
    [string] $AwsProfile,
    [string] $Container,
    [string] $EnvironmentFile,
    [string] $SecretsFile,
    [switch] $AllowDirty,
    [int] $TimeoutMinutes = 15
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

function Invoke-Aws {
    $arguments = @($args) + @('--region', $Region, '--output', 'json')
    if ($AwsProfile) { $arguments += @('--profile', $AwsProfile) }
    $result = & aws @arguments
    if ($LASTEXITCODE -ne 0) { throw "aws $($args -join ' ') failed with exit code $LASTEXITCODE." }
    if ($result) { return ($result | Out-String | ConvertFrom-Json -Depth 50) }
}

function Write-Step([string] $message) {
    Write-Host ""
    Write-Host "==> $message" -ForegroundColor Cyan
}

foreach ($tool in 'aws', 'docker', 'git') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "'$tool' is not installed or not on PATH." }
}

Write-Step 'Checking the working tree'
$dirty = git status --porcelain
if ($dirty -and -not $AllowDirty) {
    throw "There are uncommitted changes. Commit them first, or pass -AllowDirty to deploy them anyway."
}
$version = (Get-Content -Raw versions.json | ConvertFrom-Json).versionKodingTask
$commit = (git rev-parse --short=12 HEAD).Trim()
if ($dirty) { $commit = "$commit-dirty" }
$buildDate = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
Write-Host "version $version, commit $commit"

Write-Step 'Resolving the registry'
$account = (Invoke-Aws sts get-caller-identity).Account
$registry = "$account.dkr.ecr.$Region.amazonaws.com"
$image = "${registry}/${Repository}:$version-$commit"
Write-Host $image

Write-Step 'Building the image'
# --pull: start from the newest .NET 10 patch (security fixes), never a stale cached base image.
docker build --pull -f src/KodingTask.Api/Dockerfile -t $image .

Write-Step 'Pushing the image'
$loginArguments = @('ecr', 'get-login-password', '--region', $Region)
if ($AwsProfile) { $loginArguments += @('--profile', $AwsProfile) }
& aws @loginArguments | docker login --username AWS --password-stdin $registry
docker push $image

Write-Step 'Registering a new task definition revision'
$serviceInfo = (Invoke-Aws ecs describe-services --cluster $Cluster --services $Service).services[0]
if (-not $serviceInfo) { throw "Service '$Service' was not found in cluster '$Cluster'." }
$current = Invoke-Aws ecs describe-task-definition --task-definition $serviceInfo.taskDefinition --include TAGS
$definition = $current.taskDefinition
$containerDefinition = if ($Container) {
    $definition.containerDefinitions | Where-Object name -eq $Container
} else {
    $definition.containerDefinitions[0]
}
if (-not $containerDefinition) { throw "Container '$Container' not found in the task definition." }
$containerDefinition.image = $image

function Read-Pairs([string] $path) {
    $pairs = [ordered]@{}
    if ($path) {
        $json = Get-Content -Raw -Path $path | ConvertFrom-Json -AsHashtable
        foreach ($key in $json.Keys) { $pairs[$key] = [string] $json[$key] }
    }
    return $pairs
}

$variables = Read-Pairs $EnvironmentFile
foreach ($entry in @{ BUILD_VERSION = $version; BUILD_COMMIT = $commit; BUILD_DATE = $buildDate }.GetEnumerator()) {
    $variables[$entry.Key] = $entry.Value
}
$environment = [System.Collections.Generic.List[object]]::new()
foreach ($variable in @($containerDefinition.environment)) {
    if ($variable -and -not $variables.Contains($variable.name)) { $environment.Add($variable) }
}
foreach ($entry in $variables.GetEnumerator()) { $environment.Add([pscustomobject]@{ name = $entry.Key; value = $entry.Value }) }
$containerDefinition.environment = $environment.ToArray()

$secretReferences = Read-Pairs $SecretsFile
if ($secretReferences.Count -gt 0) {
    $secrets = [System.Collections.Generic.List[object]]::new()
    foreach ($secret in @($containerDefinition.secrets)) {
        if ($secret -and -not $secretReferences.Contains($secret.name)) { $secrets.Add($secret) }
    }
    foreach ($entry in $secretReferences.GetEnumerator()) { $secrets.Add([pscustomobject]@{ name = $entry.Key; valueFrom = $entry.Value }) }
    $containerDefinition | Add-Member -NotePropertyName secrets -NotePropertyValue $secrets.ToArray() -Force
}

foreach ($readOnly in 'taskDefinitionArn', 'revision', 'status', 'requiresAttributes', 'compatibilities',
                     'registeredAt', 'registeredBy', 'deregisteredAt') {
    $definition.PSObject.Properties.Remove($readOnly)
}
if ($current.tags) { $definition | Add-Member -NotePropertyName tags -NotePropertyValue $current.tags -Force }

$definitionFile = New-TemporaryFile
try {
    $definition | ConvertTo-Json -Depth 50 | Set-Content -Path $definitionFile -Encoding utf8NoBOM
    $registered = Invoke-Aws ecs register-task-definition --cli-input-json "file://$definitionFile"
} finally {
    Remove-Item $definitionFile -ErrorAction SilentlyContinue
}
$newDefinition = $registered.taskDefinition.taskDefinitionArn
Write-Host $newDefinition

Write-Step 'Updating the service'
Invoke-Aws ecs update-service --cluster $Cluster --service $Service --task-definition $newDefinition | Out-Null

$deadline = [DateTime]::UtcNow.AddMinutes($TimeoutMinutes)
do {
    Start-Sleep -Seconds 15
    $state = (Invoke-Aws ecs describe-services --cluster $Cluster --services $Service).services[0]
    $primary = $state.deployments | Where-Object status -eq 'PRIMARY'
    Write-Host ("  {0}: running {1}/{2}, rollout {3}" -f $primary.taskDefinition.Split('/')[-1], $primary.runningCount, $primary.desiredCount, $primary.rolloutState)
    if ($primary.rolloutState -eq 'FAILED') { throw "The deployment failed: $($primary.rolloutStateReason)" }
    if ($primary.taskDefinition -ne $newDefinition) { throw "The service rolled back to $($primary.taskDefinition)." }
} until (($primary.rolloutState -eq 'COMPLETED' -and $state.deployments.Count -eq 1) -or [DateTime]::UtcNow -gt $deadline)
if ($primary.rolloutState -ne 'COMPLETED') { throw "The deployment did not complete within $TimeoutMinutes minutes." }

Write-Step "Verifying $HealthUrl"
$verified = $false
for ($attempt = 1; $attempt -le 20 -and -not $verified; $attempt++) {
    try {
        $health = Invoke-RestMethod -Uri $HealthUrl -Headers @{ 'Cache-Control' = 'no-cache' } -TimeoutSec 10
        Write-Host "  version $($health.version), commit $($health.commit)"
        $verified = $health.version -eq $version -and $health.commit -eq $commit
    } catch {
        Write-Host "  $($_.Exception.Message)"
    }
    if (-not $verified) { Start-Sleep -Seconds 6 }
}
if (-not $verified) { throw "The health endpoint does not report version $version and commit $commit." }

Write-Host ""
Write-Host "Deployed $version ($commit)." -ForegroundColor Green
