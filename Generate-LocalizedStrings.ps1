function Get-ResourceFileForLang {
    param([string] $Lang)

    $DefaultLang = "en"
    try {
        Get-Content "FluentInfo\Strings\$Lang\Resources.resw" -ErrorAction Stop
    }
    catch {
        Get-Content "FluentInfo\Strings\$DefaultLang\Resources.resw" -ErrorAction Stop
    }
}

$fixISO639 = @{ "gr" = "el" }

$langs = Get-ChildItem "External\MediaInfo\Source\Resource\Plugin\Language"

foreach ($langCsv in $langs) {
    $lang = $langCsv.BaseName
    if ($fixISO639.ContainsKey($lang)) {
        $lang = $fixISO639[$lang]
    }
    Write-Output "updating $lang"

    $csvFile = Get-Content $langCsv -Raw
    $resourcesFile = Get-ResourceFileForLang $lang
    [xml]$resourcesFileXml = $resourcesFile

    $resourcesFileXml.root.data | Where-Object { $_.name -eq "MediaInfoLanguage" } | ForEach-Object { $_.value = $csvFile }

    $null = New-Item -ItemType Directory -Path "FluentInfo\Strings\$lang\" -Force
    $resourcesFileXml.Save("FluentInfo\Strings\$lang\Resources.resw")
}
