# PowerShell script to create a branch, commit, push, and open a PR
# Usage: .\scripts\create_pr.ps1 -Branch feature/iterative-workflow-engine -Title 'Iteratively process executable tasks in WorkflowEngine'
param(
    [string]$Branch = 'feature/iterative-workflow-engine',
    [string]$Title = 'Iteratively process executable tasks in WorkflowEngine',
    [string]$BodyFile = 'docs/PRs/feature-iterative-workflow-engine.md',
    [string]$Remote = 'origin'
)

Set-StrictMode -Version Latest

# Stage changes if any
if ((git status --porcelain) -ne '') {
    Write-Host 'Staging changes...'
    git add .
}

# Create or switch branch
git checkout -B $Branch

# Commit
try {
    git commit -m $Title | Out-Null
} catch {
    Write-Host 'No changes to commit.'
}

# Push
git push -u $Remote $Branch

# Create PR using gh if available
if (Get-Command gh -ErrorAction SilentlyContinue) {
    gh pr create --title $Title --body-file $BodyFile --base main --head $Branch
} else {
    $repoUrl = git config --get remote.$Remote.url
    Write-Host "gh CLI not found. Create PR manually: $repoUrl/compare/main...$Branch?expand=1"
}

# Run tests if dotnet is available
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Write-Host 'Running dotnet test...'
    dotnet test
}

Write-Host "Done. Branch: $Branch"