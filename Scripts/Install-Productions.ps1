<#
.DESCRIPTION
  Downloads latest version of the Production.SDK app and installs it in the application library.

.PARAMETER RelativityHost
  Url of the Relativity instance.
.PARAMETER RelativityUsername
  Username.
.PARAMETER RelativityPassword
  Password.  
#>


param(
	[Parameter(Mandatory)]
	[ValidateNotNullOrEmpty()]
	[string]$RelativityHost,
	
	[Parameter(Mandatory)]
	[ValidateNotNullOrEmpty()]
	[string]$RelativityUsername,
	
	[Parameter(Mandatory)]
	[ValidateNotNullOrEmpty()]
	[string]$RelativityPassword
	
)

Write-Host "RelativityHost: $RelativityHost"

[securestring]$securePassword = ConvertTo-SecureString -String $RelativityPassword -AsPlainText -Force
[pscredential]$credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $RelativityUsername, $securePassword

try{
	$rapPath = "\\bld-pkgs\Packages\Productions\master\14.1.9\Relativity.Productions.rap"

	if(-not (Get-Module -ListAvailable -Name RAPTools))
	{
		Install-Module RAPTools -Force
	}
	Import-Module RAPTools -Force
	Install-RelativityLibraryApplication -HostName "$RelativityHost" -FilePath "$rapPath" -RelativityCredential $credential
}
finally{
	Write-Output "Production install ended"
}