# Release and Rollback Procedures

## Release checklist

Before creating a release tag (`v*`), verify:

### Pre-release
- [ ] All unit tests pass (Domain 100%/100%, Application 80%/80%)
- [ ] All integration tests pass (Infrastructure 60%/60%)
- [ ] No critical or high vulnerabilities in `dotnet list package --vulnerable`
- [ ] No secrets detected by gitleaks
- [ ] CHANGELOG or release notes prepared
- [ ] Version number updated in relevant files (if applicable)

### Release execution
1. Create and push tag: `git tag v1.2.3 && git push origin v1.2.3`
2. CI automatically:
   - Builds signed Android APK
   - Publishes Web artifacts
   - Creates GitHub Release with artifacts and auto-generated release notes

### Post-release
- [ ] Verify APK installs and runs on test device
- [ ] Verify Web deployment (if applicable)
- [ ] Monitor logs for errors in first 24 hours
- [ ] Announce release (if applicable)

## Rollback procedure

### Android APK rollback
1. Identify the previous stable version tag (e.g., `v1.2.2`)
2. Download the APK from the previous GitHub Release
3. Uninstall current version on test device
4. Install previous APK
5. Verify functionality

### Web rollback
1. Identify the previous stable deployment
2. If using IIS/WebDeploy:
   - Stop the Web app
   - Restore previous deployment package
   - Start the Web app
   - Verify functionality
3. If using other hosting:
   - Follow provider-specific rollback procedure
   - Verify functionality

### Database rollback
- **No automated database migration system exists**
- If schema changes were introduced, manual intervention is required
- Always backup database before applying releases with schema changes

## Emergency contacts

- Repository owner: See GitHub profile for contact information
- For critical issues, create a GitHub Issue with label `bug` and `priority: critical`

## Release cadence

- No fixed release cadence
- Releases are created when significant features or fixes are ready
- Tag naming follows semantic versioning: `vMAJOR.MINOR.PATCH`
