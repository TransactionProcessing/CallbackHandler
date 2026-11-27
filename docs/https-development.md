# CallbackHandler development HTTPS

CallbackHandler serves HTTPS on its existing port 5010. HTTP is disabled. The HTTPS setup uses `TransactionProcessing.CertificateDefaults`; certificate and hostname validation remain enabled.

## Configure the listener

Copy `CallbackHandler/hosting.development.example.json` to `CallbackHandler/hosting.development.json` for local development, or provide the equivalent settings through the deployment configuration provider. The development file is intentionally git-ignored. Mount the CA-issued certificate and private key as read-only files accessible only to the service identity. Do not commit certificates, private keys, or passwords.

For a PEM certificate, `Path` must identify the full-chain PEM file and `KeyPath` its private key. For a password-protected PFX, use `Path` and provide `Password` through the deployment secret provider. The certificate must include the DNS name callers use in its Subject Alternative Name. Keep the CA root in the caller's normal trust store; do not disable certificate validation.

The existing `/health` and `/healthui` endpoints are available on HTTPS. Configure deployment readiness/liveness probes to use `https://<service-dns-name>:5010/health` and trust the private CA root. There are no deployment manifests in this repository, so the consuming deployment must apply the HTTPS probe URL and mount/inject the certificate settings.

## Verify TLS

From a machine with the private CA root installed (or pass its public root certificate explicitly):

```powershell
curl.exe --cacert <ca-root.pem> https://<service-dns-name>:5010/health
```

The request should complete without a TLS validation error. Repeat against an untrusted root and against a DNS name absent from the certificate SAN; both TLS handshakes must fail. Do not use `-k` or a custom certificate-validation callback for these checks.
