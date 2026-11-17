<# :
@echo off
setlocal DisableDelayedExpansion
title GIN-PDF-RW Setup v002 [ VladiMIR + AI ]

set "PS_EXE=powershell.exe"
if exist "%SystemRoot%\Sysnative\WindowsPowerShell\v1.0\powershell.exe" set "PS_EXE=%SystemRoot%\Sysnative\WindowsPowerShell\v1.0\powershell.exe"

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [*] Elevating to Administrator privileges for GIN-PDF-RW Setup...
    "%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd.exe -ArgumentList '/c \"\"%~f0\"\"' -Verb RunAs"
    exit /b
)

set "GIN_SETUP_SCRIPT=%~f0"
start "" "%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -STA -WindowStyle Hidden -Command "try { $p = $env:GIN_SETUP_SCRIPT; if (-not $p -or -not (Test-Path $p)) { $p = '%~f0' }; $raw = [System.IO.File]::ReadAllText($p, [System.Text.Encoding]::UTF8); Invoke-Expression $raw } catch { Add-Type -AssemblyName System.Windows.Forms; [System.Windows.Forms.MessageBox]::Show($_.Exception.ToString(), 'GIN-PDF-RW Setup Error') }"
exit /b
#>

# ==============================================================================
#  GIN-PDF-RW SETUP                                                v002
#  Smart Multilingual Installer for Windows 10 / 11
#  Author: Vladimir Bulantsev (@GinCz) & AI [VladiMIR + AI]
#  Encoding: UTF-8 | Target: C:\Program Files\GIN-PDF-RW
# ==============================================================================

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase, System.Drawing, System.Windows.Forms

$script:Loc = @{
    "en" = @{
        "Title"       = "GIN-PDF-RW Setup v002 — Installation"
        "Header"      = "GIN-PDF-RW v002"
        "Sub"         = "Zen Open Source PDF Reader & Editor with AcroForms"
        "PathLabel"   = "Installation Directory:"
        "LangLabel"   = "Language:"
        "Associate"   = "Associate with .PDF files (Set as default reader)"
        "DesktopLnk"  = "Create Desktop shortcut (including MEGA Desktop)"
        "BtnInstall"  = "INSTALL NOW"
        "BtnCancel"   = "Cancel"
        "Installing"  = "Installing files..."
        "DoneMsg"     = "GIN-PDF-RW v002 has been successfully installed!"
    }
    "ru" = @{
        "Title"       = "Установка GIN-PDF-RW v002"
        "Header"      = "GIN-PDF-RW v002"
        "Sub"         = "Просмотр и редактор PDF с поддержкой форм AcroForms"
        "PathLabel"   = "Папка установки:"
        "LangLabel"   = "Язык программы:"
        "Associate"   = "Ассоциировать с файлами .PDF (открывать по умолчанию)"
        "DesktopLnk"  = "Создать ярлыки на рабочем столе (включая рабочий стол MEGA)"
        "BtnInstall"  = "УСТАНОВИТЬ"
        "BtnCancel"   = "Отмена"
        "Installing"  = "Копирование файлов..."
        "DoneMsg"     = "GIN-PDF-RW v002 успешно установлена в систему!"
    }
    "cs" = @{
        "Title"       = "Instalace GIN-PDF-RW v002"
        "Header"      = "GIN-PDF-RW v002"
        "Sub"         = "Zen Open Source prohlížeč a editor PDF s podporou formulářů"
        "PathLabel"   = "Cílová složka instalace:"
        "LangLabel"   = "Jazyk aplikace:"
        "Associate"   = "Přidružit k souborům .PDF (nastavit jako výchozí prohlížeč)"
        "DesktopLnk"  = "Vytvořit zástupce na ploše (včetně plochy MEGA)"
        "BtnInstall"  = "NAINSTALOVAT"
        "BtnCancel"   = "Zrušit"
        "Installing"  = "Kopírování souborů..."
        "DoneMsg"     = "GIN-PDF-RW v002 byl úspěšně nainstalován!"
    }
}

$sysLang = [System.Globalization.CultureInfo]::CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant()
if (-not $script:Loc.ContainsKey($sysLang)) { $sysLang = "en" }
$script:CurLang = $sysLang

function T($k) {
    $d = $script:Loc[$script:CurLang]
    if (-not $d.ContainsKey($k)) { $d = $script:Loc["en"] }
    return $d[$k]
}

[xml]$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="GIN-PDF-RW Setup"
        Height="460" Width="620"
        WindowStartupLocation="CenterScreen"
        Background="#181825" Foreground="#CDD6F4"
        FontFamily="Segoe UI, sans-serif" ResizeMode="NoResize">
    <Grid Margin="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,16">
            <TextBlock Name="TxtHeader" Text="GIN-PDF-RW v002" FontSize="22" FontWeight="Bold" Foreground="#89B4FA"/>
            <TextBlock Name="TxtSub" Text="Zen Open Source PDF Reader &amp; Editor" FontSize="13" Foreground="#BAC2DE" Margin="0,4,0,0"/>
        </StackPanel>

        <Border Grid.Row="1" Height="1" Background="#313244" Margin="0,0,0,16"/>

        <!-- Settings -->
        <StackPanel Grid.Row="2">
            <TextBlock Name="LblPath" Text="Installation Directory:" FontSize="12" FontWeight="SemiBold" Foreground="#A6ADC8" Margin="0,0,0,6"/>
            <TextBox Name="TxtPath" Text="C:\Program Files\GIN-PDF-RW" Background="#313244" Foreground="#CDD6F4" BorderThickness="0" Padding="8,6" FontSize="12" Margin="0,0,0,14"/>

            <TextBlock Name="LblLang" Text="Language:" FontSize="12" FontWeight="SemiBold" Foreground="#A6ADC8" Margin="0,0,0,6"/>
            <ComboBox Name="CmbLang" Width="200" HorizontalAlignment="Left" Background="#313244" Foreground="#CDD6F4" Padding="6,4" Margin="0,0,0,16">
                <ComboBoxItem Content="English (en)"/>
                <ComboBoxItem Content="Русский (ru)"/>
                <ComboBoxItem Content="Čeština (cs)"/>
            </ComboBox>

            <CheckBox Name="ChkAssociate" Content="Associate with .PDF files" Foreground="#CDD6F4" FontSize="12" IsChecked="True" Margin="0,0,0,8"/>
            <CheckBox Name="ChkDesktop" Content="Create Desktop shortcut" Foreground="#CDD6F4" FontSize="12" IsChecked="True"/>
        </StackPanel>

        <!-- Action Buttons -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,16,0,0">
            <Button Name="BtnCancel" Content="Cancel" Width="100" Height="36" Background="#313244" Foreground="#CDD6F4" FontWeight="SemiBold" Margin="0,0,10,0"/>
            <Button Name="BtnInstall" Content="INSTALL NOW" Width="140" Height="36" Background="#89B4FA" Foreground="#11111B" FontWeight="Bold"/>
        </StackPanel>
    </Grid>
</Window>
"@

$reader = New-Object System.Xml.XmlNodeReader $xaml
$win = [System.Windows.Markup.XamlReader]::Load($reader)

$txtHeader    = $win.FindName("TxtHeader")
$txtSub       = $win.FindName("TxtSub")
$lblPath      = $win.FindName("LblPath")
$txtPath      = $win.FindName("TxtPath")
$lblLang      = $win.FindName("LblLang")
$cmbLang      = $win.FindName("CmbLang")
$chkAssociate = $win.FindName("ChkAssociate")
$chkDesktop   = $win.FindName("ChkDesktop")
$btnCancel    = $win.FindName("BtnCancel")
$btnInstall   = $win.FindName("BtnInstall")

function UpdateStrings() {
    $win.Title = T "Title"
    $txtHeader.Text = T "Header"
    $txtSub.Text = T "Sub"
    $lblPath.Text = T "PathLabel"
    $lblLang.Text = T "LangLabel"
    $chkAssociate.Content = T "Associate"
    $chkDesktop.Content = T "DesktopLnk"
    $btnInstall.Content = T "BtnInstall"
    $btnCancel.Content = T "BtnCancel"
}

if ($script:CurLang -eq "ru") { $cmbLang.SelectedIndex = 1 }
elseif ($script:CurLang -eq "cs") { $cmbLang.SelectedIndex = 2 }
else { $cmbLang.SelectedIndex = 0 }

UpdateStrings()

$cmbLang.Add_SelectionChanged({
    if ($cmbLang.SelectedIndex -eq 1) { $script:CurLang = "ru" }
    elseif ($cmbLang.SelectedIndex -eq 2) { $script:CurLang = "cs" }
    else { $script:CurLang = "en" }
    UpdateStrings()
})

$btnCancel.Add_Click({ $win.Close() })

$btnInstall.Add_Click({
    $targetDir = $txtPath.Text.Trim()
    if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }

    $scriptDir = Split-Path -Parent $env:GIN_SETUP_SCRIPT
    if (-not $scriptDir) { $scriptDir = [System.AppDomain]::CurrentDomain.BaseDirectory }

    # Copy files
    Copy-Item "$scriptDir\GIN-PDF-RW.cmd" "$targetDir\GIN-PDF-RW.cmd" -Force
    if (Test-Path "$scriptDir\..\version.json") { Copy-Item "$scriptDir\..\version.json" "$targetDir\version.json" -Force }

    # Create Shortcuts
    if ($chkDesktop.IsChecked) {
        $wsh = New-Object -ComObject WScript.Shell
        $desktopPaths = @("D:\MEGA\DOCS\desktop", "$env:USERPROFILE\Desktop", "$env:PUBLIC\Desktop")
        foreach ($dp in $desktopPaths) {
            if (Test-Path $dp) {
                try {
                    $lnk = $wsh.CreateShortcut("$dp\GIN-PDF-RW.lnk")
                    $lnk.TargetPath = "$targetDir\GIN-PDF-RW.cmd"
                    $lnk.WorkingDirectory = $targetDir
                    $lnk.Description = "GIN-PDF-RW v002 by VladiMIR+AI"
                    $lnk.Save()
                } catch {}
            }
        }
    }

    # Associate .pdf
    if ($chkAssociate.IsChecked) {
        try {
            cmd /c "assoc .pdf=GIN-PDF-RW.Document >nul"
            cmd /c "ftype GIN-PDF-RW.Document=\"$targetDir\GIN-PDF-RW.cmd\" \"%%1\" >nul"
        } catch {}
    }

    [System.Windows.Forms.MessageBox]::Show((T "DoneMsg"), "GIN-PDF-RW Setup", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
    $win.Close()
})

$win.ShowDialog() | Out-Null
