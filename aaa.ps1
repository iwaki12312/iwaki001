$wshell = New-Object -ComObject WScript.Shell

Write-Host "5秒ごとにEnterキーを送信します。終了するには Ctrl + C を押してください。"
Start-Sleep -Seconds 3

while ($true) {
    $wshell.SendKeys("{ENTER}")
    Write-Host "Enterキーを送信しました。"
    Start-Sleep -Seconds 5
}
