#!/usr/bin/env pwsh
$ErrorActionPreference = 'Stop'

# Script to create a branch, commit CI files, push, and create/update a PR
# Ensures the milestone exists (creates it if missing)

$branch = "ci/azure-pipelines-github-actions"
$commitMsg = "Add CI/CD: Azure Pipelines and GitHub Actions for Azure Web App deployment"
$prTitle = "CI/CD: Azure Pipelines + GitHub Actions for Azure Web App"
$milestone = "Tests passing and CI/CD yaml created"

git checkout -b $branch
# add files
git add azure-pipelines/azure-pipelines.yml .github/workflows/azure-webapp-deploy.yml
git commit -m $commitMsg
git push -u origin $branch

# get changed files in the commit
$changed = git show --name-only --pretty="" HEAD | Where-Object { $_ -ne '' } | ForEach-Object { " - $_" } | Out-String

$prBody = @"
This PR adds:

- azure-pipelines/azure-pipelines.yml
- .github/workflows/azure-webapp-deploy.yml

Changed files:
$changed

Please replace placeholders (<your-app-service-name>, <Azure Service Connection Name>) and add the AZURE_CREDENTIALS secret for GitHub Actions.
"@

# ensure milestone exists (create if missing)
try {
    gh milestone view $milestone | Out-Null
} catch {
    Write-Host "Milestone '$milestone' not found, creating..."
    gh milestone create --title $milestone
}

# create or update PR associated with current branch and assign milestone
try {
    gh pr view --json url -q .url | Out-Null
    gh pr edit --title $prTitle --body $prBody --milestone $milestone
} catch {
    gh pr create --title $prTitle --body $prBody --label "ci" --milestone $milestone
}

Write-Host "PR created/updated. If assigning the milestone failed, ensure you have permission to create milestones in the repository."