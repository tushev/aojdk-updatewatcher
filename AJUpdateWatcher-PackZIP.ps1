Add-Type -AssemblyName System.IO.Compression

$archive_filename = "$(get-location)\SetupOutput\AJUpdateWatcher-2.X.X.0-no-installer-latest.zip";

Remove-Item -Path $archive_filename -ErrorAction SilentlyContinue -Confirm:$false

# creates empty zip file:
[System.IO.Compression.ZipArchive] $arch = [System.IO.Compression.ZipFile]::Open($archive_filename,[System.IO.Compression.ZipArchiveMode]::Update)

# add your files to archive
foreach ($filename in Get-Content .\AJUpdateWatcher-list-to-ZIP.txt)
{
    $file = Get-Item -Path $filename

    $archive_name = $file.Name
    if ($file.DirectoryName -like '*3rdparty_licensing')
    {
        $archive_name = $filename
    }    
    
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($arch, $file.FullName, $archive_name)
}

$arch.Dispose()