param(
    [Parameter(Mandatory = $true, HelpMessage = "Previous version (e.g. 26.01)")]
    [string]$prev,
    [Parameter(Mandatory = $true, HelpMessage = "New version (e.g. 26.05)")]
    [string]$new
)

Get-ChildItem MediaInfo\source-*.txt | ForEach-Object { (Get-Content $_) -replace $prev, $new | Set-Content $_ }
git -C .\External\MediaInfo fetch
git -C .\External\MediaInfo checkout "v$new"
pwsh .\Generate-LocalizedStrings.ps1
