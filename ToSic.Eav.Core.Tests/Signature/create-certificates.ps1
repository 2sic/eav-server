param ( $Path, $PfxPwd = "2sic2018")
# create the root CA
$RootCertParams = @{
    FriendlyName = "2sxc-ca"
    Subject = "CN=2sxc CA,O=2sic,OU=Development,C=CH,ST=StGallen,L=Buchs"
    DnsName = "2sxc CA"
    KeyLength = 2048
    KeyAlgorithm = 'RSA'
    HashAlgorithm = 'SHA256'
    KeyExportPolicy = 'Exportable'
    NotAfter = "2040-01-01"
    CertStoreLocation = 'Cert:\LocalMachine\My'
    Type = 'CodeSigningCert'
    KeySpec = 'Signature'
    KeyUsage = 'CertSign' #fixes invalid cert error
    Extension = [System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new($true, $true, 0, $true) #fixes invalid CA cert error for import in firefox
}
$RootCert = New-SelfSignedCertificate @RootCertParams

# export root cert pfx for backup
Export-PfxCertificate -Cert $RootCert  -FilePath (Join-Path $Path "2sxc-ca.pfx") -Password (ConvertTo-SecureString $PfxPwd -AsPlainText -Force)

# export certificate using Base64 .CER format
Set-Content -Path (Join-Path $Path "2sxc-ca_Base64.cer") -Value @([System.Convert]::ToBase64String($RootCert.Export([Security.Cryptography.X509Certificates.X509ContentType]::Pkcs12))) -Encoding Ascii

# export root cert so we can import it to Firefox
Export-Certificate -Cert $RootCert  -FilePath (Join-Path $Path "2sxc-ca_Public.crt")

# export certificate Public Key using Base64 .CER format
Set-Content -Path (Join-Path $Path "2sxc-ca_Base64_Public.cer") -Value @([System.Convert]::ToBase64String($RootCert.RawData)) -Encoding Ascii

