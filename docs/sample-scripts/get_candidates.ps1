$feature_branch = '8'

$adopt_registry = "Registry::HKLM\Software\AdoptOpenJDK"
# use the following line if you need 32-bit installations
$adopt_registry_x86 = "Registry::HKLM\Software\WOW6432Node\AdoptOpenJDK"

$image_types = Get-ChildItem -Path $adopt_registry
foreach ($type in $image_types) {
    $matching_installations = Get-ChildItem -Path $type.PSPath | Where Name -Like "*\$($feature_branch)*"

    $c = ''
    foreach ($inst in $matching_installations) {
        $c = (Get-ItemProperty (Get-ChildItem -Path $inst.PSPath -Recurse | Where Name -Like '*\MSI*').PSPath).Path
    }

    New-Variable -Force -Name "candidate_$($type.PSChildName)" -Value $c
    Write-Host "The most likely $($type.PSChildName) candidate for $($feature_branch) branch is $($c)"
}

